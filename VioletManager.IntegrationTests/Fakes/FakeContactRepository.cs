using VioletManager.Application.Interfaces;
using VioletManager.Domain.Entities;

namespace VioletManager.IntegrationTests.Fakes;

/// <summary>
/// In-memory <see cref="IContactRepository"/> used to run the API without real persistence.
/// </summary>
public sealed class FakeContactRepository : IContactRepository
{
    private readonly Lock _gate = new();
    private readonly List<Contact> _contacts = [];

    public IReadOnlyList<Contact> Contacts
    {
        get
        {
            lock (_gate)
            {
                return _contacts.ToArray();
            }
        }
    }

    public Task AddAsync(Contact contact, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            _contacts.Add(contact);
        }

        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(Guid contactId, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var removed = _contacts.RemoveAll(contact => contact.Id == contactId);

            return Task.FromResult(removed > 0);
        }
    }

    public Task<Contact?> GetAsync(Guid contactId, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var contact = _contacts.SingleOrDefault(candidate => candidate.Id == contactId);

            return Task.FromResult(contact);
        }
    }
}
