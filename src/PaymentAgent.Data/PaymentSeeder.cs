using Microsoft.EntityFrameworkCore;
using PaymentAgent.Domain;
using static PaymentAgent.Domain.PaymentStatus;

namespace PaymentAgent.Data;

public static class PaymentSeeder
{
    private sealed record Step(int Seconds, PaymentStatus Status, string? ErrorCode = null);

    private static Step S(int seconds, PaymentStatus status, string? errorCode = null)
        => new(seconds, status, errorCode);

    public static async Task SeedAsync(PaymentDbContext db, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken);

        if (await db.Payments.AnyAsync(cancellationToken))
        {
            return;
        }

        var payments = new List<Payment>();
        var events = new List<PaymentStatusEvent>();
        var nextId = 1;

        void Add(string orderId, decimal amount, string brand, string last4, DateTimeOffset start, params Step[] steps)
        {
            var id = Guid.Parse($"00000000-0000-0000-0000-{nextId++:D12}");

            payments.Add(new Payment
            {
                Id = id,
                OrderId = orderId,
                Amount = amount,
                Currency = "TRY",
                Status = steps[^1].Status,
                CardBrand = brand,
                CardLast4 = last4,
                CustomerEmail = $"musteri.{orderId.ToLowerInvariant()}@example.com",
                CreatedAt = start
            });

            foreach (var step in steps)
            {
                events.Add(new PaymentStatusEvent
                {
                    PaymentId = id,
                    Status = step.Status,
                    ErrorCode = step.ErrorCode,
                    OccurredAt = start.AddSeconds(step.Seconds)
                });
            }
        }

        Add("ORD-1001", 249.90m, "Visa", "1111", now.AddHours(-30),
            S(0, Created), S(4, PendingAuthentication), S(19, Authorized), S(25, Captured));

        Add("ORD-1002", 1250.00m, "Mastercard", "2222", now.AddHours(-26),
            S(0, Created), S(3, PendingAuthentication), S(184, Failed, PaymentErrorCodes.ThreeDsTimeout));

        Add("ORD-1003", 3499.00m, "Visa", "3333", now.AddHours(-20),
            S(0, Created), S(2, Failed, PaymentErrorCodes.InsufficientFunds));

        Add("ORD-1004", 780.50m, "Mastercard", "4444", now.AddHours(-14),
            S(0, Created), S(2, Failed, PaymentErrorCodes.IssuerDeclined));

        Add("ORD-1005", 560.00m, "Visa", "5555", now.AddHours(-9),
            S(0, Created), S(3, PendingAuthentication), S(41, Failed, PaymentErrorCodes.ThreeDsAuthFailed));

        Add("ORD-1006", 199.99m, "Mastercard", "6666", now.AddHours(-5),
            S(0, Created), S(30, Failed, PaymentErrorCodes.GatewayTimeout));

        Add("ORD-1007", 8999.00m, "Visa", "7777", now.AddHours(-3),
            S(0, Created), S(5, Failed, PaymentErrorCodes.FraudSuspected));

        Add("ORD-1008", 430.00m, "Mastercard", "8888", now.AddHours(-2),
            S(0, Created), S(1, Failed, PaymentErrorCodes.CardExpired));

        Add("ORD-1009", 615.00m, "Visa", "9999", now.AddHours(-4),
            S(0, Created), S(3, PendingAuthentication));

        Add("ORD-1010", 2100.00m, "Mastercard", "1010", now.AddMinutes(-70),
            S(0, Created), S(4, PendingAuthentication), S(22, Authorized));

        for (var i = 0; i < 8; i++)
        {
            Add($"ORD-{2001 + i}", 150m + i * 40m, i % 2 == 0 ? "Visa" : "Mastercard", $"{2000 + i * 11:D4}",
                now.AddMinutes(-5 - i * 5),
                S(0, Created), S(3, PendingAuthentication), S(183, Failed, PaymentErrorCodes.ThreeDsTimeout));
        }

        Add("ORD-2101", 320.00m, "Visa", "2101", now.AddMinutes(-12),
            S(0, Created), S(4, PendingAuthentication), S(15, Authorized), S(21, Captured));

        Add("ORD-2102", 95.50m, "Mastercard", "2102", now.AddMinutes(-27),
            S(0, Created), S(3, PendingAuthentication), S(18, Authorized), S(23, Captured));

        for (var i = 0; i < 12; i++)
        {
            Add($"ORD-{3001 + i}", 100m + i * 37.5m, i % 2 == 0 ? "Visa" : "Mastercard", $"{3000 + i * 7:D4}",
                now.AddMinutes(-90 - i * 20),
                S(0, Created), S(4, PendingAuthentication), S(17, Authorized), S(24, Captured));
        }

        db.Payments.AddRange(payments);
        db.StatusEvents.AddRange(events);
        await db.SaveChangesAsync(cancellationToken);
    }
}