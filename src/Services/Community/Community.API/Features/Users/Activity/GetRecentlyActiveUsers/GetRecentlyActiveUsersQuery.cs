using MediatR;

namespace Community.API.Features.Users.Activity.GetRecentlyActiveUsers;

public record GetRecentlyActiveUsersQuery(int TimeSpanInMinutes) : IRequest<IEnumerable<Guid>>;
