using System.Net;
using System.Net.Http.Json;
using JetBrains.Annotations;
using VioletManager.API.Controllers;
using VioletManager.Application.Contacts.Results;

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

    [TestMethod]
    [Description("Verifies that deleting an existing contact returns 204 and removes it from the store.")]
    public async Task DeleteContact_WithExistingContact_Returns204AndRemovesIt()
    {
        // ARRANGE
        using var factory = new ContactApiFactory();
        using var client = factory.CreateClient();

        var payload = new { firstName = "John", lastName = "Doe" };

        using var createResponse = await client.PostAsJsonAsync(ContactsUri, payload, TestContext.CancellationTokenSource.Token);

        var created = await createResponse.Content.ReadFromJsonAsync<CreateContactResult>();

        Assert.IsNotNull(created);

        // ACT
        using var response = await client.DeleteAsync(
            $"{ContactsUri}/{created.ContactId}",
            TestContext.CancellationTokenSource.Token);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        Assert.AreEqual(0, factory.ContactRepository.Contacts.Count);
    }

    [TestMethod]
    [Description("Verifies that the location returned by the create endpoint is the one the delete endpoint accepts.")]
    public async Task DeleteContact_UsingCreatedLocationHeader_Returns204()
    {
        // ARRANGE
        using var factory = new ContactApiFactory();
        using var client = factory.CreateClient();

        var payload = new { firstName = "John", lastName = "Doe" };

        using var createResponse = await client.PostAsJsonAsync(ContactsUri, payload, TestContext.CancellationTokenSource.Token);

        var location = createResponse.Headers.Location;

        Assert.IsNotNull(location);

        // ACT
        using var response = await client.DeleteAsync(location, TestContext.CancellationTokenSource.Token);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    [TestMethod]
    [Description("Verifies that deleting an unknown contact returns a 404 problem response.")]
    public async Task DeleteContact_WithUnknownId_Returns404Problem()
    {
        // ARRANGE
        using var factory = new ContactApiFactory();
        using var client = factory.CreateClient();

        // ACT
        using var response = await client.DeleteAsync(
            $"{ContactsUri}/{Guid.NewGuid()}",
            TestContext.CancellationTokenSource.Token);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();

        Assert.IsTrue(
            body.Contains("\"status\":404", StringComparison.Ordinal),
            $"Expected a ProblemDetails body, got: {body}");
    }

    [TestMethod]
    [Description("Verifies that deleting a contact twice returns 404 on the second attempt.")]
    public async Task DeleteContact_CalledTwice_Returns404OnSecondAttempt()
    {
        // ARRANGE
        using var factory = new ContactApiFactory();
        using var client = factory.CreateClient();

        var payload = new { firstName = "John", lastName = "Doe" };

        using var createResponse = await client.PostAsJsonAsync(ContactsUri, payload, TestContext.CancellationTokenSource.Token);

        var created = await createResponse.Content.ReadFromJsonAsync<CreateContactResult>();

        Assert.IsNotNull(created);

        var uri = $"{ContactsUri}/{created.ContactId}";

        // ACT
        using var first = await client.DeleteAsync(uri, TestContext.CancellationTokenSource.Token);
        using var second = await client.DeleteAsync(uri, TestContext.CancellationTokenSource.Token);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.NoContent, first.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, second.StatusCode);
    }

    [TestMethod]
    [Description("Verifies that an empty identifier is rejected with a 400 problem response.")]
    public async Task DeleteContact_WithEmptyGuid_Returns400Problem()
    {
        // ARRANGE
        using var factory = new ContactApiFactory();
        using var client = factory.CreateClient();

        // ACT
        using var response = await client.DeleteAsync(
            $"{ContactsUri}/{Guid.Empty}",
            TestContext.CancellationTokenSource.Token);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    [Description("Verifies that a non-GUID identifier does not route to the delete endpoint.")]
    public async Task DeleteContact_WithNonGuidId_Returns404()
    {
        // ARRANGE
        using var factory = new ContactApiFactory();
        using var client = factory.CreateClient();

        // ACT
        using var response = await client.DeleteAsync(
            $"{ContactsUri}/not-a-guid",
            TestContext.CancellationTokenSource.Token);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    [Description("Verifies that deleting one contact leaves the others untouched.")]
    public async Task DeleteContact_WithSeveralContacts_RemovesOnlyTheTargetedOne()
    {
        // ARRANGE
        using var factory = new ContactApiFactory();
        using var client = factory.CreateClient();

        using var targetResponse = await client.PostAsJsonAsync(
            ContactsUri,
            new { firstName = "John", lastName = "Doe" },
            TestContext.CancellationTokenSource.Token);

        using var bystanderResponse = await client.PostAsJsonAsync(
            ContactsUri,
            new { firstName = "Jane", lastName = "Roe" },
            TestContext.CancellationTokenSource.Token);

        var target = await targetResponse.Content.ReadFromJsonAsync<CreateContactResult>();
        var bystander = await bystanderResponse.Content.ReadFromJsonAsync<CreateContactResult>();

        Assert.IsNotNull(target);
        Assert.IsNotNull(bystander);

        // ACT
        using var response = await client.DeleteAsync(
            $"{ContactsUri}/{target.ContactId}",
            TestContext.CancellationTokenSource.Token);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        Assert.AreEqual(bystander.ContactId, factory.ContactRepository.Contacts.Single().Id);
    }

    [TestMethod]
    [Description("Verifies that the contacts collection itself cannot be deleted.")]
    public async Task DeleteContacts_WithoutId_Returns405()
    {
        // ARRANGE
        using var factory = new ContactApiFactory();
        using var client = factory.CreateClient();

        // ACT
        using var response = await client.DeleteAsync(ContactsUri, TestContext.CancellationTokenSource.Token);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    public TestContext TestContext { get; set; } = null!;
}
