namespace VaultAutoUnseal.Worker;

using System.Net.Http.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public sealed class VaultAutoUnsealWorker : BackgroundService
{
    private readonly HttpClient httpClient;
    private readonly ILogger<VaultAutoUnsealWorker> logger;
    private readonly VaultOptions vaultOptions;

    public VaultAutoUnsealWorker(HttpClient httpClient, IOptions<VaultOptions> vaultOptions, ILogger<VaultAutoUnsealWorker> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;
        this.vaultOptions = vaultOptions.Value;

        this.httpClient.BaseAddress = new Uri(this.vaultOptions.Address);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(vaultOptions.UnsealKey))
        {
            logger.LogError("Vault unseal key is missing. Configure Vault:UnsealKey.");

            return;
        }

        logger.LogInformation("Vault auto-unseal worker started. Address: {VaultAddress}", vaultOptions.Address);

        if (vaultOptions.StartupDelaySeconds > 0)
        {
            await Task.Delay(TimeSpan.FromSeconds(vaultOptions.StartupDelaySeconds), stoppingToken);
        }

        for (var attemptNumber = 1; attemptNumber <= vaultOptions.MaxAttempts; attemptNumber++)
        {
            try
            {
                var isInitialized = await IsVaultInitializedAsync(stoppingToken);

                if (!isInitialized)
                {
                    logger.LogWarning("Vault is not initialized yet. Attempt {AttemptNumber}/{MaxAttempts}.", attemptNumber, vaultOptions.MaxAttempts);
                }
                else
                {
                    var sealStatus = await GetSealStatusAsync(stoppingToken);

                    if (!sealStatus.Sealed)
                    {
                        logger.LogInformation("Vault is already unsealed.");

                        return;
                    }

                    logger.LogInformation("Vault is sealed. Sending unseal request. Attempt {AttemptNumber}/{MaxAttempts}.", attemptNumber, vaultOptions.MaxAttempts);

                    var unsealResult = await UnsealAsync(stoppingToken);

                    if (!unsealResult.Sealed)
                    {
                        logger.LogInformation("Vault unsealed successfully.");

                        return;
                    }

                    logger.LogWarning("Vault still reports sealed=true after unseal request. Progress: {Progress}.", unsealResult.Progress);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Vault is not ready yet or unseal attempt failed. Attempt {AttemptNumber}/{MaxAttempts}.", attemptNumber, vaultOptions.MaxAttempts);
            }

            await Task.Delay(TimeSpan.FromSeconds(vaultOptions.DelayBetweenAttemptsSeconds), stoppingToken);
        }

        logger.LogError("Vault auto-unseal failed after all configured attempts.");
    }

    private async Task<bool> IsVaultInitializedAsync(CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync("/v1/sys/init", cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<InitStatusResponse>(cancellationToken: cancellationToken);

        return payload?.Initialized ?? false;
    }

    private async Task<SealStatusResponse> GetSealStatusAsync(CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync("/v1/sys/seal-status", cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<SealStatusResponse>(cancellationToken: cancellationToken);

        if (payload is null)
        {
            throw new InvalidOperationException("Vault seal-status response was empty.");
        }

        return payload;
    }

    private async Task<UnsealResponse> UnsealAsync(CancellationToken cancellationToken)
    {
        var request = new UnsealRequest
        {
            Key = vaultOptions.UnsealKey
        };

        using var response = await httpClient.PostAsJsonAsync("/v1/sys/unseal", request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<UnsealResponse>(cancellationToken: cancellationToken);

        if (payload is null)
        {
            throw new InvalidOperationException("Vault unseal response was empty.");
        }

        return payload;
    }
}
