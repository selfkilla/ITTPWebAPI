using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ITTPWebAPI.Models 
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) 
            : base(options)
        {
            Database.EnsureCreated();
        }

        //Предварительно должен быть создан пользователь Admin
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasData(
                new User()
                {
                    Guid = Guid.NewGuid(),
                    Login = "Admin",
                    Password = "Admin123",
                    Name = "Danabek",
                    Gender = 2,
                    Admin = true,
                    CreatedOn = DateTime.Now,
                    CreatedBy = "Admin",
                    ModifiedBy = "Admin",
                    ModifiedOn = DateTime.Now,
                    RevokedBy = null
                });

            modelBuilder.Entity<User>().Property(prop => prop.Admin)
                .HasDefaultValue(false);
            
            modelBuilder.Entity<User>().HasIndex(user => user.Login)
                .IsUnique();
        }

        public DbSet<User> Users { get; set; }

        public async Task<User> GetAdmin(string login, string password)
        {
            return await Users.Where(user => user.Login == login
                && user.Admin == true
                && user.RevokedBy == null)
                .FirstOrDefaultAsync();
        }

        public async Task<User> GetActiveUser(string login)
        {
            return await Users.Where(user => user.Login == login
                && user.RevokedBy == null)
                .FirstOrDefaultAsync();
        }

        public async Task<User> GetUser(string login)
        {
            return await Users.Where(user => user.Login == login)
                .FirstOrDefaultAsync();
        }
    }
}
