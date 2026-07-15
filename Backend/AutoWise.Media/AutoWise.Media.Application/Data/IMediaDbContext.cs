using AutoWise.CommonUtilities.Persistence.Abstractions;
using AutoWise.Media.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoWise.Media.Application.Data;

public interface IMediaDbContext : IBaseDbContext
{
    DbSet<MediaFile> MediaFiles { get; }
    DbSet<MediaAttachment> MediaAttachments { get; }
}
