using ExpenseTracker.Domain.Primitives;

namespace ExpenseTracker.Application.Abstractions;

public interface IDomainEventHandler<T> where T : DomainEvent
{
    Task Handle(T domainEvent, CancellationToken cancellationToken = default);
}
