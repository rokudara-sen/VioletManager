using JetBrains.Annotations;
using VioletManager.Domain.ValueObjects;

namespace VioletManager.Domain.UnitTests.ValueObjects;

[TestClass]
[TestSubject(typeof(ContactDetails))]
public sealed class ContactDetailsTests
{
    [TestMethod]
    [Description("Verifies that contact detail values are trimmed and the address is stored.")]
    public void Create_WithValues_TrimsValuesAndStoresAddress()
    {
        // ARRANGE
        var address = PostalAddress.Create(
            street: "Main Street 10",
            city: "Vienna",
            state: "Vienna",
            country: "Austria",
            zipCode: "1010");

        // ACT
        var details = ContactDetails.Create(
            email: "  john@example.com  ",
            phone: "  +43 660 1234567  ",
            address: address);

        // ASSERT
        Assert.AreEqual("john@example.com", details.Email);
        Assert.AreEqual("+43 660 1234567", details.Phone);
        Assert.AreSame(address, details.Address);
    }

    [TestMethod]
    [Description("Verifies that blank contact detail values are normalized to null.")]
    public void Create_WithBlankValues_NormalizesValuesToNull()
    {
        // ACT
        var details = ContactDetails.Create(
            email: "   ",
            phone: "",
            address: null);

        // ASSERT
        Assert.IsNull(details.Email);
        Assert.IsNull(details.Phone);
        Assert.IsNull(details.Address);
    }

    [TestMethod]
    [Description("Verifies that contact details with the same values are equal.")]
    public void Create_WithSameValues_ProducesEqualValueObjects()
    {
        // ARRANGE
        var firstAddress = PostalAddress.Create(
            street: "Main Street 10",
            city: "Vienna",
            state: "Vienna",
            country: "Austria",
            zipCode: "1010");

        var secondAddress = PostalAddress.Create(
            street: "Main Street 10",
            city: "Vienna",
            state: "Vienna",
            country: "Austria",
            zipCode: "1010");

        var firstDetails = ContactDetails.Create(
            email: "john@example.com",
            phone: "+43 660 1234567",
            address: firstAddress);

        var secondDetails = ContactDetails.Create(
            email: "john@example.com",
            phone: "+43 660 1234567",
            address: secondAddress);

        // ACT
        var areEqual = firstDetails == secondDetails;

        // ASSERT
        Assert.IsTrue(areEqual);
        Assert.AreEqual(firstDetails, secondDetails);
        Assert.AreNotSame(firstDetails, secondDetails);
    }

    [TestMethod]
    [Description("Verifies that contact details with different email addresses are not equal.")]
    public void Create_WithDifferentEmail_ProducesDifferentValueObjects()
    {
        // ARRANGE
        var firstDetails = ContactDetails.Create(
            email: "john@example.com",
            phone: "+43 660 1234567",
            address: null);

        var secondDetails = ContactDetails.Create(
            email: "jane@example.com",
            phone: "+43 660 1234567",
            address: null);

        // ACT
        var areEqual = firstDetails == secondDetails;

        // ASSERT
        Assert.IsFalse(areEqual);
        Assert.AreNotEqual(firstDetails, secondDetails);
    }

    [TestMethod]
    [Description("Verifies that contact details with different phone numbers are not equal.")]
    public void Create_WithDifferentPhone_ProducesDifferentValueObjects()
    {
        // ARRANGE
        var firstDetails = ContactDetails.Create(
            email: "john@example.com",
            phone: "+43 660 1111111",
            address: null);

        var secondDetails = ContactDetails.Create(
            email: "john@example.com",
            phone: "+43 660 2222222",
            address: null);

        // ACT
        var areEqual = firstDetails == secondDetails;

        // ASSERT
        Assert.IsFalse(areEqual);
        Assert.AreNotEqual(firstDetails, secondDetails);
    }

    [TestMethod]
    [Description("Verifies that contact details with different addresses are not equal.")]
    public void Create_WithDifferentAddress_ProducesDifferentValueObjects()
    {
        // ARRANGE
        var firstAddress = PostalAddress.Create(
            street: "Main Street 10",
            city: "Vienna",
            state: "Vienna",
            country: "Austria",
            zipCode: "1010");

        var secondAddress = PostalAddress.Create(
            street: "Example Street 20",
            city: "Graz",
            state: "Styria",
            country: "Austria",
            zipCode: "8010");

        var firstDetails = ContactDetails.Create(
            email: "john@example.com",
            phone: "+43 660 1234567",
            address: firstAddress);

        var secondDetails = ContactDetails.Create(
            email: "john@example.com",
            phone: "+43 660 1234567",
            address: secondAddress);

        // ACT
        var areEqual = firstDetails == secondDetails;

        // ASSERT
        Assert.IsFalse(areEqual);
        Assert.AreNotEqual(firstDetails, secondDetails);
    }
}