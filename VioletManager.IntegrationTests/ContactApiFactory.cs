using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VioletManager.Application;
using VioletManager.IntegrationTests.Fakes;

namespace VioletManager.IntegrationTests;

/// <summary>
/// Boots the real API pipeline with an in-memory contact repository.
/// </summary>
public sealed class ContactApiFactory : WebApplicationFactory<Program>
{
    public FakeContactRepository ContactRepository { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IContactRepository>();
            services.AddSingleton<IContactRepository>(ContactRepository);
        });
    }
}
