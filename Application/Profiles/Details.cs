using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Profiles;

public class Details
{
	public class Query : IRequest<Result<Application.Profiles.Profile>>
	{
		public string? Username { get; set; }
	}

	public class Handler : IRequestHandler<Query, Result<Application.Profiles.Profile>>
	{
		private readonly DataContext _context;
		private readonly IMapper _mapper;
		private readonly IUserAccessor _userAccessor;

		public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor) {
			_context = context;
			_mapper = mapper;
			_userAccessor = userAccessor;
		}

		public async Task<Result<Application.Profiles.Profile>> Handle(Query request, CancellationToken cancellationToken) {
			var user = await _context.Users
				.ProjectTo<Application.Profiles.Profile>(_mapper.ConfigurationProvider,
					new { currentUsername = _userAccessor.GetUsername() })
				.SingleOrDefaultAsync(x => x.Username == request.Username);

			if (user == null) return null;

			return Result<Application.Profiles.Profile>.Success(user);
		}
	}
}