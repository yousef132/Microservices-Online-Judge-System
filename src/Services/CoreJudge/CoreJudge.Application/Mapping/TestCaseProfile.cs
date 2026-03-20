using AutoMapper;
using CoreJudge.Application.Features.TestCases.Commands.Create;
using CoreJudge.Application.Features.Testcases.Commands.Update;
using CoreJudge.Application.Features.Testcases.Queries.GetTestCasesByProblemId;
using CoreJudge.Domain.Models;

namespace CoreJudge.Application.Mapping;

public class TestCaseProfile : Profile
{
    public TestCaseProfile()
    {
        CreateMap<GetTestCasesByProblemIdQuerey, Testcase>();
        CreateMap<CreateTestcaseCommand, Testcase>();
        CreateMap<UpdateTestcaseCommand, Testcase>()
            .ForMember(dest => dest.Output, opt => opt.MapFrom(src => src.ExpectedOutput))
            .ForMember(dest => dest.Input, opt => opt.MapFrom(src => src.Input));
    }
    
}