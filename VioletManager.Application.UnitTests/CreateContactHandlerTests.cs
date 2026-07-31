using JetBrains.Annotations;
using VioletManager.Application.UnitTests.Fakes;

namespace VioletManager.Application.UnitTests;

[TestClass]
[TestSubject(typeof(CreateContactHandler))]
public sealed class CreateContactHandlerTests
{
    private static CreateContactCommand EmptyCommand => new(
        FirstName: null,
        LastName: null,
        WorkEmail: null,
        WorkPhone: null,
        WorkAddress: null,
        WorkCity: null,
        WorkState: null,
        WorkCountry: null,
        WorkZipCode: null,
        PrivateEmail: null,
        PrivatePhone: null,
        PrivateAddress: null,
        PrivateCity: null,
        PrivateState: null,
        PrivateCountry: null,
        PrivateZipCode: null,
        CompanyName: null,
        AdditionalNotes: null);

    [TestMethod]
    [Description("Verifies that a valid command persists the contact and returns its identifier.")]
    public async Task HandleAsync_WithMinimalCommand_PersistsContactAndReturnsId()
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var handler = new CreateContactHandler(repository);

        var command = EmptyCommand with { FirstName = "John", LastName = "Doe" };

        // ACT
        var result = await handler.HandleAsync(command);

        // ASSERT
        Assert.AreEqual(1, repository.AddCallCount);

        var contact = repository.Contacts.Single();

