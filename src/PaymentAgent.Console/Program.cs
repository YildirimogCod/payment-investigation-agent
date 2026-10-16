using Anthropic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using PaymentAgent.Data;
using PaymentAgent.Domain;

internal class Program
{
    private static async Task Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "hello")
        {
            await RunHelloAsync();
            return;
        }
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
    private static async Task RunHelloAsync()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var apiKey = config["Anthropic:ApiKey"]
            ?? throw new InvalidOperationException("Anthropic:ApiKey bulunamadı. user-secrets ile ekle.");

        AnthropicClient client = new() { ApiKey = apiKey };
        IChatClient chatClient = client.AsIChatClient("claude-haiku-5-5");

        var response = await chatClient.GetResponseAsync("Merhaba! Tek cümleyle kendini tanıt.");

        Console.WriteLine(response.Text);
        Console.WriteLine($"Token: giriş={response.Usage?.InputTokenCount}, çıkış={response.Usage?.OutputTokenCount}");
    }
}