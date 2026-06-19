using MediatR;
using System.Transactions;

namespace Tesseract.Application.Common.Transactions;

public class TransactionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ITransactional
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var options = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted,
        };

        using var scope = new TransactionScope(TransactionScopeOption.Required,
            options, TransactionScopeAsyncFlowOption.Enabled);

        var response = await next(cancellationToken);

        scope.Complete();

        return response;
    }
}