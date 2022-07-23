using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Activities;
using Microsoft.AspNetCore.Authorization;
using Activities;
using Application.Core;
using System.Text.Json;

namespace API.Controllers;

public class ActivitiesController : BaseApiController
{
	[HttpGet]
	public async Task<IActionResult> GetActivities(CancellationToken ct, [FromQuery] ActivityParams param) {
		return HandlePagedResult(await Mediator.Send(new List.Query { Params = param }, ct));
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetActivity(Guid id) {
		return HandleResult(await Mediator.Send(new Details.Query { Id = id }));
	}

	[HttpPost]
	public async Task<IActionResult> CreateActivity(Activity activity) {
		return HandleResult(await Mediator.Send(new Create.Command { Activity = activity }));
	}

	[Authorize(Policy = "IsActivityHost")]
	[HttpPut("{id}")]
	public async Task<IActionResult> EditActivity(Guid id, Activity activity) {
		activity.Id = id;
		return HandleResult(await Mediator.Send(new Edit.Command { Activity = activity }));
	}

	[Authorize(Policy = "IsActivityHost")]
	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteActivity(Guid id) {
		return HandleResult(await Mediator.Send(new Delete.Command { Id = id }));
	}

	[HttpPost("{id}/attend")]
	public async Task<IActionResult> Attend(Guid id) {
		return HandleResult(await Mediator.Send(new UpdateAttendance.Command { Id = id }));
	}
}
