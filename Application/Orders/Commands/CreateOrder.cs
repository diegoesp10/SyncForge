using Application.Imports;
using Contracts.Orders;
using Domain.Imports.Enums;
using Domain.Orders;
using Domain.Resources;

namespace Application.Orders.Commands;

public sealed record CreateOrderCommand(
    int ImportJobId, string SourceSystem, string ExternalId,
    string CustomerName, decimal Amount, string Currency);

public sealed class CreateOrderHandler(IOrderRepository orders, IImportJobRepository importJobs)
{
    public async Task<OrderResponse> HandleAsync(CreateOrderCommand command, string language, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var order = new Order(command.ImportJobId, command.SourceSystem, command.ExternalId,
            command.CustomerName, command.Amount, command.Currency, language);

        var job = await importJobs.GetByIdAsync(command.ImportJobId, cancellationToken)
            ?? throw new KeyNotFoundException(ErrorMessages.Get(ErrorCode.ImportJobNotFound, language, command.ImportJobId));

        if (job.Status != ImportStatus.Processing)
            throw new InvalidOperationException(ErrorMessages.Get(ErrorCode.InvalidTransition, language,
                job.Id, ErrorMessages.Status(ImportStatus.Processing, language), ErrorMessages.Status(job.Status, language)));

        if (!string.Equals(order.SourceSystem, job.SourceSystem, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException(ErrorMessages.Get(ErrorCode.SourceSystemMismatch, language), nameof(command));

        if (await orders.GetBySourceAsync(order.SourceSystem, order.ExternalId, cancellationToken) is not null)
            throw new InvalidOperationException(ErrorMessages.Get(ErrorCode.OrderAlreadyExists, language, order.ExternalId, order.SourceSystem));

        await orders.AddAsync(order, cancellationToken);
        await orders.SaveChangesAsync(cancellationToken);
        return order.ToResponse();
    }
}
