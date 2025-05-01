#pragma warning disable ASPIRECOSMOSDB001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

var builder = DistributedApplication.CreateBuilder(args);

var cosmosDb = builder.AddAzureCosmosDB("cosmos")
    .RunAsPreviewEmulator(configure =>
    {
        // Get the web UX to see data.
        configure.WithDataExplorer();
		// Keep the data persistent between container restarts.
		configure.WithDataVolume("AspireFirstTryCosmos");
		// Keep the container running even when Aspire stops to speed up startup time.
		configure.WithLifetime(ContainerLifetime.Persistent);
    });

var apiService = builder.AddProject<Projects.TryAspireFirstTime_ApiService>("apiservice")
    .WithExternalHttpEndpoints()
    .WithReference(cosmosDb)
    .WaitFor(cosmosDb)
    // Technically works, but it doesn't show the response in the UI, so that's not the right way to use HttpCommands.
    .WithHttpCommand("/people", "Get People", commandOptions: new HttpCommandOptions
    {
        Method = HttpMethod.Get,
		// Just fooling around. You could read the response to determine if the action was successful.
		GetCommandResult = async context =>
        {
            var response = await context.Response.Content.ReadAsStringAsync();

            return new ExecuteCommandResult
            {
                Success = true
			};
        }
	});

var built = builder.Build();
built.Run();
