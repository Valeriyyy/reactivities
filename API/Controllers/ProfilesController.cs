using API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Profiles;

namespace Controllers;

public class ProfilesController : BaseApiController
{
    [HttpGet("{username}")]
    public async Task<ActionResult<Profile>> GetProfile(string username)
    {
		return HandleResult(await Mediator.Send(new Details.Query { Username = username }));
	}
}