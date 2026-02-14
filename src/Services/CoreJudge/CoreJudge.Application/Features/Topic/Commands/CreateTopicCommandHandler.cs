using AutoMapper;
using CodeSphere.Domain.Abstractions;
using CoreJudge.Domain.Models;
using CoreJudge.Domain.Premitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreJudge.Application.Features.Topics.Commands
{
    public class CreateTopicCommandHandler : IRequestHandler<CreateTopicCommand, Response>
    {
        readonly IUnitOfWork _unitOfWork;
        readonly IMapper _mapper;

        public CreateTopicCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response> Handle(CreateTopicCommand request, CancellationToken cancellationToken)
        {
            var topic = await _unitOfWork.Repository<Topic>().Where(t => t.Name == request.Name).FirstOrDefaultAsync() ; 
            if(topic is not null)
                throw new Exception("Topic with the same name already exists !!");
            var newTopic = new Topic
            {
                Name = request.Name
            };

            await _unitOfWork.Repository<Topic>().AddAsync(newTopic);
            await _unitOfWork.CompleteAsync();

            var mappedTopic = _mapper.Map<CreateTopicCommandResponse>(newTopic); 

            return await Response.SuccessAsync(mappedTopic, "Topic Created Successfully !!");
        }
    }
}
