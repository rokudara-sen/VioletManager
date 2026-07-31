using System.Net;
using System.Net.Http.Json;
using JetBrains.Annotations;
using VioletManager.API.Controllers;
using VioletManager.Application;

namespace VioletManager.IntegrationTests;

[TestClass]
[TestSubject(typeof(ContactController))]
public sealed class ContactEndpointTests
{
    private const string ContactsUri = "/api/v1/contacts";

    [TestMethod]
    [Description("Verifies that a valid request creates a contact and returns 201 with its location.")]
    public async Task PostContact_WithValidPayload_Returns201WithLocationAndPersistsContact()
    {
        // ARRANGE
        using var factory = new ContactApiFactory();
        using var client = factory.CreateClient();

        var payload = new
        {
            firstName = "John",
            lastName = "Doe",
            workEmail = "john.doe@company.com",
            workCity = "Vienna",
            companyName = "Violet Industries",
        };

        // ACT
        using var response = await client.PostAsJsonAsync(ContactsUri, payload, TestContext.CancellationTokenSource.Token);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<CreateContactResult>();

        Assert.IsNotNull(result);
        Assert.AreNotEqual(Guid.Empty, result.ContactId);
        Assert.AreEqual($"{ContactsUri}/{result.ContactId}", response.Headers.Location?.ToString());

        var contact = factory.ContactRepository.Contacts.Single();

        Assert.AreEqual(result.ContactId, contact.Id);
        Assert.AreEqual("John", contact.FirstName);
        Assert.AreEqual("Doe", contact.LastName);
        Assert.AreEqual("Violet Industries", contact.CompanyName);
        Assert.AreEqual("john.doe@company.com", contact.WorkDetails?.Email);
        Assert.AreEqual("Vienna", contact.WorkDetails?.Address?.City);
    }

    [TestMethod]
    [Description("Verifies that a payload without a usable name is rejected with a 400 problem response.")]
    public async Task PostContact_WithoutValidName_Returns400ProblemAndPersistsNothing()
    {
        // ARRANGE
        using var factory = new ContactApiFactory();
        using var client = factory.CreateClient();

        var payload = new { companyName = "Violet Industries" };

        // ACT
        using var response = await client.PostAsJsonAsync(ContactsUri, payload, TestContext.CancellationTokenSource.Token);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.AreEqual(0, factory.ContactRepository.Contacts.Count);

        var body = await response.Content.ReadAsStringAsync();

        Assert.IsTrue(
            body.Contains("\"status\":400", StringComparison.Ordinal),
            $"Expected a ProblemDetails body, got: {body}");
    }

    [TestMethod]
    [Description("Verifies that a malformed JSON body is rejected with a 400 before reaching the handler.")]
    public async Task PostContact_WithMalformedJson_Returns400()
    {
        // ARRANGE
        using var factory = new ContactApiFactory();
        using var client = factory.CreateClient();

        using var content = new StringContent(
            "{ \"firstName\": ",
            System.Text.Encoding.UTF8,
            "application/json");

        // ACT
        using var response = await client.PostAsync(ContactsUri, content, TestContext.CancellationTokenSource.Token);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.AreEqual(0, factory.ContactRepository.Contacts.Count);
    }

    [TestMethod]
    [Description("Verifies that only POST is routed on the contacts collection.")]
    public async Task GetContacts_WhenNoReadEndpointExists_Returns405()
    {
        // ARRANGE
        using var factory = new ContactApiFactory();
        using var client = factory.CreateClient();

        // ACT
        using var response = await client.GetAsync(ContactsUri, TestContext.CancellationTokenSource.Token);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    public TestContext TestContext { get; set; } = null!;
}
