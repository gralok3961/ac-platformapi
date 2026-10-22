using Base.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Base.Data
{
    public abstract class BaseIdentityDbContext<TUser>(DbContextOptions options) : IdentityDbContext<TUser>(options) where TUser : IdentityUser
    {
        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;

            var entries = ChangeTracker.Entries().Where(e => e.Entity is BaseDto && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (BaseDto)entry.Entity;
                entity.DateUpdated = now;
                if (entry.State == EntityState.Added) entity.DateCreated = now;
            }

            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}
