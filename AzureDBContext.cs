using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore.SqlServer;
using System.ComponentModel.DataAnnotations.Schema;


namespace AzureSQLDBConnectionAPI
{
    public class AzureDBContext : DbContext
    {
        public AzureDBContext(DbContextOptions opions):base(opions)
        {
            var conn = (Microsoft.Data.SqlClient.SqlConnection)Database.GetDbConnection();
            var opt = new DefaultAzureCredentialOptions() { ExcludeSharedTokenCacheCredential = true };
            var credential = new DefaultAzureCredential(opt);
            var token = credential
                    .GetToken(new Azure.Core.TokenRequestContext(
                        new[] { "https://database.windows.net/.default" }));
            conn.AccessToken = token.Token;
        }
        public DbSet<Versions> Version { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Versions>()
            .ToTable("VersionInfo")
            .HasNoKey(); 
        }
    }
    public class Versions
    {
        [Column("version")]
        public string? Appversion { get; set; }
    }
}
