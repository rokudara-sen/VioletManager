using VioletManager.Domain.Entities;

namespace VioletManager.Application.Interfaces;

public interface IContactRepository
{
    public Task AddAsync(Contact contact, CancellationToken cancellationToken = default);
    public Task<bool> DeleteAsync(Guid contactId, CancellationToken cancellationToken = default);
}