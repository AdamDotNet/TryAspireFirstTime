using TryAspireFirstTime.ApiService.Data.People;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Add cosmos db
builder.AddAzureCosmosClient("cosmos");
builder.Services.AddSingleton<IPeopleDocumentClient, PeopleDocumentClient>();

var app = builder.Build();

await app.Services.GetRequiredService<IPeopleDocumentClient>().InitializeAsync();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapGet("/", (ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("AspireFirstTime.ApiService");
    logger.LogInformation("Hello world from the root endpoint!");
	return Results.Text("Hello world");
});

app.MapGet("/debug", (IConfiguration configuration) =>
{
    // Dump all configuration values as the response body.
    var configValues = configuration.AsEnumerable()
        .Select(kvp => $"{kvp.Key}: {kvp.Value}")
        .ToList();
    return Results.Text(string.Join(Environment.NewLine, configValues));
});

app.MapGet("/people", async (IPeopleDocumentClient documentclient) =>
{
    return Results.Ok(await documentclient.GetPeopleAsync());
})
    .WithName("GetPeople");

// Get a person by ID
app.MapGet("/people/{id}", async (string id, IPeopleDocumentClient documentclient) =>
{
    var person = await documentclient.GetPersonAsync(id);
    return person is not null ? Results.Ok(person) : Results.NotFound();
})
    .WithName("GetPerson")
    .Produces<Person>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

// Upsert a person
app.MapPut("/people", async (Person person, IPeopleDocumentClient documentclient) =>
{
    var upsertedPerson = await documentclient.UpsertPersonAsync(person);
    return Results.Ok(upsertedPerson);
})
    .WithName("UpsertPerson")
    .Produces<Person>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest);

// Delete a person by ID
app.MapDelete("/people/{id}", async (string id, IPeopleDocumentClient documentclient) =>
{
    await documentclient.DeletePersonAsync(id);
    return Results.NoContent();
})
    .WithName("DeletePerson")
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound);

// Delete all people
app.MapDelete("/people", async (IPeopleDocumentClient documentclient) =>
{
    await documentclient.DeleteAllPeopleAsync();
    return Results.NoContent();
})
    .WithName("DeleteAllPeople")
    .Produces(StatusCodes.Status204NoContent);

app.MapDefaultEndpoints();

app.Run();


public partial class Program { }