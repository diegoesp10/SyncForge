namespace Contracts.Orders;

public sealed record OrderResponse(
    int Id,
    int ImportJobId,
    string SourceSystem,
    string ExternalId,
    string CustomerName,
    decimal Amount,
    string Currency,
    DateTime CreatedAt);
