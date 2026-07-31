using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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
    [Description("Verifies that the contacts collection itself cannot be read.")]
    public async Task GetContacts_WithoutId_Returns405()
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

    [TestMethod]
    [Description("Verifies that an existing contact is returned with 200 and its full payload.")]
    public async Task GetContact_WithExistingContact_Returns200WithItsData()
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
            additionalNotes = "Prefers email.",
        };

        using var createResponse = await client.PostAsJsonAsync(ContactsUri, payload, TestContext.CancellationTokenSource.Token);

        var created = await createResponse.Content.ReadFromJsonAsync<CreateContactResult>();

        Assert.IsNotNull(created);

        // ACT
        using var response = await client.GetAsync(
            $"{ContactsUri}/{created.ContactId}",
            TestContext.CancellationTokenSource.Token);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual($"{ContactsUri}/{created.ContactId}", response.Headers.Location?.ToString());

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.CancellationTokenSource.Token));

        var root = document.RootElement;

        Assert.AreEqual(created.ContactId, root.GetProperty("contactId").GetGuid());
        Assert.AreEqual("John", root.GetProperty("firstName").GetString());
        Assert.AreEqual("Doe", root.GetProperty("lastName").GetString());
        Assert.AreEqual("Violet Industries", root.GetProperty("companyName").GetString());
        Assert.AreEqual("Prefers email.", root.GetProperty("additionalNotes").GetString());

        var workDetails = root.GetProperty("workDetails");

        Assert.AreEqual("john.doe@company.com", workDetails.GetProperty("email").GetString());
        Assert.AreEqual("Vienna", workDetails.GetProperty("address").GetProperty("city").GetString());

        Assert.AreEqual(JsonValueKind.Null, root.GetProperty("privateDetails").ValueKind);
        Assert.AreEqual(0, root.GetProperty("categories").GetArrayLength());
        Assert.AreEqual(0, root.GetProperty("subcategories").GetArrayLength());
    }

    [TestMethod]
    [Description("Verifies that the location returned by the create endpoint is readable via GET.")]
    public async Task GetContact_UsingCreatedLocationHeader_Returns200()
    {
        // ARRANGE
        using var factory = new ContactApiFactory();
        using var client = factory.CreateClient();

        var payload = new { firstName = "John", lastName = "Doe" };

        using var createResponse = await client.PostAsJsonAsync(ContactsUri, payload, TestContext.CancellationTokenSource.Token);

        var location = createResponse.Headers.Location;

        Assert.IsNotNull(location);

        // ACT
        using var response = await client.GetAsync(location, TestContext.CancellationTokenSource.Token);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    [Description("Verifies that a 404 response carries no location header.")]
    public async Task GetContact_WithUnknownId_DoesNotReturnALocationHeader()
    {
        // ARRANGE
        using var factory = new ContactApiFactory();
        using var client = factory.CreateClient();

        // ACT
        using var response = await client.GetAsync(
            $"{ContactsUri}/{Guid.NewGuid()}",
            TestContext.CancellationTokenSource.Token);

        // ASSERT
        Assert.IsNull(response.Headers.Location);
    }

    [TestMethod]
    [Description("Verifies that reading an unknown contact returns a 404 problem response.")]
    public async Task GetContact_WithUnknownId_Returns404Problem()
    {
        // ARRANGE
        using var factory = new ContactApiFactory();
        using var client = factory.CreateClient();

        // ACT
        using var response = await client.GetAsync(
            $"{ContactsUri}/{Guid.NewGuid()}",
            TestContext.CancellationTokenSource.Token);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync(TestContext.CancellationTokenSource.Token);

        Assert.IsTrue(
            body.Contains("\"status\":404", StringComparison.Ordinal),
            $"Expected a ProblemDetails body, got: {body}");
    }

    [TestMethod]
    [Description("Verifies that an empty identifier is rejected with a 400 problem response.")]
    public async Task GetContact_WithEmptyGuid_Returns400Problem()
    {
        // ARRANGE
        using var factory = new ContactApiFactory();
        using var client = factory.CreateClient();

        // ACT
        using var response = await client.GetAsync(
            $"{ContactsUri}/{Guid.Empty}",
            TestContext.CancellationTokenSource.Token);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    [Description("Verifies that a non-GUID identifier does not route to the get endpoint.")]
    public async Task GetContact_WithNonGuidId_Returns404()
    {
        // ARRANGE
        using var factory = new ContactApiFactory();
        using var client = factory.CreateClient();

        // ACT
        using var response = await client.GetAsync(
            $"{ContactsUri}/not-a-guid",
            TestContext.CancellationTokenSource.Token);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    [Description("Verifies that a deleted contact can no longer be read.")]
    public async Task GetContact_AfterDelete_Returns404()
    {
        // ARRANGE
        using var factory = new ContactApiFactory();
        using var client = factory.CreateClient();

        using var createResponse = await client.PostAsJsonAsync(
            ContactsUri,
            new { firstName = "John", lastName = "Doe" },
            TestContext.CancellationTokenSource.Token);

        var created = await createResponse.Content.ReadFromJsonAsync<CreateContactResult>();

        Assert.IsNotNull(created);

        var uri = $"{ContactsUri}/{created.ContactId}";

        using var deleteResponse = await client.DeleteAsync(uri, TestContext.CancellationTokenSource.Token);

        Assert.AreEqual(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // ACT
        using var response = await client.GetAsync(uri, TestContext.CancellationTokenSource.Token);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    [Description("Verifies that reading one contact returns that contact and not another.")]
    public async Task GetContact_WithSeveralContacts_ReturnsOnlyTheRequestedOne()
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

        Assert.IsNotNull(target);

        // ACT
        using var response = await client.GetAsync(
            $"{ContactsUri}/{target.ContactId}",
            TestContext.CancellationTokenSource.Token);

        // ASSERT
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.CancellationTokenSource.Token));

        Assert.AreEqual(target.ContactId, document.RootElement.GetProperty("contactId").GetGuid());
        Assert.AreEqual("John", document.RootElement.GetProperty("firstName").GetString());
        Assert.AreEqual(2, factory.ContactRepository.Contacts.Count);
    }

    public TestContext TestContext { get; set; } = null!;
}
