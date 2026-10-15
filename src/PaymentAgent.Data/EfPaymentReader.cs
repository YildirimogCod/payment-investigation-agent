using Microsoft.EntityFrameworkCore;
using PaymentAgent.Domain;

namespace PaymentAgent.Data;

public class EfPaymentReader : IPaymentReader
{
    private readonly PaymentDbContext _db;
    public EfPaymentReader(PaymentDbContext db)
    {
        _db = db;
    }
    public Task<Payment?> GetPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        return _db.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == paymentId,cancellationToken);
    }

    public Task<Payment?> GetPaymentByOrderIdAsync(string orderId, CancellationToken cancellationToken = default)
    {
        return _db.Payments
           .AsNoTracking()
           .FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentStatusEvent>> GetStatusHistoryAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        return await _db.StatusEvents
             .AsNoTracking()
             .Where(e => e.PaymentId == paymentId)
             .OrderBy(e => e.OccurredAt)
             .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> SearchFailedPaymentsAsync(DateTimeOffset since, int limit, CancellationToken cancellationToken = default)
    {
        return await _db.Payments
            .AsNoTracking()
            .Where(p => p.Status == PaymentStatus.Failed && p.CreatedAt >= since)
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}