using MemoAna.Backend.Infrastructure.Identity.Models;
using MemoAna.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MemoAna.Backend.UnitTests.Common;

/// <summary>Tests database context persistence behaviors.</summary>
public sealed class MemoAnaDbContextTests
{
    [Fact]
    public async Task SaveChanges_HandlesIdentifiersAndSoftDelete()
    {
        await using MemoAna.BackendDbContext context = CreateContext();
        User user = new("user@example.com");
        Role role = new("Operator");
        string userId = user.Id;
        string roleId = role.Id;
        context.Users.Add(user);
        context.Roles.Add(role);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.NotEqual(userId, user.Id);
        Assert.NotEqual(roleId, role.Id);

        context.Users.Remove(user);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.True(user.IsDeleted);
        Assert.NotNull(user.DeletedAt);
        User? persisted = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == user.Id, TestContext.Current.CancellationToken);
        Assert.NotNull(persisted);
        Assert.True(persisted.IsDeleted);
    }

    [Fact]
    public async Task SaveChangesOverloads_ApplyPersistenceHooks()
    {
        await using MemoAna.BackendDbContext context = CreateContext();

        Assert.Equal(0, context.SaveChanges());
        Assert.Equal(0, context.SaveChanges(true));
        Assert.Equal(0, await context.SaveChangesAsync(
            CancellationToken.None));
        Assert.Equal(0, await context.SaveChangesAsync(
            true, CancellationToken.None));
    }

    private static MemoAnaDbContext CreateContext()
    {
        DbContextOptions<MemoAnaDbContext> options =
            new DbContextOptionsBuilder<MemoAnaDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        return new MemoAna.BackendDbContext(options);
    }
}
