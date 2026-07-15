using AutoWise.CommonUtilities.Persistence.PostgreSQL.Context;
using AutoWise.Media.Application.Data;
using AutoWise.Media.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoWise.Media.Infrastructure.Data;

public class MediaDbContext(DbContextOptions<MediaDbContext> options) : DbContext(options), IMediaDbContext
{
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
    public DbSet<MediaAttachment> MediaAttachments => Set<MediaAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureDatabaseWithSchema(InfrastructureDataConstants.MediaSchema);
    }
}