        Assert.AreEqual(contact.Id, result.ContactId);
        Assert.AreNotEqual(Guid.Empty, result.ContactId);
        Assert.AreEqual("John", contact.FirstName);
        Assert.AreEqual("Doe", contact.LastName);
    }

    [TestMethod]
    [Description("Verifies that a null command is rejected before the repository is touched.")]
    public async Task HandleAsync_WithNullCommand_ThrowsArgumentNullException()
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var handler = new CreateContactHandler(repository);

        // ACT & ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            () => handler.HandleAsync(null!));

        Assert.AreEqual(0, repository.AddCallCount);
    }

    [TestMethod]
    [Description("Verifies that a command without any usable name is rejected without persisting.")]
    public async Task HandleAsync_WithoutValidName_ThrowsArgumentExceptionAndDoesNotPersist()
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var handler = new CreateContactHandler(repository);

        var command = EmptyCommand with { FirstName = "   ", CompanyName = "Violet Industries" };

        // ACT & ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => handler.HandleAsync(command));

        Assert.AreEqual(0, repository.AddCallCount);
    }

    [TestMethod]
    [Description("Verifies that no contact details are created when only the names are supplied.")]
    public async Task HandleAsync_WithoutDetailFields_LeavesWorkAndPrivateDetailsNull()
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var handler = new CreateContactHandler(repository);

        var command = EmptyCommand with { FirstName = "John", LastName = "Doe" };

        // ACT
        await handler.HandleAsync(command);

        // ASSERT
        var contact = repository.Contacts.Single();

        Assert.IsNull(contact.WorkDetails);
        Assert.IsNull(contact.PrivateDetails);
    }

    [TestMethod]
    [DataRow("   ", "   ", DisplayName = "Whitespace only")]
    [DataRow(null, null, DisplayName = "Null only")]
    [Description("Verifies that blank detail fields do not produce empty contact details.")]
    public async Task HandleAsync_WithBlankDetailFields_LeavesWorkDetailsNull(
        string? email,
        string? city)
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var handler = new CreateContactHandler(repository);

        var command = EmptyCommand with
        {
            FirstName = "John",
            WorkEmail = email,
            WorkCity = city,
        };

        // ACT
        await handler.HandleAsync(command);

        // ASSERT
        Assert.IsNull(repository.Contacts.Single().WorkDetails);
    }

    [TestMethod]
    [Description("Verifies that work details are populated from the command, including the address.")]
    public async Task HandleAsync_WithFullWorkFields_SetsWorkDetailsWithAddress()
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var handler = new CreateContactHandler(repository);

        var command = EmptyCommand with
        {
            FirstName = "John",
            WorkEmail = "  john.doe@company.com  ",
            WorkPhone = "+43 660 1234567",
            WorkAddress = "Main Street 10",
            WorkCity = "Vienna",
            WorkState = "Vienna",
            WorkCountry = "Austria",
            WorkZipCode = "1010",
        };

        // ACT
        await handler.HandleAsync(command);

        // ASSERT
        var workDetails = repository.Contacts.Single().WorkDetails;

        Assert.IsNotNull(workDetails);
        Assert.AreEqual("john.doe@company.com", workDetails.Email);
        Assert.AreEqual("+43 660 1234567", workDetails.Phone);

        Assert.IsNotNull(workDetails.Address);
        Assert.AreEqual("Main Street 10", workDetails.Address.Street);
        Assert.AreEqual("Vienna", workDetails.Address.City);
        Assert.AreEqual("Vienna", workDetails.Address.State);
        Assert.AreEqual("Austria", workDetails.Address.Country);
        Assert.AreEqual("1010", workDetails.Address.ZipCode);
    }

    [TestMethod]
    [Description("Verifies that contact details without any address field carry a null address.")]
    public async Task HandleAsync_WithWorkEmailOnly_SetsWorkDetailsWithoutAddress()
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var handler = new CreateContactHandler(repository);

        var command = EmptyCommand with
        {
            FirstName = "John",
            WorkEmail = "john.doe@company.com",
        };

        // ACT
        await handler.HandleAsync(command);

        // ASSERT
        var workDetails = repository.Contacts.Single().WorkDetails;

        Assert.IsNotNull(workDetails);
        Assert.AreEqual("john.doe@company.com", workDetails.Email);
        Assert.IsNull(workDetails.Phone);
        Assert.IsNull(workDetails.Address);
    }

    [TestMethod]
    [Description("Verifies that an address is created even when only a single address field is supplied.")]
    public async Task HandleAsync_WithSingleAddressField_CreatesAddress()
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var handler = new CreateContactHandler(repository);

        var command = EmptyCommand with
        {
            FirstName = "John",
            PrivateCity = "Graz",
        };

        // ACT
        await handler.HandleAsync(command);

        // ASSERT
        var privateDetails = repository.Contacts.Single().PrivateDetails;

        Assert.IsNotNull(privateDetails);
        Assert.IsNotNull(privateDetails.Address);
        Assert.AreEqual("Graz", privateDetails.Address.City);
    }

    [TestMethod]
    [Description("Verifies that work and private details are mapped independently of each other.")]
    public async Task HandleAsync_WithWorkAndPrivateFields_MapsBothIndependently()
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var handler = new CreateContactHandler(repository);

        var command = EmptyCommand with
        {
            FirstName = "John",
            WorkEmail = "john.doe@company.com",
            WorkCity = "Vienna",
            PrivateEmail = "john@example.com",
            PrivateCity = "Graz",
        };

        // ACT
        await handler.HandleAsync(command);

        // ASSERT
        var contact = repository.Contacts.Single();

        Assert.IsNotNull(contact.WorkDetails);
        Assert.IsNotNull(contact.PrivateDetails);
        Assert.AreEqual("john.doe@company.com", contact.WorkDetails.Email);
        Assert.AreEqual("Vienna", contact.WorkDetails.Address?.City);
        Assert.AreEqual("john@example.com", contact.PrivateDetails.Email);
        Assert.AreEqual("Graz", contact.PrivateDetails.Address?.City);
    }

    [TestMethod]
    [Description("Verifies that the company name and additional notes are trimmed onto the contact.")]
    public async Task HandleAsync_WithCompanyAndNotes_TrimsAndSetsThem()
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var handler = new CreateContactHandler(repository);

        var command = EmptyCommand with
        {
            FirstName = "John",
            CompanyName = "  Violet Industries  ",
            AdditionalNotes = "  Important customer  ",
        };

        // ACT
        await handler.HandleAsync(command);

        // ASSERT
        var contact = repository.Contacts.Single();

        Assert.AreEqual("Violet Industries", contact.CompanyName);
        Assert.AreEqual("Important customer", contact.AdditionalNotes);
    }

    [TestMethod]
    [Description("Verifies that the cancellation token is forwarded to the repository.")]
    public async Task HandleAsync_WithCancellationToken_ForwardsItToRepository()
    {
        // ARRANGE
        var repository = new FakeContactRepository();
        var handler = new CreateContactHandler(repository);

        using var cancellationTokenSource = new CancellationTokenSource();

        var command = EmptyCommand with { FirstName = "John" };

        // ACT
        await handler.HandleAsync(command, cancellationTokenSource.Token);

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

        var handler = new CreateContactHandler(repository);

        var command = EmptyCommand with { FirstName = "John" };

        // ACT & ASSERT
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => handler.HandleAsync(command));
    }
}
