using JetBrains.Annotations;
using VioletManager.Domain.Entities;
using VioletManager.Domain.ValueObjects;

namespace VioletManager.Domain.UnitTests.Entities;

[TestClass]
[TestSubject(typeof(Contact))]
public sealed class ContactTests
{
    [TestMethod]
    [Description("Verifies that a contact is created when two valid names are supplied.")]
    public void Create_WithFirstAndLastName_CreatesContact()
    {
        // ARRANGE
        const string firstName = "John";
        const string lastName = "Doe";

        // ACT
        var contact = Contact.Create(firstName, lastName);

        // ASSERT
        Assert.AreNotEqual(Guid.Empty, contact.Id);
        Assert.AreEqual(firstName, contact.FirstName);
        Assert.AreEqual(lastName, contact.LastName);
    }

    [TestMethod]
    [Description("Verifies that a contact is created when only a first name is supplied.")]
    public void Create_WithOnlyFirstName_CreatesContact()
    {
        // ARRANGE
        const string firstName = "John";

        // ACT
        var contact = Contact.Create(firstName, null);

        // ASSERT
        Assert.AreNotEqual(Guid.Empty, contact.Id);
        Assert.AreEqual(firstName, contact.FirstName);
        Assert.IsNull(contact.LastName);
    }

    [TestMethod]
    [Description("Verifies that a contact is created when only a last name is supplied.")]
    public void Create_WithOnlyLastName_CreatesContact()
    {
        // ARRANGE
        const string lastName = "Doe";

        // ACT
        var contact = Contact.Create(null, lastName);

        // ASSERT
        Assert.AreNotEqual(Guid.Empty, contact.Id);
        Assert.IsNull(contact.FirstName);
        Assert.AreEqual(lastName, contact.LastName);
    }

    [TestMethod]
    [DataRow(null, null)]
    [DataRow("", "")]
    [DataRow(" ", " ")]
    [DataRow("", null)]
    [DataRow(null, "   ")]
    [Description("Verifies that contact creation fails when neither name contains a valid value.")]
    public void Create_WithoutValidName_ThrowsArgumentException(
        string? firstName,
        string? lastName)
    {
        // ACT & ASSERT
        Assert.ThrowsExactly<ArgumentException>(
            () => Contact.Create(firstName, lastName));
    }

    [TestMethod]
    [Description("Verifies that leading and trailing whitespace is removed from contact names.")]
    public void Create_WithWhitespaceAroundNames_TrimsNames()
    {
        // ARRANGE
        const string firstName = "  John  ";
        const string lastName = "  Doe  ";

        // ACT
        var contact = Contact.Create(firstName, lastName);

        // ASSERT
        Assert.AreEqual("John", contact.FirstName);
        Assert.AreEqual("Doe", contact.LastName);
    }

    [TestMethod]
    [Description("Verifies that an existing contact's name can be changed.")]
    public void SetName_WithValidNames_UpdatesNames()
    {
        // ARRANGE
        var contact = Contact.Create("John", "Doe");

        // ACT
        contact.SetName("Jane", "Smith");

        // ASSERT
        Assert.AreEqual("Jane", contact.FirstName);
        Assert.AreEqual("Smith", contact.LastName);
    }

    [TestMethod]
    [Description("Verifies that changing a contact's name trims the supplied values.")]
    public void SetName_WithWhitespaceAroundNames_TrimsNames()
    {
        // ARRANGE
        var contact = Contact.Create("John", "Doe");

        // ACT
        contact.SetName("  Jane  ", "  Smith  ");

        // ASSERT
        Assert.AreEqual("Jane", contact.FirstName);
        Assert.AreEqual("Smith", contact.LastName);
    }

    [TestMethod]
    [Description("Verifies that an invalid name change does not modify the current names.")]
    public void SetName_WithoutValidName_ThrowsAndPreservesCurrentNames()
    {
        // ARRANGE
        var contact = Contact.Create("John", "Doe");

        // ACT
        var action = () => contact.SetName(" ", null);

        // ASSERT
        Assert.ThrowsExactly<ArgumentException>(action);
        Assert.AreEqual("John", contact.FirstName);
        Assert.AreEqual("Doe", contact.LastName);
    }

    [TestMethod]
    [Description("Verifies that work details can be assigned to a contact.")]
    public void SetWorkDetails_WithContactDetails_SetsWorkDetails()
    {
        // ARRANGE
        var contact = Contact.Create("John", "Doe");

        var address = PostalAddress.Create(
            street: "Main Street 10",
            city: "Vienna",
            state: "Vienna",
            country: "Austria",
            zipCode: "1010");

        var workDetails = ContactDetails.Create(
            email: "john.doe@company.com",
            phone: "+43 660 1234567",
            address: address);

        // ACT
        contact.SetWorkDetails(workDetails);

        // ASSERT
        Assert.AreSame(workDetails, contact.WorkDetails);
    }

