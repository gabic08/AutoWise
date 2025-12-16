using MediatR;

namespace AutoWise.CommonUtilities.Mediator.CqrsAbstractions;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse> where TQuery : IQuery<TResponse> where TResponse : notnull;

