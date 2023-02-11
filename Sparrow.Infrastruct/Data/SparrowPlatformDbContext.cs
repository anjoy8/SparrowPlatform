using SparrowPlatform.Domain.Models;
using SparrowPlatform.Infrastruct.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace SparrowPlatform.Infrastruct.Data
{
    public class SparrowPlatformDbContext : DbContext
    {
        public SparrowPlatformDbContext(DbContextOptions<SparrowPlatformDbContext> options)
         : base(options)
        {
        }

        public DbSet<UserInfo> UserInfos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserInfoMap());

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
        }
    }
}
