using JetBrains.Annotations;
using VioletManager.Application.Contacts.Commands;
using VioletManager.Application.Handlers.Contacts;
using VioletManager.Application.UnitTests.Fakes;
using VioletManager.Domain.Entities;
using VioletManager.Domain.ValueObjects;

namespace VioletManager.Application.UnitTests;

[TestClass]
[TestSubject(typeof(GetContactHandler))]
public sealed class GetContactHandlerTests
{
    [TestMethod]
    [Description("Verifies that an existing contact is returned with all of its data.")]
    public async Task HandleAsync_WithExistingContact_ReturnsItsData()
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var contact = Contact.Create("John", "Doe");

        contact.SetCompanyName("Violet Industries");
        contact.SetAdditionalNotes("Prefers email.");
        contact.SetWorkDetails(
            ContactDetails.Create(
                "john.doe@company.com",
                "+43 1 234567",
                PostalAddress.Create("Main Street 1", "Vienna", null, "Austria", "1010")));
        contact.AddCategory("Customer");
        contact.AddSubcategory("Enterprise");

        await repository.AddAsync(contact);

        var handler = new GetContactHandler(repository);

        // ACT
        var result = await handler.HandleAsync(new GetContactCommand(contact.Id));

        // ASSERT
        Assert.IsNotNull(result);
        Assert.AreEqual(contact.Id, result.ContactId);
        Assert.AreEqual("John", result.FirstName);
        Assert.AreEqual("Doe", result.LastName);
        Assert.AreEqual("Violet Industries", result.CompanyName);
        Assert.AreEqual("Prefers email.", result.AdditionalNotes);
        Assert.AreEqual("john.doe@company.com", result.WorkDetails?.Email);
        Assert.AreEqual("Vienna", result.WorkDetails?.Address?.City);
        Assert.IsNull(result.PrivateDetails);
        Assert.AreEqual("Customer", result.Categories.Single());
        Assert.AreEqual("Enterprise", result.Subcategories.Single());
        Assert.AreEqual(1, repository.GetCallCount);
        Assert.AreEqual(contact.Id, repository.LastRequestedContactId);
    }

    [TestMethod]
    [Description("Verifies that an unknown contact yields null instead of throwing.")]
    public async Task HandleAsync_WithUnknownContact_ReturnsNull()
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var handler = new GetContactHandler(repository);

        // ACT
        var result = await handler.HandleAsync(new GetContactCommand(Guid.NewGuid()));

        // ASSERT
        Assert.IsNull(result);
        Assert.AreEqual(1, repository.GetCallCount);
    }

    [TestMethod]
    [Description("Verifies that the requested contact is returned when several are stored.")]
    public async Task HandleAsync_WithSeveralContacts_ReturnsTheRequestedOne()
    {
        // ARRANGE
        var repository = new FakeContactRepository();

        var target = Contact.Create("John", "Doe");
        var bystander = Contact.Create("Jane", "Roe");

        await repository.AddAsync(target);
        await repository.AddAsync(bystander);

        var handler = new GetContactHandler(repository);

        // ACT
        var result = await handler.HandleAsync(new GetContactCommand(target.Id));

        // ASSERT
        Assert.IsNotNull(result);
        Assert.AreEqual(target.Id, result.ContactId);
        Assert.AreEqual("John", result.FirstName);
    }

    [TestMethod]
    [Description("Verifies that a deleted contact is no longer readable.")]
    public async Task HandleAsync_AfterContactWasDeleted_ReturnsNull()
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var contact = Contact.Create("John", "Doe");

        await repository.AddAsync(contact);
        await repository.DeleteAsync(contact.Id);

        var handler = new GetContactHandler(repository);

        // ACT
        var result = await handler.HandleAsync(new GetContactCommand(contact.Id));

        // ASSERT
        Assert.IsNull(result);
    }

    [TestMethod]
    [Description("Verifies that a null command is rejected before the repository is touched.")]
    public async Task HandleAsync_WithNullCommand_ThrowsArgumentNullException()
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var handler = new GetContactHandler(repository);

        // ACT & ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            () => handler.HandleAsync(null!));

        Assert.AreEqual(0, repository.GetCallCount);
    }

    [TestMethod]
    [Description("Verifies that an empty identifier is rejected before the repository is touched.")]
    public async Task HandleAsync_WithEmptyContactId_ThrowsArgumentExceptionAndDoesNotRead()
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var handler = new GetContactHandler(repository);

        // ACT & ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => handler.HandleAsync(new GetContactCommand(Guid.Empty)));

        Assert.AreEqual(0, repository.GetCallCount);
    }

    [TestMethod]
    [Description("Verifies that the cancellation token is forwarded to the repository.")]
    public async Task HandleAsync_WithCancellationToken_ForwardsItToRepository()
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var handler = new GetContactHandler(repository);

        using var cancellationTokenSource = new CancellationTokenSource();

        // ACT
        await handler.HandleAsync(
            new GetContactCommand(Guid.NewGuid()),
            cancellationTokenSource.Token);

        // ASSERT
        Assert.AreEqual(cancellationTokenSource.Token, repository.LastCancellationToken);
    }

    [TestMethod]
    [Description("Verifies that a repository failure is surfaced to the caller.")]
    public async Task HandleAsync_WhenRepositoryThrows_PropagatesException()
    {
        // ARRANGE
        var repository = new FakeContactRepository
        {
            ExceptionToThrow = new InvalidOperationException("storage unavailable"),
        };

        var handler = new GetContactHandler(repository);

        // ACT & ASSERT
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => handler.HandleAsync(new GetContactCommand(Guid.NewGuid())));
    }

    [TestMethod]
    [Description("Verifies that reading a contact leaves the store untouched.")]
    public async Task HandleAsync_WithExistingContact_DoesNotModifyTheStore()
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var contact = Contact.Create("John", "Doe");

        await repository.AddAsync(contact);

        var handler = new GetContactHandler(repository);

        // ACT
        await handler.HandleAsync(new GetContactCommand(contact.Id));

        // ASSERT
        Assert.AreEqual(1, repository.Contacts.Count);
        Assert.AreEqual(0, repository.DeleteCallCount);
    }
}
