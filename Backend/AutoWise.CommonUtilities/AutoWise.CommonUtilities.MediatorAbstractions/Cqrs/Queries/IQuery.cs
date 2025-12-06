using MediatR;

namespace AutoWise.CommonUtilities.MediatorAbstractions.Cqrs.Queries;

public interface IQuery<out TResponse> : IRequest<TResponse> where TResponse : notnull
{

}
