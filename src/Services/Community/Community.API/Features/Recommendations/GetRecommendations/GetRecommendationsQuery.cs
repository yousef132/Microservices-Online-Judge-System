using Community.API.Entities;
using MediatR;

namespace Community.API.Features.Recommendations.GetRecommendations;

public record GetRecommendationsQuery(int Limit) : IRequest<IEnumerable<Article>>;