    [TestMethod]
    [Description("Verifies that existing work details can be replaced.")]
    public void SetWorkDetails_WithNewDetails_ReplacesExistingWorkDetails()
    {
        // ARRANGE
        var contact = Contact.Create("John", "Doe");

        var originalDetails = ContactDetails.Create(
            email: "old@company.com",
            phone: "+43 660 1111111",
            address: null);

        var newDetails = ContactDetails.Create(
            email: "new@company.com",
            phone: "+43 660 2222222",
            address: null);

        contact.SetWorkDetails(originalDetails);

        // ACT
        contact.SetWorkDetails(newDetails);

        // ASSERT
        Assert.AreSame(newDetails, contact.WorkDetails);
        Assert.AreNotSame(originalDetails, contact.WorkDetails);
    }

    [TestMethod]
    [Description("Verifies that existing work details can be cleared.")]
    public void ClearWorkDetails_WithExistingDetails_SetsWorkDetailsToNull()
    {
        // ARRANGE
        var contact = Contact.Create("John", "Doe");

        var workDetails = ContactDetails.Create(
            email: "john.doe@company.com",
            phone: "+43 660 1234567",
            address: null);

        contact.SetWorkDetails(workDetails);

        // ACT
        contact.ClearWorkDetails();

        // ASSERT
        Assert.IsNull(contact.WorkDetails);
    }

    [TestMethod]
    [Description("Verifies that private details can be assigned to a contact.")]
    public void SetPrivateDetails_WithContactDetails_SetsPrivateDetails()
    {
        // ARRANGE
        var contact = Contact.Create("John", "Doe");

        var address = PostalAddress.Create(
            street: "Example Street 20",
            city: "Graz",
            state: "Styria",
            country: "Austria",
            zipCode: "8010");

        var privateDetails = ContactDetails.Create(
            email: "john@example.com",
            phone: "+43 699 1234567",
            address: address);

        // ACT
        contact.SetPrivateDetails(privateDetails);

        // ASSERT
        Assert.AreSame(privateDetails, contact.PrivateDetails);
    }

    [TestMethod]
    [Description("Verifies that existing private details can be replaced.")]
    public void SetPrivateDetails_WithNewDetails_ReplacesExistingPrivateDetails()
    {
        // ARRANGE
        var contact = Contact.Create("John", "Doe");

        var originalDetails = ContactDetails.Create(
            email: "old@example.com",
            phone: "+43 699 1111111",
            address: null);

        var newDetails = ContactDetails.Create(
            email: "new@example.com",
            phone: "+43 699 2222222",
            address: null);

        contact.SetPrivateDetails(originalDetails);

        // ACT
        contact.SetPrivateDetails(newDetails);

        // ASSERT
        Assert.AreSame(newDetails, contact.PrivateDetails);
        Assert.AreNotSame(originalDetails, contact.PrivateDetails);
    }

    [TestMethod]
    [Description("Verifies that existing private details can be cleared.")]
    public void ClearPrivateDetails_WithExistingDetails_SetsPrivateDetailsToNull()
    {
        // ARRANGE
        var contact = Contact.Create("John", "Doe");

        var privateDetails = ContactDetails.Create(
            email: "john@example.com",
            phone: "+43 699 1234567",
            address: null);

        contact.SetPrivateDetails(privateDetails);

        // ACT
        contact.ClearPrivateDetails();

        // ASSERT
        Assert.IsNull(contact.PrivateDetails);
    }

    [TestMethod]
    [Description("Verifies that a company name is trimmed before being stored.")]
    public void SetCompanyName_WithWhitespaceAroundValue_TrimsCompanyName()
    {
        // ARRANGE
        var contact = Contact.Create("John", "Doe");

        // ACT
        contact.SetCompanyName("  Violet Industries  ");

        // ASSERT
        Assert.AreEqual("Violet Industries", contact.CompanyName);
    }

    [TestMethod]
    [Description("Verifies that a blank company name is normalized to null.")]
    public void SetCompanyName_WithBlankValue_SetsCompanyNameToNull()
    {
        // ARRANGE
        var contact = Contact.Create("John", "Doe");
        contact.SetCompanyName("Violet Industries");

        // ACT
        contact.SetCompanyName("   ");

        // ASSERT
        Assert.IsNull(contact.CompanyName);
    }

    [TestMethod]
    [Description("Verifies that additional notes are trimmed before being stored.")]
    public void SetAdditionalNotes_WithWhitespaceAroundValue_TrimsNotes()
    {
        // ARRANGE
        var contact = Contact.Create("John", "Doe");

        // ACT
        contact.SetAdditionalNotes("  Important customer  ");

        // ASSERT
        Assert.AreEqual("Important customer", contact.AdditionalNotes);
    }

