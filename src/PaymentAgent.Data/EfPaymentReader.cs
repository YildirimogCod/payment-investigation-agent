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

    public async Task<PaymentStats> GetStatsAsync(DateTimeOffset since, CancellationToken cancellationToken = default)
    {
        var recent = _db.Payments
            .AsNoTracking()
            .Where(p => p.CreatedAt >= since);

        var total = await recent.CountAsync(cancellationToken);
        var failedIds = recent
        .Where(p => p.Status == PaymentStatus.Failed)
        .Select(p => p.Id);
        var failed = await failedIds.CountAsync(cancellationToken);
        var byCode = await _db.StatusEvents
       .AsNoTracking()
       .Where(e => e.Status == PaymentStatus.Failed && failedIds.Contains(e.PaymentId))
       .GroupBy(e => e.ErrorCode ?? "UNKNOWN")
       .Select(g => new { Code = g.Key, Count = g.Count() })
       .ToListAsync(cancellationToken);

        return new PaymentStats(total, failed, byCode.ToDictionary(x => x.Code, x => x.Count));
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