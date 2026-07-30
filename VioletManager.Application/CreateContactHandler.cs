using VioletManager.Domain.Entities;
using VioletManager.Domain.ValueObjects;

namespace VioletManager.Application;

public sealed class CreateContactHandler
{
    private readonly IContactRepository _contactRepository;

    public CreateContactHandler(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    public async Task<CreateContactResult> HandleAsync(
        CreateContactCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var contact = Contact.Create(
            command.FirstName,
            command.LastName);

        var workDetails = CreateContactDetails(
            command.WorkEmail,
            command.WorkPhone,
            command.WorkAddress,
            command.WorkCity,
            command.WorkState,
            command.WorkCountry,
            command.WorkZipCode);

        var privateDetails = CreateContactDetails(
            command.PrivateEmail,
            command.PrivatePhone,
            command.PrivateAddress,
            command.PrivateCity,
            command.PrivateState,
            command.PrivateCountry,
            command.PrivateZipCode);

        if (workDetails is not null)
        {
            contact.SetWorkDetails(workDetails);
        }

        if (privateDetails is not null)
        {
            contact.SetPrivateDetails(privateDetails);
        }

        contact.SetCompanyName(command.CompanyName);
        contact.SetAdditionalNotes(command.AdditionalNotes);

        await _contactRepository.AddAsync(
            contact,
            cancellationToken);

        return new CreateContactResult(contact.Id);
    }

    private static ContactDetails? CreateContactDetails(
        string? email,
        string? phone,
        string? street,
        string? city,
        string? state,
        string? country,
        string? zipCode)
    {
        var hasAddress = HasValue(street)
                         || HasValue(city)
                         || HasValue(state)
                         || HasValue(country)
                         || HasValue(zipCode);

        var hasDetails = HasValue(email)
                         || HasValue(phone)
                         || hasAddress;

        if (!hasDetails)
        {
            return null;
        }

        var address = hasAddress
            ? PostalAddress.Create(
                street,
                city,
                state,
                country,
                zipCode)
            : null;

        return ContactDetails.Create(
            email,
            phone,
            address);
    }

    private static bool HasValue(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }
}