using VioletManager.Application;
using VioletManager.Application.Interfaces;
using VioletManager.Domain.Entities;

namespace VioletManager.Application.UnitTests.Fakes;

/// <summary>
/// In-memory <see cref="IContactRepository"/> that records what was added, deleted, and read.
/// </summary>
public sealed class FakeContactRepository : IContactRepository
{
    private readonly List<Contact> _contacts = [];

    public IReadOnlyList<Contact> Contacts => _contacts;

    public int AddCallCount { get; private set; }

    public int DeleteCallCount { get; private set; }

    public Guid? LastDeletedContactId { get; private set; }

    public int GetCallCount { get; private set; }

    public Guid? LastRequestedContactId { get; private set; }

    public CancellationToken LastCancellationToken { get; private set; }

    public Exception? ExceptionToThrow { get; set; }

    public Task AddAsync(Contact contact, CancellationToken cancellationToken = default)
    {
        AddCallCount++;
        LastCancellationToken = cancellationToken;

        if (ExceptionToThrow is not null)
        {
            return Task.FromException(ExceptionToThrow);
        }

        _contacts.Add(contact);

        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(Guid contactId, CancellationToken cancellationToken = default)
    {
        DeleteCallCount++;
        LastDeletedContactId = contactId;
        LastCancellationToken = cancellationToken;

        if (ExceptionToThrow is not null)
        {
            return Task.FromException<bool>(ExceptionToThrow);
        }

        var removed = _contacts.RemoveAll(contact => contact.Id == contactId);

        return Task.FromResult(removed > 0);
    }

    public Task<Contact?> GetAsync(Guid contactId, CancellationToken cancellationToken = default)
    {
        GetCallCount++;
        LastRequestedContactId = contactId;
        LastCancellationToken = cancellationToken;

        if (ExceptionToThrow is not null)
        {
            return Task.FromException<Contact?>(ExceptionToThrow);
        }

        var contact = _contacts.SingleOrDefault(candidate => candidate.Id == contactId);

        return Task.FromResult(contact);
    }
}
