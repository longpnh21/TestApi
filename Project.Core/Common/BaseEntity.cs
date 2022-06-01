using Project.Core.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Core.Common
{
    public class BaseEntity : IAuditableEntity
    {
        [Key]
        public virtual object[] Id { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? LastModifiedTime { get; set; }
        public bool IsDelete { get; set; }
        [NotMapped]
        public bool IsSoftDelete { get; set; } = true;
    }

    public class BaseEntity<TKey> : BaseEntity, IAuditableEntity<TKey>
    {
        [Key]
        public virtual TKey Id { get; set; }
    }
}
