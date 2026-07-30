namespace VioletManager.Domain.ValueObjects;

public sealed record PostalAddress
{
    public string? Street { get; }
    public string? City { get; }
    public string? State { get; }
    public string? Country { get; }
    public string? ZipCode { get; }

    private PostalAddress(
        string? street,
        string? city,
        string? state,
        string? country,
        string? zipCode)
    {
        Street = Normalize(street);
        City = Normalize(city);
        State = Normalize(state);
        Country = Normalize(country);
        ZipCode = Normalize(zipCode);
    }

    public static PostalAddress Create(
        string? street,
        string? city,
        string? state,
        string? country,
        string? zipCode)
    {
        return new PostalAddress(
            street,
            city,
            state,
            country,
            zipCode);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}