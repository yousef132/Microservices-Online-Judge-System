using CoreJudge.Domain.Models.Entities;
using CoreJudge.Domain.Premitives;

namespace CoreJudge.Application.Features.Problem.Common;


public record ProblemResponse(
    int Id,
    string Name
);