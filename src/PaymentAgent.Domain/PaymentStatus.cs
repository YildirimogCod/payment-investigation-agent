namespace PaymentAgent.Domain;

public enum PaymentStatus
{
    Created,
    PendingAuthentication,
    Authorized,
    Captured,
    Failed
}