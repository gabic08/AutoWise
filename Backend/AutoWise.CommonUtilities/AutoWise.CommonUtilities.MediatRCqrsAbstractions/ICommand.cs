using MediatR;

namespace AutoWise.CommonUtilities.MediatRCqrsAbstractions;

// Unit is a MediatR placeholder return type used when the command doesn't return a value
public interface ICommand : ICommand<Unit>
{
}

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
