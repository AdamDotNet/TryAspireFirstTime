using Newtonsoft.Json;

namespace TryAspireFirstTime.ApiService.Data.People
{
	public class Person
	{
		[JsonProperty("id")]
		public required string Id { get; set; }

		[JsonProperty("firstName")]
		public required string FirstName { get; set; }

		[JsonProperty("middleName")]
		public string? MiddleName { get; set; }

		[JsonProperty("lastName")]
		public required string LastName { get; set; }
	}
}
