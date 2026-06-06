using Community.API.Entities;
using MediatR;

namespace Community.API.Features.Users.Activity.GetRecentActivity;

public record GetRecentActivityQuery(Guid UserId, int Limit) : IRequest<IEnumerable<UserActivity>>;
