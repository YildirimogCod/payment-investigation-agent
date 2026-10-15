namespace PaymentAgent.Domain;
public static class PaymentErrorCodes
{
    public const string ThreeDsTimeout = "3DS_TIMEOUT";
    public const string ThreeDsAuthFailed = "3DS_AUTH_FAILED";
    public const string InsufficientFunds = "INSUFFICIENT_FUNDS";
    public const string IssuerDeclined = "ISSUER_DECLINED";
    public const string GatewayTimeout = "GATEWAY_TIMEOUT";
    public const string FraudSuspected = "FRAUD_SUSPECTED";
    public const string CardExpired = "CARD_EXPIRED";
}