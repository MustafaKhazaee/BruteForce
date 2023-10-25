
namespace BruteForce.Domain.Entities;

/// <summary>
/// Base class for all other entities
/// </summary>
/// <typeparam name="T">Specifies the type of primary key (usually int or long)</typeparam>
public abstract class AggregateRoot<TKey> where TKey : IComparable
{
    public TKey Id { get; private set; }
}
