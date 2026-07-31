using JetBrains.Annotations;
using VioletManager.Application.Contacts.Commands;
using VioletManager.Application.Handlers.Contacts;
using VioletManager.Application.UnitTests.Fakes;
using VioletManager.Domain.Entities;

namespace VioletManager.Application.UnitTests;

[TestClass]
[TestSubject(typeof(DeleteContactHandler))]
public sealed class DeleteContactHandlerTests
{
    [TestMethod]
    [Description("Verifies that deleting an existing contact removes it and reports success.")]
    public async Task HandleAsync_WithExistingContact_RemovesItAndReturnsTrue()
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var contact = Contact.Create("John", "Doe");

        await repository.AddAsync(contact);

        var handler = new DeleteContactHandler(repository);

        // ACT
        var deleted = await handler.HandleAsync(new DeleteContactCommand(contact.Id));

        // ASSERT
        Assert.IsTrue(deleted);
        Assert.AreEqual(1, repository.DeleteCallCount);
        Assert.AreEqual(contact.Id, repository.LastDeletedContactId);
        Assert.AreEqual(0, repository.Contacts.Count);
    }

    [TestMethod]
    [Description("Verifies that deleting an unknown contact reports failure instead of throwing.")]
    public async Task HandleAsync_WithUnknownContact_ReturnsFalse()
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var handler = new DeleteContactHandler(repository);

        // ACT
        var deleted = await handler.HandleAsync(new DeleteContactCommand(Guid.NewGuid()));

        // ASSERT
        Assert.IsFalse(deleted);
        Assert.AreEqual(1, repository.DeleteCallCount);
    }

    [TestMethod]
    [Description("Verifies that only the targeted contact is removed.")]
    public async Task HandleAsync_WithSeveralContacts_RemovesOnlyTheTargetedOne()
    {
        // ARRANGE
        var repository = new FakeContactRepository();

        var target = Contact.Create("John", "Doe");
        var bystander = Contact.Create("Jane", "Roe");

        await repository.AddAsync(target);
        await repository.AddAsync(bystander);

        var handler = new DeleteContactHandler(repository);

        // ACT
        var deleted = await handler.HandleAsync(new DeleteContactCommand(target.Id));

        // ASSERT
        Assert.IsTrue(deleted);
        Assert.AreEqual(bystander.Id, repository.Contacts.Single().Id);
    }

    [TestMethod]
    [Description("Verifies that deleting the same contact twice succeeds once and then reports failure.")]
    public async Task HandleAsync_CalledTwiceForSameContact_IsNotSuccessfulTheSecondTime()
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var contact = Contact.Create("John", "Doe");

        await repository.AddAsync(contact);

        var handler = new DeleteContactHandler(repository);
        var command = new DeleteContactCommand(contact.Id);

        // ACT
        var first = await handler.HandleAsync(command);
        var second = await handler.HandleAsync(command);

        // ASSERT
        Assert.IsTrue(first);
        Assert.IsFalse(second);
        Assert.AreEqual(2, repository.DeleteCallCount);
    }

    [TestMethod]
    [Description("Verifies that a null command is rejected before the repository is touched.")]
    public async Task HandleAsync_WithNullCommand_ThrowsArgumentNullException()
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var handler = new DeleteContactHandler(repository);

        // ACT & ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            () => handler.HandleAsync(null!));

        Assert.AreEqual(0, repository.DeleteCallCount);
    }

    [TestMethod]
    [Description("Verifies that an empty identifier is rejected before the repository is touched.")]
    public async Task HandleAsync_WithEmptyContactId_ThrowsArgumentExceptionAndDoesNotDelete()
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var handler = new DeleteContactHandler(repository);

        // ACT & ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => handler.HandleAsync(new DeleteContactCommand(Guid.Empty)));

        Assert.AreEqual(0, repository.DeleteCallCount);
    }

    [TestMethod]
    [Description("Verifies that the cancellation token is forwarded to the repository.")]
    public async Task HandleAsync_WithCancellationToken_ForwardsItToRepository()
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var handler = new DeleteContactHandler(repository);

        using var cancellationTokenSource = new CancellationTokenSource();

        // ACT
        await handler.HandleAsync(
            new DeleteContactCommand(Guid.NewGuid()),
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

        var handler = new DeleteContactHandler(repository);

        // ACT & ASSERT
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => handler.HandleAsync(new DeleteContactCommand(Guid.NewGuid())));
    }
}
