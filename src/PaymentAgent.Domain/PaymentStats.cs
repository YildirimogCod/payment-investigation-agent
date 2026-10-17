namespace PaymentAgent.Domain;

public record PaymentStats(
    int Total,
    int Failed,
    IReadOnlyDictionary<string, int> FailedByErrorCode);