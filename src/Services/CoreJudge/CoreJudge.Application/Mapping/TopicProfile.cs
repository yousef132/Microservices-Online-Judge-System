using AutoMapper;
using CoreJudge.Application.Features.Topics.Commands;
using CoreJudge.Domain.Models;

namespace CoreJudge.Application.Mapping;

public class TopicProfile : Profile
{
    public TopicProfile()
    {
        CreateMap<Topic, CreateTopicCommandResponse>();
    }
}