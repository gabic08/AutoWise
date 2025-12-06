using MediatR;

namespace AutoWise.CommonUtilities.MediatRAbstractions.Cqrs.Queries;

public interface IQuery<out TResponse> : IRequest<TResponse> where TResponse : notnull
{

}
