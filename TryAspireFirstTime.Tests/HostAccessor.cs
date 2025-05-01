using Aspire.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace TryAspireFirstTime.Tests
{
	public static class HostAccessor
	{
		public static async Task<DistributedApplication> GetAppAsync(ITestOutputHelper testOutputHelper)
		{
			// Arrange
			var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.TryAspireFirstTime_AppHost>(args: ["--environment=Automation"]);
			appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
			{
				clientBuilder.AddStandardResilienceHandler();
			});
			appHost.Services.AddLogging(loggingBuilder =>
			{
				loggingBuilder.AddXUnit(testOutputHelper);
			});

			var app = await appHost.BuildAsync();
			var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
			await app.StartAsync();
			await resourceNotificationService.WaitForResourceAsync("apiservice", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));

			return app;
		}
	}
}
