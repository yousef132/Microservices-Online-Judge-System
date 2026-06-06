using Community.API.Persistence;
using MediatR;

namespace Community.API.Features.Users.Activity.GetRecentlyActiveUsers;

public class GetRecentlyActiveUsersHandler(IUserActivityLogRepository userActivityLogRepository)
    : IRequestHandler<GetRecentlyActiveUsersQuery, IEnumerable<Guid>>
{
    public async Task<IEnumerable<Guid>> Handle(GetRecentlyActiveUsersQuery request, CancellationToken cancellationToken)
    {
        return await userActivityLogRepository.GetRecentlyActiveUserIdsAsync(TimeSpan.FromMinutes(request.TimeSpanInMinutes));
    }
}
