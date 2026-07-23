using AutoWise.CommonUtilities.Persistence.Abstractions;
using AutoWise.Users.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoWise.Users.Application;

public interface IUsersDbContext : IBaseDbContext
{
    DbSet<User> Users { get; }
}
