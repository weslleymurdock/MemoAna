using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MemoAna.Infrastructure.Persistence;

internal sealed class GameDbContextFactory : IDesignTimeDbContextFactory<GameDbContext>
{
    public GameDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GameDbContext>();
        optionsBuilder.UseSqlite("Data Source=design_time_database.db3");
        return new GameDbContext(optionsBuilder.Options);
    }
}