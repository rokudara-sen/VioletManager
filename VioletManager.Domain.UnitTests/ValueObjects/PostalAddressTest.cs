using JetBrains.Annotations;
using VioletManager.Domain.ValueObjects;

namespace VioletManager.Domain.UnitTests.ValueObjects;

[TestClass]
[TestSubject(typeof(PostalAddress))]
public sealed class PostalAddressTests
{
    [TestMethod]
    [Description("Verifies that postal address values are trimmed.")]
    public void Create_WithWhitespaceAroundValues_TrimsValues()
    {
        // ARRANGE
        const string street = "  Main Street 10  ";
        const string city = "  Vienna  ";
        const string state = "  Vienna  ";
        const string country = "  Austria  ";
        const string zipCode = "  1010  ";

        // ACT
        var address = PostalAddress.Create(
            street,
            city,
            state,
            country,
            zipCode);

        // ASSERT
        Assert.AreEqual("Main Street 10", address.Street);
        Assert.AreEqual("Vienna", address.City);
        Assert.AreEqual("Vienna", address.State);
        Assert.AreEqual("Austria", address.Country);
        Assert.AreEqual("1010", address.ZipCode);
    }

    [TestMethod]
    [Description("Verifies that blank postal address values are normalized to null.")]
    public void Create_WithBlankValues_NormalizesValuesToNull()
    {
        // ACT
        var address = PostalAddress.Create(
            street: "",
            city: " ",
            state: "\t",
            country: "   ",
            zipCode: null);

        // ASSERT
        Assert.IsNull(address.Street);
        Assert.IsNull(address.City);
        Assert.IsNull(address.State);
        Assert.IsNull(address.Country);
        Assert.IsNull(address.ZipCode);
    }

    [TestMethod]
    [Description("Verifies that postal addresses with the same values are equal.")]
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

        // ACT
        var areEqual = firstAddress == secondAddress;

        // ASSERT
        Assert.IsTrue(areEqual);
        Assert.AreEqual(firstAddress, secondAddress);
        Assert.AreNotSame(firstAddress, secondAddress);
    }

    [TestMethod]
    [Description("Verifies that postal addresses with different values are not equal.")]
    public void Create_WithDifferentValues_ProducesDifferentValueObjects()
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

        // ACT
        var areEqual = firstAddress == secondAddress;

        // ASSERT
        Assert.IsFalse(areEqual);
        Assert.AreNotEqual(firstAddress, secondAddress);
    }
}