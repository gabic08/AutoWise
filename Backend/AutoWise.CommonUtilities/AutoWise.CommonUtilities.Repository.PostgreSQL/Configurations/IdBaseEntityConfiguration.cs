using AutoWise.CommonUtilities.Models.BaseEntities.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWise.CommonUtilities.Repository.PostgreSQL.Configurations;

public abstract class IdBaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class, IIdBaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()")
            .IsRequired();
    }
}