using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MottuProjeto.Data
{
    // Usado SOMENTE pelo 'dotnet ef' em tempo de design
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var cfg = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var conn = Environment.GetEnvironmentVariable("ORACLE_CONN")
                      ?? cfg.GetConnectionString("Default");

            if (string.IsNullOrWhiteSpace(conn))
                throw new InvalidOperationException(
                    "Defina ORACLE_CONN (variável de ambiente) ou ConnectionStrings:Default (appsettings.json).");

            var opts = new DbContextOptionsBuilder<AppDbContext>()
                .UseOracle(conn)
                .Options;

            return new AppDbContext(opts);
        }
    }
}
