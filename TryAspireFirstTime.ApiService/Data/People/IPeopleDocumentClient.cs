
namespace TryAspireFirstTime.ApiService.Data.People
{
	public interface IPeopleDocumentClient
	{
		Task DeleteAllPeopleAsync();
		Task DeletePersonAsync(string id);
		Task<IEnumerable<Person>> GetPeopleAsync();
		Task<Person?> GetPersonAsync(string id);
		Task InitializeAsync();
		Task<Person> UpsertPersonAsync(Person person);
	}
}