using AutoMapper;
using CoreJudge.Application.Features.Contests.Common;
using CoreJudge.Application.Features.Problem.Common;
using CoreJudge.Application.Features.Problems.Commands.Create;
using CoreJudge.Application.Features.Problems.Queries.GetAll;
using CoreJudge.Application.Features.Problems.Queries.GetById;
using CoreJudge.Application.Mapping.Resolvers;
using CoreJudge.Domain.Models;

namespace CoreJudge.Application.Mapping;

public class ProblemProfile : Profile
{
    public ProblemProfile()
    {
        CreateMap<CreateProblemCommand, Problem>();
        CreateMap<Problem, CreateProblemCommandResponse>();
        CreateMap<ProblemDocument, GetAllQueryResponse>()
            .ForMember(d => d.Topics, opt => opt.MapFrom<TopicResolver>());
        CreateMap<Problem, GetByIdQueryResponse>()
            .ForMember(d => d.TestCases, O => O.MapFrom(S => S.Testcases))
            .ForMember(d => d.Topics, O => O.MapFrom(S => S.ProblemTopics));

        CreateMap<ProblemTopic, TopicDto>()
            .ForMember(d => d.Id, O => O.MapFrom(S => S.Topic.Id))
            .ForMember(d => d.Name, O => O.MapFrom(S => S.Topic.Name));

        CreateMap<Testcase, TestCasesDto>()
            .ForMember(d => d.ExpectedOutput, O => O.MapFrom(S => S.Output));

        CreateMap<Problem, ContestProblemResponse>();

        CreateMap<Problem, ProblemResponse>();
    }
}