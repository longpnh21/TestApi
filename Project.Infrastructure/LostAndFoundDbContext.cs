using Microsoft.EntityFrameworkCore;
using Project.Core.Entities;
using Project.Core.Interfaces;
using Project.Infrastructure.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Project.Infrastructure
{
    public class LostAndFoundDbContext : DbContext, IApplicationDbContext
    {

        public LostAndFoundDbContext(DbContextOptions<LostAndFoundDbContext> options)
        : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<LostProperty> LostProperties { get; set; }
        public DbSet<Location> Locations { get; set; }

        public void MarkAsModified(object item) => Entry(item).State = EntityState.Modified;

        public bool IsEntityDetached(object item) => Entry(item).State == EntityState.Detached;

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<ICreationTime>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreationTime = DateTime.UtcNow;
                }
            }

            foreach (var entry in ChangeTracker.Entries<IModificationTime>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.LastModifiedTime = DateTime.UtcNow;
                }
            }

            foreach (var entry in ChangeTracker.Entries<ISoftDelete>())
            {
                if (entry.State == EntityState.Deleted)
                {
                    if (entry.Entity.IsSoftDelete)
                    {
                        entry.Entity.IsDelete = true;
                        entry.State = EntityState.Modified;
                    }
                }
            }

            int result = await base.SaveChangesAsync(cancellationToken);
            return result;
        }
    }
}
