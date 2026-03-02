using AutoMapper;
using CoreJudge.Application.Features.Contests.Queries.GetContestSubmissionsHistory;
using CoreJudge.Application.Features.Submissions.Queries.GetProblemSubmissions;
using CoreJudge.Application.Features.Submissions.Queries.GetSubmissionData;
using CoreJudge.Domain.Models;

namespace CoreJudge.Application.Mapping;

public class SubmissionProfile : Profile
{
    public SubmissionProfile()
    {
        CreateMap<Submission, GetSubmissionDataQueryResponse>();
        CreateMap<Submission, GetProblemSubmissionsResponse>()
            .ForMember(dest => dest.SubmitTime, opt => opt.MapFrom(src => src.SubmitTime))
            .ForMember(dest => dest.SubmitMemory, opt => opt.MapFrom(src => src.SubmitMemory))
            .ForMember(dest => dest.Result, opt => opt.MapFrom(src => src.Result))
            .ForMember(dest => dest.SubmissionDate, opt => opt.MapFrom(src => src.SubmissionDate))
            .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Language))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));


        CreateMap<Submission, GetContestSubmissionsQueryResponse>()
            .ForMember(dest => dest.ProblemName, opt => opt.MapFrom(src => src.Problem.Name))
            .ForMember(dest => dest.Time, opt => opt.MapFrom(src => src.SubmitTime))
            .ForMember(dest => dest.Result, opt => opt.MapFrom(src => src.Result))
            .ForMember(dest => dest.SubmissionDate, opt => opt.MapFrom(src => src.SubmissionDate))
            .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Language));
    }
}