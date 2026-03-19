namespace VaultAutoUnseal.Worker;

internal sealed class UnsealResponse
{
    public bool Sealed { get; set; }
    public int Progress { get; set; }
}
