using AutoWise.CommonUtilities.Persistence.PostgreSQL.Configurations;
using AutoWise.Media.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWise.Media.Infrastructure.Data.Configurations;

public class MediaAttachmentConfiguration : CreatedAuditEntityConfiguration<MediaAttachment>
{
    public override void Configure(EntityTypeBuilder<MediaAttachment> builder)
    {
        base.Configure(builder);
        builder.ToTable(nameof(MediaAttachment), InfrastructureDataConstants.MediaSchema);

        builder.Property(ma => ma.MediaFileId).IsRequired();
        builder.Property(ma => ma.ParentType).IsRequired();
        builder.Property(ma => ma.ParentEntityId).IsRequired();
        builder.Property(ma => ma.OriginalFileName).IsRequired();

        builder.HasIndex(ma => new { ma.ParentType, ma.ParentEntityId });
    }
}
