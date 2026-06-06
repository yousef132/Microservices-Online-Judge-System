using Community.API.Common.DTOs;
using MediatR;

namespace Community.API.Features.Votes.CastVote;

public record CastVoteCommand(Guid ArticleId, Guid? CommentId, int Value) : IRequest<CastVoteResponse>;
