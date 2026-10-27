namespace PeasyPilot.Core.Tests.Integration;

using System.Net;
using PeasyPilot.Core.Assertions;

using PeasyPilot.Integration.Fixtures;
using PeasyPilot.Integration.Helpers;
using Xunit;

public class IntegrationHelpersTests
{
    private record Product(int Id, string Name);

    [Fact]
    public async Task InMemoryTestDatabase_InitializesAndResetsCorrectly()
    {
        // Arrange
        var db = new InMemoryTestDatabase();
        await db.InitializeAsync();
        await db.SeedAsync();
        await db.Store.SeedAsync([new Product(1, "Laptop")]);

        var items = await db.Store.GetAllAsync<Product>();
        PeasyAssert.Single(items);

        // Act
        await db.ResetAsync();
        var itemsAfterReset = await db.Store.GetAllAsync<Product>();

        // Assert
        PeasyAssert.IsEmpty(itemsAfterReset);
    }

    [Fact]
    public void HttpTestClient_AssertStatusCode_ValidatesExpectedCode()
    {
        var responsePass = new HttpResponseMessage(HttpStatusCode.OK);
        var responseFail = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        // Act & Assert
        HttpTestClient.AssertStatusCode(responsePass, HttpStatusCode.OK);
        PeasyAssert.Throws<InvalidOperationException>(() => HttpTestClient.AssertStatusCode(responseFail, HttpStatusCode.OK));
    }
}
