using Microsoft.EntityFrameworkCore;
using PaymentAgent.Data;
using PaymentAgent.Domain;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseSqlite("Data Source = payments.db")
            .Options;

        await using var db = new PaymentDbContext(options);

        var now = DateTimeOffset.UtcNow;
        await PaymentSeeder.SeedAsync(db, now);

        IPaymentReader reader = new EfPaymentReader(db);

        var failed = await reader.SearchFailedPaymentsAsync(now.AddHours(-1), 20);

        Console.WriteLine($"Son 1 saatte başarısız ödeme sayısı: {failed.Count}");

        foreach(var payment in failed)
        {
            Console.WriteLine($"{payment.OrderId} | {payment.Status} | {payment.CreatedAt}");
        }
        if (failed.Count > 0)
        {
            var history = await reader.GetStatusHistoryAsync(failed[0].Id);
            Console.WriteLine($"{failed[0].OrderId} durum geçmişi:");
            foreach (var e in history)
            {
                Console.WriteLine($"  {e.OccurredAt.ToLocalTime():HH:mm:ss}  {e.Status}  {e.ErrorCode}");
            }
        }
    }
}