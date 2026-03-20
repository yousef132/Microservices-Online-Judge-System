using BuildingBlocks.Core.CQRS;
using CodeSphere.Domain.Abstractions;
using CoreJudge.Domain.Models;
using CoreJudge.Domain.Premitives;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreJudge.Application.Features.Topics.Queries.GetAll
{
    internal class GetAllTopicsQueryHandler : IQueryHandler<GetAllTopicsQuery, Response>
    {
        readonly IUnitOfWork _unitOfWork;

        public GetAllTopicsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response> Handle(GetAllTopicsQuery request, CancellationToken cancellationToken)
        {
            var topics = await _unitOfWork.Repository<Topic>().GetAllAsync();

            if (topics == null)
                return await Response.FailureAsync("No Topics Found !!");

            return await Response.SuccessAsync(topics, "topics have been fetched successfully");
        }
    }
}
