using VioletManager.Application.Contacts.Commands;
using VioletManager.Application.Contacts.Results;
using VioletManager.Application.Interfaces;

namespace VioletManager.Application.Handlers.Contacts;

public sealed class GetContactHandler
{
    private readonly IContactRepository _contactRepository;

    public GetContactHandler(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }
    
    public async Task<GetContactResult?> HandleAsync(
        GetContactCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.ContactId == Guid.Empty)
        {
            throw new ArgumentException(
                "Contact ID cannot be empty.",
                nameof(command));
        }

        var contact = await _contactRepository.GetAsync(
            command.ContactId,
            cancellationToken);

        if (contact is null)
        {
            return null;
        }

        return new GetContactResult(
            contact.Id,
            contact.FirstName,
            contact.LastName,
            contact.WorkDetails,
            contact.PrivateDetails,
            contact.CompanyName,
            contact.AdditionalNotes,
            contact.Categories,
            contact.Subcategories);
    }
}
