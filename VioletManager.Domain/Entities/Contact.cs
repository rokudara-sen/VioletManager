using VioletManager.Domain.ValueObjects;

namespace VioletManager.Domain.Entities;

public sealed class Contact
{
    private readonly List<string> _categories = [];
    private readonly List<string> _subcategories = [];

    public Guid Id { get; private set; }

    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }

    public ContactDetails? WorkDetails { get; private set; }
    public ContactDetails? PrivateDetails { get; private set; }

    public string? CompanyName { get; private set; }
    public string? AdditionalNotes { get; private set; }

    public IReadOnlyCollection<string> Categories =>
        _categories.AsReadOnly();

    public IReadOnlyCollection<string> Subcategories =>
        _subcategories.AsReadOnly();

    private Contact(
        Guid id,
        string? firstName,
        string? lastName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Contact ID cannot be empty.",
                nameof(id));
        }

        Id = id;
        SetName(firstName, lastName);
    }

    public static Contact Create(
        string? firstName,
        string? lastName)
    {
        return new Contact(
            Guid.NewGuid(),
            firstName,
            lastName);
    }

    public void SetName(
        string? firstName,
        string? lastName)
    {
        firstName = Normalize(firstName);
        lastName = Normalize(lastName);

        if (firstName is null && lastName is null)
        {
            throw new ArgumentException(
                "At least one of first name or last name must be provided.");
        }

        FirstName = firstName;
        LastName = lastName;
    }

    public void SetWorkDetails(ContactDetails? workDetails)
    {
        WorkDetails = workDetails;
    }

    public void ClearWorkDetails()
    {
        WorkDetails = null;
    }

    public void SetPrivateDetails(ContactDetails? privateDetails)
    {
        PrivateDetails = privateDetails;
    }

    public void ClearPrivateDetails()
    {
        PrivateDetails = null;
    }

    public void SetCompanyName(string? companyName)
    {
        CompanyName = Normalize(companyName);
    }

    public void SetAdditionalNotes(string? additionalNotes)
    {
        AdditionalNotes = Normalize(additionalNotes);
    }

    public void AddCategory(string category)
    {
        category = NormalizeRequired(
            category,
            nameof(category));

        var alreadyExists = _categories.Contains(
            category,
            StringComparer.OrdinalIgnoreCase);

        if (alreadyExists)
        {
            return;
        }

        _categories.Add(category);
    }

    public void RemoveCategory(string category)
    {
        category = NormalizeRequired(
            category,
            nameof(category));

        _categories.RemoveAll(existing =>
            string.Equals(
                existing,
                category,
                StringComparison.OrdinalIgnoreCase));
    }

    public void ClearCategories()
    {
        _categories.Clear();
    }

    public void AddSubcategory(string subcategory)
    {
        subcategory = NormalizeRequired(
            subcategory,
            nameof(subcategory));

        var alreadyExists = _subcategories.Contains(
            subcategory,
            StringComparer.OrdinalIgnoreCase);

        if (alreadyExists)
        {
            return;
        }

        _subcategories.Add(subcategory);
    }

    public void RemoveSubcategory(string subcategory)
    {
        subcategory = NormalizeRequired(
            subcategory,
            nameof(subcategory));

        _subcategories.RemoveAll(existing =>
            string.Equals(
                existing,
                subcategory,
                StringComparison.OrdinalIgnoreCase));
    }

    public void ClearSubcategories()
    {
        _subcategories.Clear();
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static string NormalizeRequired(
        string? value,
        string parameterName)
    {
        return Normalize(value)
            ?? throw new ArgumentException(
                "Value cannot be null, empty, or whitespace.",
                parameterName);
    }

    // REQUIRED BY EF CORE
    private Contact()
    {
    }
}