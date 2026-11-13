using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PowerCheck.Api.Data;

// Used only by the "dotnet ef" command line tool to create migrations.
// The running app gets its AppDbContext from Program.cs instead.
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=app.db")
            .Options;
        return new AppDbContext(options);
    }
}
