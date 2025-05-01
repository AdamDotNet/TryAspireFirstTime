using Microsoft.Azure.Cosmos;

namespace TryAspireFirstTime.ApiService.Data.People
{
	public class PeopleDocumentClient : IPeopleDocumentClient
	{
		private readonly CosmosClient _cosmosClient;
		private readonly ILogger<PeopleDocumentClient> _logger;
		private readonly Database _database;
		private readonly Container _container;

		public PeopleDocumentClient(CosmosClient cosmosClient, ILogger<PeopleDocumentClient> logger)
		{
			_cosmosClient = cosmosClient;
			_logger = logger;

			_database = _cosmosClient.GetDatabase("AspireDemo");
			_container = _database.GetContainer("people");
		}

		public async Task InitializeAsync()
		{
			_logger.LogInformation("Initializing Cosmos DB client...");
			await _cosmosClient.CreateDatabaseIfNotExistsAsync("AspireDemo");
			await _database.CreateContainerIfNotExistsAsync(new ContainerProperties("people", "/id"));
			_logger.LogInformation("Cosmos DB client initialized.");
		}

		public async Task<IEnumerable<Person>> GetPeopleAsync()
		{
			_logger.LogInformation("Retrieving all people from Cosmos DB...");
			var query = new QueryDefinition("SELECT * FROM c");
			var iterator = _container.GetItemQueryIterator<Person>(query);
			var results = new List<Person>();
			while (iterator.HasMoreResults)
			{
				var response = await iterator.ReadNextAsync();
				results.AddRange(response.Resource);
			}

			_logger.LogInformation($"Retrieved {results.Count} people from Cosmos DB.");
			return results;
		}

		public async Task<Person?> GetPersonAsync(string id)
		{
			try
			{
				_logger.LogInformation($"Retrieving person with ID {id} from Cosmos DB...");
				var response = await _container.ReadItemAsync<Person>(id, new PartitionKey(id));
				_logger.LogInformation($"Retrieved person with ID {id} {response is not null} from Cosmos DB.");
				return response!.Resource;
			}
			catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				return null;
			}
		}

		public async Task<Person> UpsertPersonAsync(Person person)
		{
			_logger.LogInformation($"Upserting person with ID {person.Id} into Cosmos DB...");
			var result = await _container.UpsertItemAsync(person, new PartitionKey(person.Id));
			_logger.LogInformation($"Upserted person with ID {person.Id} into Cosmos DB.");
			return result.Resource;
		}

		public async Task DeletePersonAsync(string id)
		{
			_logger.LogInformation($"Deleting person with ID {id} from Cosmos DB...");
			await _container.DeleteItemStreamAsync(id, new PartitionKey(id));
			_logger.LogInformation($"Deleted person with ID {id} from Cosmos DB.");
		}

		public async Task DeleteAllPeopleAsync()
		{
			_logger.LogInformation("Deleting all people from Cosmos DB...");
			var people = await GetPeopleAsync();
			foreach (var person in people)
			{
				await DeletePersonAsync(person.Id);
			}
			_logger.LogInformation("Deleted all people from Cosmos DB.");
		}
	}
}
