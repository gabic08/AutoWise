using AutoWise.CommonUtilities.Persistence.PostgreSQL.Configurations;
using AutoWise.Users.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWise.Users.Infrastructure.Data.Configurations;

public class UserConfiguration : ModifiedCreatedAuditEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);
        builder.ToTable(nameof(User), InfrastructureDataConstants.UsersSchema);

        builder.Property(u => u.ExternalId).IsRequired();
        builder.Property(u => u.Provider).IsRequired();
        builder.Property(u => u.Email).IsRequired();

        builder.HasIndex(u => new { u.Provider, u.ExternalId }).IsUnique();
        builder.HasIndex(u => new { u.Provider, u.Email }).IsUnique();
    }
}
