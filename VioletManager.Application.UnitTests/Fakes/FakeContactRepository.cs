using VioletManager.Application;
using VioletManager.Domain.Entities;

namespace VioletManager.Application.UnitTests.Fakes;

/// <summary>
/// In-memory <see cref="IContactRepository"/> that records what was added.
/// </summary>
public sealed class FakeContactRepository : IContactRepository
{
    private readonly List<Contact> _contacts = [];

    public IReadOnlyList<Contact> Contacts => _contacts;

    public int AddCallCount { get; private set; }

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
}
