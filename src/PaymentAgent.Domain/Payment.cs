namespace PaymentAgent.Domain;

public class Payment
{
    public Guid Id { get; set; }
    public required string OrderId { get; set; }
    public decimal Amount { get; set; }
    public required string Currency { get; set; }
    public PaymentStatus Status { get; set; }
    public required string CardBrand { get; set; }
    public required string CardLast4 { get; set; }
    public required string CustomerEmail { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}