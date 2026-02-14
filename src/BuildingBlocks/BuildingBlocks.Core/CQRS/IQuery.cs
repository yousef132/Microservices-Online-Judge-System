using MediatR;

namespace BuildingBlocks.Core.CQRS
{
    public interface IQuery<out TResponse> : IRequest<TResponse> where TResponse : notnull
    {

    }
    public interface IQuery : IRequest<Unit>
    {

    }

}
