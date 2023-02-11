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

        public DbSet<AccountInfo> AccountInfos { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<ApplicationInfos> ApplicationInfos { get; set; }
        public DbSet<UserInfo> UserInfos { get; set; }
        public DbSet<RoleInfo> RoleInfos { get; set; }
        public DbSet<UserAccount> UserAccount { get; set; }
        public DbSet<RoleApplication> RoleApplication { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AccountInfoMap());
            modelBuilder.ApplyConfiguration(new ApplicationInfosMap());
            modelBuilder.ApplyConfiguration(new ApplicationMap());
            modelBuilder.ApplyConfiguration(new UserInfoMap());
            modelBuilder.ApplyConfiguration(new RoleInfoMap());
            modelBuilder.ApplyConfiguration(new UserAccountMap());
            modelBuilder.ApplyConfiguration(new RoleApplicationMap());

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            //optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));
        }
    }
}
