using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DistributedTransactions.Data;

public abstract class BaseDbContext : DbContext
{
    abstract protected string GetDatabaseName();
    abstract protected Type GetDomainModelType();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connStringBuilder = new NpgsqlConnectionStringBuilder();
        connStringBuilder.SslMode = SslMode.Disable;
        // connStringBuilder.IncludeErrorDetails = true;
        string databaseUrlEnv = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (databaseUrlEnv == null) {
            connStringBuilder.Host = "localhost";
            connStringBuilder.Port = 26257;
            connStringBuilder.Username = "root";
        } else {
            Uri databaseUrl = new Uri(databaseUrlEnv);
            connStringBuilder.Host = databaseUrl.Host;
            connStringBuilder.Port = databaseUrl.Port;
            var items = databaseUrl.UserInfo.Split(new[] { ':' });
            if (items.Length > 0) connStringBuilder.Username = items[0];
            if (items.Length > 1) connStringBuilder.Password = items[1];
        }
        connStringBuilder.Database = GetDatabaseName();
        optionsBuilder.UseNpgsql(connStringBuilder.ConnectionString)
            // .UseLazyLoadingProxies()
            // .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .LogTo(Console.WriteLine, LogLevel.Warning)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            Assembly.GetExecutingAssembly(),
            t => t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>) &&
                GetDomainModelType().IsAssignableFrom(i.GenericTypeArguments[0])
            )
        );
    }
}
