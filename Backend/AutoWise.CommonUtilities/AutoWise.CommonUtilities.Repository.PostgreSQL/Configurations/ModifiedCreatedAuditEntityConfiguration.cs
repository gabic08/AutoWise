using AutoWise.CommonUtilities.Models.BaseEntities.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWise.CommonUtilities.Persistence.PostgreSQL.Configurations;

public abstract class ModifiedCreatedAuditEntityConfiguration<TEntity> : CreatedAuditEntityConfiguration<TEntity>
    where TEntity : class, IModifiedCreatedAuditBaseEntity
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder); // applies Id + CreatedAudit configuration

        builder.Property(x => x.LastModifiedOn)
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        builder.Property(x => x.LastModifiedByUserId)
            .IsRequired(false);
    }
}

public abstract class ModifiedCreatedAuditEntityConfiguration<TEntity, TUser> : ModifiedCreatedAuditEntityConfiguration<TEntity>
    where TEntity : class, IModifiedCreatedAuditBaseEntity<TUser>
    where TUser : class
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder); // applies Id + CreatedAudit + ModifiedAudit configuration

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LastModifiedByUser)
            .WithMany()
            .HasForeignKey(x => x.LastModifiedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}