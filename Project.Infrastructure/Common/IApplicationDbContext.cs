using Microsoft.EntityFrameworkCore;
using Project.Core.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Project.Infrastructure.Common
{
    public interface IApplicationDbContext : IDisposable
    {
        DbSet<Employee> Employees { get; set; }
        DbSet<LostProperty> LostProperties { get; set; }

        void MarkAsModified(object item);
        bool IsEntityDetached(object item);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}