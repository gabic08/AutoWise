using MediatR;

namespace AutoWise.CommonUtilities.MediatRAbstractions.Cqrs.Queries;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse> where TQuery : IQuery<TResponse> where TResponse : notnull
{
}
