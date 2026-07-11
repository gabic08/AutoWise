using AutoWise.CommonUtilities.Models.BaseEntities.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWise.CommonUtilities.Persistence.PostgreSQL.Configurations;

public abstract class CreatedAuditEntityConfiguration<TEntity> : IdBaseEntityConfiguration<TEntity>
    where TEntity : class, ICreatedAuditBaseEntity
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder); // applies Id configuration

        builder.Property(x => x.CreatedOn)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.CreatedByUserId)
            .IsRequired();
    }
}

public abstract class CreatedAuditEntityConfiguration<TEntity, TUser> : CreatedAuditEntityConfiguration<TEntity>
    where TEntity : class, ICreatedAuditBaseEntity<TUser>
    where TUser : class
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder); // applies Id + CreatedAudit configuration

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}