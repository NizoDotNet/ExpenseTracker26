namespace ExpenseTracker.Domain.Primitives;

public abstract class Entity<T> : IEquatable<Entity<T>>
{
    public T Id { get; protected set; }
    public bool Equals(Entity<T>? other)
        => other is not null && EqualityComparer<T>.Default.Equals(this.Id, other.Id);

    public override bool Equals(object? obj)
        => obj is not null && Equals((Entity<T>)obj);
}
