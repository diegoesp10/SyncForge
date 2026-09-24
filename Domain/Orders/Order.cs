using Domain.Resources;

namespace Domain.Orders;

public sealed class Order
{
    private Order() { } // EF Core

    public Order(int importJobId, string sourceSystem, string externalId,
        string customerName, decimal amount, string currency, string language)
    {
        if (importJobId <= 0)
            throw new ArgumentOutOfRangeException(nameof(importJobId), ErrorMessages.Get(ErrorCode.InvalidImportJobId, language));
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), ErrorMessages.Get(ErrorCode.InvalidAmount, language));
        if (amount > 9999999999999999.99m || decimal.Round(amount, 2) != amount)
            throw new ArgumentOutOfRangeException(nameof(amount), ErrorMessages.Get(ErrorCode.InvalidAmountPrecision, language));

        var normalizedCurrency = DomainValidation.Required(currency, nameof(currency), 3, language).ToUpperInvariant();
        if (normalizedCurrency.Length != 3 || normalizedCurrency.Any(c => c is < 'A' or > 'Z'))
            throw new ArgumentException(ErrorMessages.Get(ErrorCode.InvalidCurrency, language), nameof(currency));

        ImportJobId = importJobId;
        SourceSystem = DomainValidation.Required(sourceSystem, nameof(sourceSystem), 200, language);
        ExternalId = DomainValidation.Required(externalId, nameof(externalId), 200, language);
        CustomerName = DomainValidation.Required(customerName, nameof(customerName), 200, language);
        Amount = amount;
        Currency = normalizedCurrency;
        CreatedAt = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public int ImportJobId { get; private set; }
    public string SourceSystem { get; private set; } = null!;
    public string ExternalId { get; private set; } = null!;
    public string CustomerName { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
}
