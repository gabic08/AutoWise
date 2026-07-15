using AutoWise.CommonUtilities.Persistence.PostgreSQL.Configurations;
using AutoWise.Media.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWise.Media.Infrastructure.Data.Configurations;

public class MediaFileConfiguration : CreatedAuditEntityConfiguration<MediaFile>
{
    public override void Configure(EntityTypeBuilder<MediaFile> builder)
    {
        base.Configure(builder);
        builder.ToTable(nameof(MediaFile), InfrastructureDataConstants.MediaSchema);

        builder.Property(mf => mf.ContentHash)
            .HasMaxLength(64)
            .IsRequired();
        builder.HasIndex(mf => mf.ContentHash).IsUnique();

        builder.Property(mf => mf.ContentType).IsRequired();
        builder.Property(mf => mf.FileExtension).IsRequired();
        builder.Property(mf => mf.SizeInBytes).IsRequired();
        builder.Property(mf => mf.StorageProvider).IsRequired();
        builder.Property(mf => mf.StorageKey).IsRequired();

        builder.HasMany(mf => mf.MediaAttachments)
            .WithOne(ma => ma.MediaFile)
            .HasForeignKey(ma => ma.MediaFileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
