using System.Linq;
using System.Security.Claims;
using API.DTOs;
using API.Services;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
	private readonly UserManager<AppUser> _userManager;
	private readonly SignInManager<AppUser> _signInManager;
	private readonly TokenService _tokenService;
	private readonly IConfiguration _config;
	private readonly HttpClient _httpClient;

	public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, TokenService tokenService, IConfiguration config) {
		_userManager = userManager;
		_signInManager = signInManager;
		_tokenService = tokenService;
		_config = config;
		_httpClient = new HttpClient
		{
			BaseAddress = new System.Uri("https://graph.facebook.com")
		};
	}

	[AllowAnonymous]
	[HttpPost("login")]
	public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDto) {
		var user = await _userManager.Users.Include(p => p.Photos)
			.FirstOrDefaultAsync(x => x.Email == loginDto.Email);

		if (user == null) return Unauthorized();

		var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

		if (result.Succeeded) {
			await SetRefreshToken(user);
			return CreateUserObject(user);
		}
		return Unauthorized();
	}

	[AllowAnonymous]
	[HttpPost("register")]
	public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDto) {
		if (await _userManager.Users.AnyAsync(x => x.Email == registerDto.Email)) {
			ModelState.AddModelError("email", "Email taken");
			return ValidationProblem();
		}
		if (await _userManager.Users.AnyAsync(x => x.UserName == registerDto.Username)) {
			ModelState.AddModelError("username", "Username taken");
			return ValidationProblem();
		}

		var user = new AppUser
		{
			DisplayName = registerDto.DisplayName,
			Email = registerDto.Email,
			UserName = registerDto.Username,
			Bio = "something for now cause I don't want to drop my tables"
		};

		var result = await _userManager.CreateAsync(user, registerDto.Password);

		if (result.Succeeded) {
			await SetRefreshToken(user);
			return CreateUserObject(user);
		}

		return BadRequest("Problem registering user");
	}

	[Authorize]
	[HttpGet]
	public async Task<ActionResult<UserDTO>> GetCurrentUser() {
		var user = await _userManager.Users.Include(p => p.Photos)
			.FirstOrDefaultAsync(x => x.Email == User.FindFirstValue(ClaimTypes.Email));

		if (user is null)
			return new UserDTO { };

		await SetRefreshToken(user);

		return CreateUserObject(user);
	}

	[AllowAnonymous]
	[HttpPost("fbLogin")]
	public async Task<ActionResult<UserDTO>> FacebookLogin(string accessToken) {
		var fbVerifyKeys = _config["Facebook:AppId"] + "|" + _config["Facebook:AppSecret"];

		var verifyToken = await _httpClient.GetAsync($"debug_token?input_token={accessToken}&access_token={fbVerifyKeys}");
		if (!verifyToken.IsSuccessStatusCode) {
			return Unauthorized();
		}

		var fbUrl = $"me?access_token={accessToken}&fields=name,email,picture.width(100).height(100)";

		var response = await _httpClient.GetAsync(fbUrl);
		if (!response.IsSuccessStatusCode) {
			return Unauthorized();
		}

		var fbInfo = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

		var username = (string)fbInfo.id;
		var user = await _userManager.Users.Include(p => p.Photos).FirstOrDefaultAsync(x => x.UserName == username);
		if (user != null) {
			return CreateUserObject(user);
		}

		user = new AppUser
		{
			DisplayName = (string)fbInfo.name,
			Email = (string)fbInfo.email,
			UserName = (string)fbInfo.id,
			Photos = new List<Photo> {
				new Photo {
					Id = "fb_" + (string)fbInfo.id,
					Url = (string)fbInfo.picture.data.url,
					IsMain = true
				}
			}
		};

		var result = await _userManager.CreateAsync(user);

		if (!result.Succeeded) return BadRequest("Problem creating a user account");

		await SetRefreshToken(user);

		return CreateUserObject(user);
	}

	[Authorize]
	[HttpPost("refreshToken")]
	public async Task<ActionResult<UserDTO>> RefreshToken()
	{
		var refreshToken = Request.Cookies["refreshToken"];
		var user = await _userManager.Users
			.Include(r => r.RefreshTokens)
			.Include(p => p.Photos)
			.FirstOrDefaultAsync(x => x.UserName == User.FindFirstValue(ClaimTypes.Name));
		
		if(user == null) return Unauthorized();

		var oldToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken);

		if(oldToken is not null && !oldToken.IsActive) return Unauthorized();

		return CreateUserObject(user);
	}

	private async Task SetRefreshToken(AppUser user) {
		var refreshToken = _tokenService.GenerateRefreshToken();

		user.RefreshTokens.Add(refreshToken);
		await _userManager.UpdateAsync(user);

		var cookieOptions = new CookieOptions
		{
			HttpOnly = true,
			Expires = DateTime.UtcNow.AddDays(7)
		};

		Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
	}

	private UserDTO CreateUserObject(AppUser user) {
		return new UserDTO
		{
			DisplayName = user.DisplayName!,
			Image = user.Photos?.FirstOrDefault(x => x.IsMain)?.Url!,
			Token = _tokenService.CreateToken(user),
			Username = user.UserName
		};
	}
}