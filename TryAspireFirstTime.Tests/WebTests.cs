using TryAspireFirstTime.ApiService.Data.People;
using Xunit.Abstractions;

namespace TryAspireFirstTime.Tests;

public class WebTests
{
	private readonly ITestOutputHelper _testOutputHelper;

	public WebTests(ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper;
	}

    [Fact]
    public async Task CanCrudPeopleOverApi()
    {
		// Arrange
		await using var app = await HostAccessor.GetAppAsync(_testOutputHelper);
		var httpClient = app.CreateHttpClient("apiservice");
		await httpClient.DeleteAsync("/people");

		// Act - Can Read Empty
		var response = await httpClient.GetAsync("/people");
		response.EnsureSuccessStatusCode();
		var emptyPeople = await response.Content.ReadAsAsync<List<Person>>();
		Assert.Empty(emptyPeople);

		// Act - Can Create
		var person = new Person
		{
			Id = Guid.NewGuid().ToString(),
			FirstName = "John",
			LastName = "Doe"
		};
		response = await httpClient.PutAsJsonAsync("/people", person);
		response.EnsureSuccessStatusCode();
		var createdPerson = await response.Content.ReadAsAsync<Person>();
		Assert.Equal(person.Id, createdPerson.Id);
		Assert.Equal(person.FirstName, createdPerson.FirstName);
		Assert.Equal(person.LastName, createdPerson.LastName);

		// Act - Can Read Created Person
		response = await httpClient.GetAsync($"/people/{person.Id}");
		response.EnsureSuccessStatusCode();
		var readPerson = await response.Content.ReadAsAsync<Person>();
		Assert.NotNull(readPerson);
		Assert.Equal(person.Id, readPerson.Id);
		Assert.Equal(person.FirstName, readPerson.FirstName);
		Assert.Equal(person.LastName, readPerson.LastName);

		// Act - Can Read All People
		response = await httpClient.GetAsync("/people");
		response.EnsureSuccessStatusCode();
		var allPeople = await response.Content.ReadAsAsync<List<Person>>();
		var actualPersonFromAll = Assert.Single(allPeople);
		Assert.Equal(person.Id, actualPersonFromAll.Id);
		Assert.Equal(person.FirstName, actualPersonFromAll.FirstName);
		Assert.Equal(person.LastName, actualPersonFromAll.LastName);

		// Act - Can Update
		person.FirstName = "Jane";
		person.LastName = "Smith";
		response = await httpClient.PutAsJsonAsync("/people", person);
		response.EnsureSuccessStatusCode();
		var updatedPerson = await response.Content.ReadAsAsync<Person>();
		Assert.NotNull(updatedPerson);
		Assert.Equal(person.Id, updatedPerson.Id);
		Assert.Equal(person.FirstName, updatedPerson.FirstName);
		Assert.Equal(person.LastName, updatedPerson.LastName);

		// Act - Can Delete
		response = await httpClient.DeleteAsync($"/people/{person.Id}");
		response.EnsureSuccessStatusCode();
	}
}
