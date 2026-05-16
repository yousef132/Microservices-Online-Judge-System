using Community.API.Common.DTOs;
using MediatR;

namespace Community.API.Features.Votes.CastVote;

public record CastVoteCommand(Guid TargetId, string TargetType, int Value) : IRequest<CastVoteResponse>;
