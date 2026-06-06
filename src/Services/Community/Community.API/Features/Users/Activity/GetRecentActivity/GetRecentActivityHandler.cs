using Community.API.Entities;
using Community.API.Persistence;
using MediatR;

namespace Community.API.Features.Users.Activity.GetRecentActivity;

public class GetRecentActivityHandler(IUserActivityLogRepository userActivityLogRepository)
    : IRequestHandler<GetRecentActivityQuery, IEnumerable<UserActivity>>
{
    public async Task<IEnumerable<UserActivity>> Handle(GetRecentActivityQuery request, CancellationToken cancellationToken)
    {
        return await userActivityLogRepository.GetRecentActivityForUserAsync(request.UserId, request.Limit);
    }
}
