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
    public async Task GetWebResourceRootReturnsOkStatusCode()
    {
        // Arrange
        await using var app = await HostAccessor.GetAppAsync(_testOutputHelper);

		// Act
		var httpClient = app.CreateHttpClient("apiservice");
        var response = await httpClient.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
