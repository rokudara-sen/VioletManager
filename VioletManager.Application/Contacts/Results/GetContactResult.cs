using VioletManager.Domain.ValueObjects;

namespace VioletManager.Application.Contacts.Results;

public sealed record GetContactResult(
    Guid ContactId,
    string? FirstName,
    string? LastName,
    ContactDetails? WorkDetails,
    ContactDetails? PrivateDetails,
    string? CompanyName,
    string? AdditionalNotes,
    IReadOnlyCollection<string> Categories,
    IReadOnlyCollection<string> Subcategories);
