namespace VaultAutoUnseal.Worker;

internal sealed class SealStatusResponse
{
    public bool Sealed { get; set; }
    public int Progress { get; set; }
}
