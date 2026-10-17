namespace PaymentAgent.Domain;

public interface IPaymentReader
{
    Task<Payment?> GetPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<Payment?> GetPaymentByOrderIdAsync(string orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentStatusEvent>> GetStatusHistoryAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> SearchFailedPaymentsAsync(DateTimeOffset since, int limit, CancellationToken cancellationToken = default);
    Task<PaymentStats> GetStatsAsync(DateTimeOffset since, CancellationToken cancellationToken = default);


}