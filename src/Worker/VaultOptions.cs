namespace VaultAutoUnseal.Worker;

public sealed class VaultOptions
{
    public const string SECTION_NAME = "Vault";
    public string Address { get; set; } = "http://127.0.0.1:8200";
    public string UnsealKey { get; set; } = string.Empty;
    public int StartupDelaySeconds { get; set; } = 5;
    public int MaxAttempts { get; set; } = 60;
    public int DelayBetweenAttemptsSeconds { get; set; } = 2;
}
