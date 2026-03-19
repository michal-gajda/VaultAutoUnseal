namespace VaultAutoUnseal.Worker;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddWindowsService(options =>
        {
            options.ServiceName = "Vault Auto Unseal";
        });

        builder.Services.Configure<VaultOptions>(builder.Configuration.GetSection(VaultOptions.SECTION_NAME));

        builder.Services.AddHttpClient<VaultAutoUnsealWorker>(httpClient =>
        {
            httpClient.Timeout = TimeSpan.FromSeconds(5);
        });

        builder.Services.AddHostedService<VaultAutoUnsealWorker>();

        using var host = builder.Build();
        await host.RunAsync();
    }
}
