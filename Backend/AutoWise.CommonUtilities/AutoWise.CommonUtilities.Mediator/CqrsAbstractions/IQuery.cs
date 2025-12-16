using MediatR;

namespace AutoWise.CommonUtilities.Mediator.CqrsAbstractions;

public interface IQuery<out TResponse> : IRequest<TResponse> where TResponse : notnull;
