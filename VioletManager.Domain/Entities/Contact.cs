namespace VioletManager.Domain.Entities;

public sealed class Contact
{
    private readonly List<string> _categories = [];
    private readonly List<string> _subcategories = [];

    public Guid Id { get; private set; }

    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }

    public string? WorkEmail { get; private set; }
    public string? WorkPhone { get; private set; }

    public string? WorkAddress { get; private set; }
    public string? WorkCity { get; private set; }
    public string? WorkState { get; private set; }
    public string? WorkCountry { get; private set; }
    public string? WorkZipCode { get; private set; }

    public string? PrivateEmail { get; private set; }
    public string? PrivatePhone { get; private set; }

    public string? PrivateAddress { get; private set; }
    public string? PrivateCity { get; private set; }
    public string? PrivateState { get; private set; }
    public string? PrivateCountry { get; private set; }
    public string? PrivateZipCode { get; private set; }

    public string? CompanyName { get; private set; }
    public string? AdditionalNotes { get; private set; }

    public IReadOnlyCollection<string> Categories => _categories.AsReadOnly();
    public IReadOnlyCollection<string> Subcategories => _subcategories.AsReadOnly();

    private Contact(
        Guid id,
        string? firstName,
        string? lastName)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Contact ID cannot be empty.", nameof(id));

        Id = id;
        SetName(firstName, lastName);
    }

    public static Contact Create(
        string? firstName,
        string? lastName)
    {
        return new Contact(Guid.NewGuid(), firstName, lastName);
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

    public void SetWorkDetails(
        string? workEmail,
        string? workPhone,
        string? workAddress,
        string? workCity,
        string? workState,
        string? workCountry,
        string? workZipCode)
    {
        WorkEmail = Normalize(workEmail);
        WorkPhone = Normalize(workPhone);
        WorkAddress = Normalize(workAddress);
        WorkCity = Normalize(workCity);
        WorkState = Normalize(workState);
        WorkCountry = Normalize(workCountry);
        WorkZipCode = Normalize(workZipCode);
    }

    public void SetPrivateDetails(
        string? privateEmail,
        string? privatePhone,
        string? privateAddress,
        string? privateCity,
        string? privateState,
        string? privateCountry,
        string? privateZipCode)
    {
        PrivateEmail = Normalize(privateEmail);
        PrivatePhone = Normalize(privatePhone);
        PrivateAddress = Normalize(privateAddress);
        PrivateCity = Normalize(privateCity);
        PrivateState = Normalize(privateState);
        PrivateCountry = Normalize(privateCountry);
        PrivateZipCode = Normalize(privateZipCode);
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
        category = NormalizeRequired(category, nameof(category));

        if (_categories.Contains(category, StringComparer.OrdinalIgnoreCase))
            return;

        _categories.Add(category);
    }

    public void RemoveCategory(string category)
    {
        category = NormalizeRequired(category, nameof(category));

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
        subcategory = NormalizeRequired(subcategory, nameof(subcategory));

        if (_subcategories.Contains(
                subcategory,
                StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        _subcategories.Add(subcategory);
    }

    public void RemoveSubcategory(string subcategory)
    {
        subcategory = NormalizeRequired(subcategory, nameof(subcategory));

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
        string value,
        string parameterName)
    {
        return Normalize(value)
            ?? throw new ArgumentException(
                "Value cannot be null, empty, or whitespace.",
                parameterName);
    }

    // Required by EF Core.
    private Contact()
    {
    }
}