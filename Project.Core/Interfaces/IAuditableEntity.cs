namespace Project.Core.Interfaces
{
    public interface IAuditableEntity : IEntity, ICreationTime, IModificationTime, ISoftDelete
    {
    }

    public interface IAuditableEntity<TKey> : IEntity<TKey>, IAuditableEntity
    {
    }
}
