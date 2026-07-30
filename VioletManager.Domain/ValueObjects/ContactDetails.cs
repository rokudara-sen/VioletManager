namespace VioletManager.Domain.ValueObjects;

public sealed record ContactDetails
{
    public string? Email { get; }
    public string? Phone { get; }
    public PostalAddress? Address { get; }

    private ContactDetails(
        string? email,
        string? phone,
        PostalAddress? address)
    {
        Email = Normalize(email);
        Phone = Normalize(phone);
        Address = address;
    }

    public static ContactDetails Create(
        string? email,
        string? phone,
        PostalAddress? address)
    {
        return new ContactDetails(email, phone, address);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}