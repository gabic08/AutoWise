using MediatR;

namespace AutoWise.CommonUtilities.MediatRCqrsAbstractions;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse> where TQuery : IQuery<TResponse> where TResponse : notnull;

