namespace VioletManager.Application.Contacts.Commands;

public sealed record CreateContactCommand(
    string? FirstName,
    string? LastName,
    string? WorkEmail,
    string? WorkPhone,
    string? WorkAddress,
    string? WorkCity,
    string? WorkState,
    string? WorkCountry,
    string? WorkZipCode,
    string? PrivateEmail,
    string? PrivatePhone,
    string? PrivateAddress,
    string? PrivateCity,
    string? PrivateState,
    string? PrivateCountry,
    string? PrivateZipCode,
    string? CompanyName,
    string? AdditionalNotes);