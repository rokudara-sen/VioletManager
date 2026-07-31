using VioletManager.Application.Contacts.Commands;
using VioletManager.Application.Interfaces;

namespace VioletManager.Application.Handlers.Contacts;

public sealed class DeleteContactHandler
{
    private readonly IContactRepository _contactRepository;

    public DeleteContactHandler(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }
    
    public async Task<bool> HandleAsync(
        DeleteContactCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.ContactId == Guid.Empty)
        {
            throw new ArgumentException(
                "Contact ID cannot be empty.",
                nameof(command));
        }

        return await _contactRepository.DeleteAsync(
            command.ContactId,
            cancellationToken);
    }
}
