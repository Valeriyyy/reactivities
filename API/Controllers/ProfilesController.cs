using API.Controllers;
using Application.Profiles;
using Microsoft.AspNetCore.Mvc;
using Profiles;

namespace Controllers;

public class ProfilesController : BaseApiController
{
	// [HttpGet("{username}")]
	// public async Task<ActionResult<Application.Profiles.Profile>> GetProfile(string username) {
	// 	return HandleResult(await Mediator.Send(new Details.Query { Username = username }));
	// }

	[HttpGet("{username}")]
	public async Task<IActionResult> GetProfile(string username) {
		return HandleResult(await Mediator.Send(new Details.Query { Username = username }));
	}

	[HttpPut]
	public async Task<ActionResult> EditProfile(Edit.Command command) {
		return HandleResult(await Mediator.Send(new Edit.Command { DisplayName = command.DisplayName, Bio = command.Bio }));
	}

	[HttpGet("{username}/activities")]
	public async Task<IActionResult> GetUserActivities(string username, string predicate) {
		return HandleResult(await Mediator.Send(new ListActivities.Query { Username = username, Predicate = predicate }));
	}
}