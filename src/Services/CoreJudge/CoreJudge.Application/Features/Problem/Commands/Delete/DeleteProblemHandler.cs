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

namespace CoreJudge.Application.Features.Problems.Commands.Delete
{
    public class DeleteProblemHandler :
        ICommandHandler<DeleteProblemCommand, Response>
    {

        private readonly IUnitOfWork _unitOfWork;

        public DeleteProblemHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Response> Handle(DeleteProblemCommand request, CancellationToken cancellationToken)
        {
            var problem = await _unitOfWork.Repository<Problem>().GetByIdAsync(request.Id);
            if (problem == null)
                return await Response.FailureAsync("Problem Not Found!!", System.Net.HttpStatusCode.NotFound);

            await _unitOfWork.Repository<Problem>().DeleteAsync(problem);
            await _unitOfWork.CompleteAsync();

            return await Response.SuccessAsync(null,"Problem Deleted Successfully", System.Net.HttpStatusCode.NoContent);
        }
    }
}
