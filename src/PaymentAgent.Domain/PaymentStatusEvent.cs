namespace PaymentAgent.Domain;

public class PaymentStatusEvent
{
    public int Id { get; set; }
    public Guid PaymentId { get; set; }
    public PaymentStatus Status { get; set; }
    public string? ErrorCode { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}