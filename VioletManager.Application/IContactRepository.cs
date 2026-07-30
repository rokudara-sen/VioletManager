using VioletManager.Domain.Entities;

namespace VioletManager.Application;

public interface IContactRepository
{
    public Task AddAsync(Contact contact, CancellationToken cancellationToken = default);
}