    [TestMethod]
    [Description("Verifies that a category is trimmed before being added.")]
    public void AddCategory_WithValidCategory_AddsTrimmedCategory()
    {
        // ARRANGE
        var contact = Contact.Create("John", "Doe");

        // ACT
        contact.AddCategory("  Customer  ");

        // ASSERT
        Assert.AreEqual(1, contact.Categories.Count);
        Assert.AreEqual("Customer", contact.Categories.Single());
    }

    [TestMethod]
    [Description("Verifies that duplicate categories are not added regardless of casing.")]
    public void AddCategory_WithCaseInsensitiveDuplicate_DoesNotAddDuplicate()
    {
        // ARRANGE
        var contact = Contact.Create("John", "Doe");
        contact.AddCategory("Customer");

        // ACT
        contact.AddCategory("customer");

        // ASSERT
        Assert.AreEqual(1, contact.Categories.Count);
        Assert.AreEqual("Customer", contact.Categories.Single());
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    [Description("Verifies that an invalid category cannot be added.")]
    public void AddCategory_WithInvalidCategory_ThrowsArgumentException(
        string? category)
    {
        // ARRANGE
        var contact = Contact.Create("John", "Doe");

        // ACT & ASSERT
        Assert.ThrowsExactly<ArgumentException>(
            () => contact.AddCategory(category!));
    }

    [TestMethod]
    [Description("Verifies that a category can be removed regardless of casing.")]
    public void RemoveCategory_WithDifferentCasing_RemovesCategory()
    {
        // ARRANGE
        var contact = Contact.Create("John", "Doe");
        contact.AddCategory("Customer");

        // ACT
        contact.RemoveCategory("CUSTOMER");

        // ASSERT
        Assert.AreEqual(0, contact.Categories.Count);
    }

    [TestMethod]
    [Description("Verifies that all categories can be removed.")]
    public void ClearCategories_WithExistingCategories_RemovesAllCategories()
    {
        // ARRANGE
        var contact = Contact.Create("John", "Doe");
        contact.AddCategory("Customer");
        contact.AddCategory("Supplier");

        // ACT
        contact.ClearCategories();

        // ASSERT
        Assert.AreEqual(0, contact.Categories.Count);
    }

    [TestMethod]
    [Description("Verifies that a subcategory is trimmed before being added.")]
    public void AddSubcategory_WithValidSubcategory_AddsTrimmedSubcategory()
    {
        // ARRANGE
        var contact = Contact.Create("John", "Doe");

        // ACT
        contact.AddSubcategory("  Premium  ");

        // ASSERT
        Assert.AreEqual(1, contact.Subcategories.Count);
        Assert.AreEqual("Premium", contact.Subcategories.Single());
    }

    [TestMethod]
    [Description("Verifies that duplicate subcategories are not added regardless of casing.")]
    public void AddSubcategory_WithCaseInsensitiveDuplicate_DoesNotAddDuplicate()
    {
        // ARRANGE
        var contact = Contact.Create("John", "Doe");
        contact.AddSubcategory("Premium");

        // ACT
        contact.AddSubcategory("premium");

        // ASSERT
        Assert.AreEqual(1, contact.Subcategories.Count);
        Assert.AreEqual("Premium", contact.Subcategories.Single());
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    [Description("Verifies that an invalid subcategory cannot be added.")]
    public void AddSubcategory_WithInvalidSubcategory_ThrowsArgumentException(
        string? subcategory)
    {
        // ARRANGE
        var contact = Contact.Create("John", "Doe");

        // ACT & ASSERT
        Assert.ThrowsExactly<ArgumentException>(
            () => contact.AddSubcategory(subcategory!));
    }

    [TestMethod]
    [Description("Verifies that a subcategory can be removed regardless of casing.")]
    public void RemoveSubcategory_WithDifferentCasing_RemovesSubcategory()
    {
        // ARRANGE
        var contact = Contact.Create("John", "Doe");
        contact.AddSubcategory("Premium");

        // ACT
        contact.RemoveSubcategory("PREMIUM");

        // ASSERT
        Assert.AreEqual(0, contact.Subcategories.Count);
    }

    [TestMethod]
    [Description("Verifies that all subcategories can be removed.")]
    public void ClearSubcategories_WithExistingSubcategories_RemovesAllSubcategories()
    {
        // ARRANGE
        var contact = Contact.Create("John", "Doe");
        contact.AddSubcategory("Premium");
        contact.AddSubcategory("Newsletter");

        // ACT
        contact.ClearSubcategories();

        // ASSERT
        Assert.AreEqual(0, contact.Subcategories.Count);
    }
}