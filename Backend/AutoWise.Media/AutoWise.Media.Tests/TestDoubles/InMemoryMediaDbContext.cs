using AutoWise.Media.Application.Data;
using AutoWise.Media.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoWise.Media.Tests.TestDoubles;

public class InMemoryMediaDbContext(DbContextOptions<InMemoryMediaDbContext> options)
    : DbContext(options), IMediaDbContext
{
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
    public DbSet<MediaAttachment> MediaAttachments => Set<MediaAttachment>();

    public static InMemoryMediaDbContext Create()
    {
        var options = new DbContextOptionsBuilder<InMemoryMediaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new InMemoryMediaDbContext(options);
    }
}
