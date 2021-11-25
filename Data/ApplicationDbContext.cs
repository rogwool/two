using two.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace two.Data
{
    //changed public class ApplicationDbContext : IdentityDbContext
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<DGroup> DGroups { get; set; } // give Tables aliases for in code
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<GroupMember>().ToTable("GroupMember")
                        .Property(e => e.GroupRoleEz)
                        .HasConversion(v => v.ToString(), v => (GroupRoleEz)Enum.Parse(typeof(GroupRoleEz), v));

            modelBuilder.Entity<GroupMember>().HasKey(p => new { p.Id, p.DGroupId });// suck up concat key

            modelBuilder.Entity<GroupMember>()
            .HasOne(bc => bc.ApplicationUser)
            .WithMany(c => c.GroupMember)
            .HasForeignKey(bc => bc.Id);

            modelBuilder.Entity<GroupMember>()
            .HasOne(bc => bc.DGroup)
            .WithMany(c => c.GroupMember)
            .HasForeignKey(bc => bc.DGroupId);

            modelBuilder.Entity<DGroup>().ToTable("DGroup");

            modelBuilder.Entity<ApplicationUser>()
                         .Property(e => e.FirstName)
                         .HasMaxLength(250);

            modelBuilder.Entity<ApplicationUser>()
                        .Property(e => e.LastName)
                        .HasMaxLength(250);

            /**Note
            https://docs.microsoft.com/en-us/aspnet/mvc/overview/getting-started/getting-started-with-ef-using-mvc/creating-a-more-complex-data-model-for-an-asp-net-mvc-application
            By convention, the Entity Framework enables cascade delete for non-nullable foreign keys
            and for many-to-many relationships. This can result in circular cascade delete rules, 
            which will cause an exception when you try to add a migration. For example, if you didn't define 
            the Department.InstructorID property as nullable, you'd get the following exception message: 
            "The referential relationship will result in a cyclical reference that's not allowed."
            If your business rules required InstructorID property to be non-nullable, you would have to use the following 
            fluent API statement to disable cascade delete on the relationship:

            //modelBuilder.Entity().HasRequired(d => d.Administrator).WithMany().WillCascadeOnDelete(false);
            **/
        }
    }
}
