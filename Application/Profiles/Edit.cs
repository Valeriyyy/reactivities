using System.ComponentModel;
using Application.Core;
using AutoMapper;
using FluentValidation;
using Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Profiles;
public class Edit
{
	public class Command : IRequest<Result<Unit>>
	{
		public string DisplayName { get; set; }
		public string Bio { get; set; }
	}

	public class CommandValidator : AbstractValidator<Command>
	{
		public CommandValidator() {
			RuleFor(DisplayName => DisplayName).NotEmpty();
		}
	}

	public class Handler : IRequestHandler<Command, Result<Unit>>
	{
		private readonly DataContext _context;
		private readonly IUserAccessor _userAccessor;

		public Handler(DataContext context, IUserAccessor userAccessor) {
			_context = context;
			_userAccessor = userAccessor;
		}

		public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken) {
			var user = await _context.Users.FirstOrDefaultAsync(user =>
				user.UserName == _userAccessor.GetUsername());

			if (user == null) {
				return null;
			}

			user.Bio = request.Bio ?? user.Bio;
			user.DisplayName = request.DisplayName ?? user.DisplayName;

			_context.Entry(user).State = EntityState.Modified;
        
			var success = await _context.SaveChangesAsync() > 0;

			if (success) {
				return Result<Unit>.Success(Unit.Value);
			}

			return Result<Unit>.Failure("Problem updating profile");
		}
	}
}
