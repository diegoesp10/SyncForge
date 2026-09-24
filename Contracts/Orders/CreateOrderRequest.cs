namespace Contracts.Orders;

public sealed record CreateOrderRequest(
    int ImportJobId,
    string SourceSystem,
    string ExternalId,
    string CustomerName,
    decimal Amount,
    string Currency);
