using Aspire.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using TryAspireFirstTime.ApiService.Data.People;
using Xunit.Abstractions;

namespace TryAspireFirstTime.Tests.Data
{
	/// <summary>
	/// Test just the document clients within the web api service against a Cosmos DB emulator.
	/// </summary>
	public class PeopleDocumentClientTests : IAsyncLifetime
	{
		private readonly ITestOutputHelper _testOutputHelper;
		private WebApplicationFactory<Program>? _factory;
		private DistributedApplication? _app;

		public PeopleDocumentClientTests(ITestOutputHelper testOutputHelper)
		{
			_testOutputHelper = testOutputHelper;
		}

		public async Task InitializeAsync()
		{
			// Use Aspire Host to start up Cosmos DB and get the connection string.
			_app = await HostAccessor.GetAppAsync(_testOutputHelper);
			var cosmos = await _app.GetConnectionStringAsync("cosmos");
			// Use the Test Application Factory to create a ServiceCollection that matches the web app verbatim.
			// We only have to replace the CosmosClient since the test app won't be fully configured to "just work".
			// This is because builder.ConfigureAppConfiguration() will execute AFTER Program.cs executes builder.AddAzureCosmosClient("cosmos") 😭
			// This is a problem because builder.AddAzureCosmosClient("cosmos") reads configuration the way it is immediately upon being called.
			// Otherwise, we would have just added the connection string to configuration and been fine.
			_factory = new WebApplicationFactory<Program>()
				.WithWebHostBuilder(builder =>
				{
					builder.UseEnvironment("Automation");

					builder.ConfigureLogging(logging =>
					{
						logging.AddXUnit(_testOutputHelper);
					});

					builder.ConfigureServices(services =>
					{
						var options = new CosmosClientOptions
						{
							ConnectionMode = ConnectionMode.Gateway,
							LimitToEndpoint = true,
							ApplicationName = "Aspire",
							CosmosClientTelemetryOptions =
							{
								DisableDistributedTracing = false
							}
						};
						services.AddSingleton(new CosmosClient(cosmos, options));
					});

					// NOTE: This doesn't work, see explanation above.
					/*builder.ConfigureAppConfiguration((context, config) =>
					{
						config.AddInMemoryCollection(new Dictionary<string, string?>
						{
							{ "cosmos:ConnectionString", cosmos }
						});
					});*/
				});
		}

		public async Task DisposeAsync()
		{
			if (_app is not null)
			{
				await _app.DisposeAsync();
			}

			if (_factory is not null)
			{
				await _factory.DisposeAsync();
			}
		}

		/// <summary>
		/// This document client is very simple, but in our real apps, we write complex SQL that we want to test directly.
		/// </summary>
		[Fact]
		public async Task CanCrudPeople()
		{
			// Arrange
			var documentClient = _factory!.Services.GetRequiredService<IPeopleDocumentClient>();
			await documentClient.DeleteAllPeopleAsync();

			// Act - Can Read Empty
			var expectEmptyPeople = await documentClient.GetPeopleAsync();
			Assert.Empty(expectEmptyPeople);

			// Act - Can Create
			var person = new Person
			{
				Id = Guid.NewGuid().ToString(),
				FirstName = "John",
				LastName = "Doe"
			};
			var createdPerson = await documentClient.UpsertPersonAsync(person);
			Assert.NotNull(createdPerson);
			Assert.Equal(person.Id, createdPerson.Id);
			Assert.Equal(person.FirstName, createdPerson.FirstName);
			Assert.Equal(person.LastName, createdPerson.LastName);

			// Act - Can Read Created
			var readPerson = await documentClient.GetPersonAsync(person.Id);
			Assert.NotNull(readPerson);
			Assert.Equal(person.Id, readPerson.Id);
			Assert.Equal(person.FirstName, readPerson.FirstName);
			Assert.Equal(person.LastName, readPerson.LastName);

			// Act - Can Read All
			var allPeople = await documentClient.GetPeopleAsync();
			Assert.NotNull(allPeople);
			var actualPersonFromAll = Assert.Single(allPeople);
			Assert.Equal(person.Id, actualPersonFromAll.Id);
			Assert.Equal(person.FirstName, actualPersonFromAll.FirstName);
			Assert.Equal(person.LastName, actualPersonFromAll.LastName);

			// Act - Can Update
			person.FirstName = "Jane";
			person.LastName = "Smith";
			var updatedPerson = await documentClient.UpsertPersonAsync(person);
			Assert.NotNull(updatedPerson);
			Assert.Equal(person.Id, updatedPerson.Id);
			Assert.Equal(person.FirstName, updatedPerson.FirstName);
			Assert.Equal(person.LastName, updatedPerson.LastName);

			// Act - Can Delete
			await documentClient.DeletePersonAsync(person.Id);
			var deletedPerson = await documentClient.GetPersonAsync(person.Id);
			Assert.Null(deletedPerson);
		}
	}
}
