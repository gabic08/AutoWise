using MediatR;

namespace AutoWise.CommonUtilities.MediatRCqrsAbstractions;

public interface IQuery<out TResponse> : IRequest<TResponse> where TResponse : notnull;
