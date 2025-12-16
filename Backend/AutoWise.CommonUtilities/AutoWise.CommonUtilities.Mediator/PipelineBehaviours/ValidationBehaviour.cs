using AutoWise.CommonUtilities.Exceptions;
using AutoWise.CommonUtilities.Mediator.CqrsAbstractions;
using FluentValidation;
using MediatR;
using System.Text.Json;

namespace AutoWise.CommonUtilities.Mediator.PipelineBehaviours;

public class ValidationBehaviour<TRequest, TResponse> (IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse> where TRequest : ICommand<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(v => v.ToDictionary())
            .GroupBy(kvp => kvp.Key)
            .ToDictionary(
                g => g.Key,
                g => g.SelectMany(x => x.Value).ToArray()
            );

        if (failures.Count > 0)
        {
            string jsonResult = JsonSerializer.Serialize(failures);
            throw new BadRequestWithMultipleFailuresException(jsonResult);
        }

        return await next(cancellationToken);
    }
}
