using BuildingBlocks.Core.CQRS;
using CoreJudge.Domain.Premitives;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreJudge.Application.Features.Topics.Commands
{
    public sealed record CreateTopicCommand(
        string Name
    ) : ICommand<Response>;
}
