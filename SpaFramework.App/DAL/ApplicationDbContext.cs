using SpaFramework.App.Models.Data.Accounts;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SpaFramework.App.Models.Data;
using SpaFramework.App.Models.Data.Content;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;
using SpaFramework.App.Models.Data.Donors;
using SpaFramework.App.Models.Data.ChangeTracking;
using SpaFramework.App.Models;
using SpaFramework.Core.Models;
using Newtonsoft.Json;
using SpaFramework.App.Models.Data.Jobs;
using SpaFramework.App.Models.Data.Clients;

namespace SpaFramework.App.DAL
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, long, IdentityUserClaim<long>, ApplicationUserRole, IdentityUserLogin<long>, IdentityRoleClaim<long>, IdentityUserToken<long>>
    {
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<ExternalCredential> ExternalCredentials { get; set; }
        public DbSet<ApplicationUserRole> ApplicationUserRoles { get; set; }

        public DbSet<ContentBlock> ContentBlocks { get; set; }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ClientStats> ClientStats { get; set; }

        public DbSet<ClientTrackedChange> ClientsTrackedChanges { get; set; }
        public DbSet<ProjectTrackedChange> ProjectsTrackedChanges { get; set; }

        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobItem> JobItems { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            /* Accounts */

            modelBuilder.Entity<ExternalCredential>()
                .HasOne(x => x.ApplicationUser)
                .WithMany(x => x.ExternalCredentials)
                .HasForeignKey(x => x.ApplicationUserId);

            modelBuilder.Entity<IdentityUserRole<long>>()
                .HasKey(x => new { x.UserId, x.RoleId });

            modelBuilder.Entity<ApplicationUserRole>()
                .HasIndex(x => x.Id)
                .IsUnique();

            modelBuilder.Entity<ApplicationUserRole>()
                .HasOne(x => x.ApplicationRole)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.RoleId)
                .IsRequired();

            modelBuilder.Entity<ApplicationUserRole>()
                .HasOne(x => x.ApplicationUser)
                .WithMany(x => x.Roles)
                .HasForeignKey(x => x.UserId)
                .IsRequired();

            /* Content */

            modelBuilder.Entity<ContentBlock>()
                .HasIndex(x => x.Slug)
                .IsUnique(false);

            modelBuilder.Entity<ContentBlock>()
                .Property(x => x.AllowedTokens)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<AllowedToken>>(v)
                );

            /* Clients */

            modelBuilder.Entity<Client>()
                .HasIndex(x => x.Abbreviation);

            modelBuilder.Entity<Project>()
                .HasOne(x => x.Client)
                .WithMany(x => x.Projects)
                .HasForeignKey(x => x.ClientId);

            modelBuilder.Entity<ClientStats>()
                .ToView("ClientStats")
                .HasKey(x => x.ClientId);

            modelBuilder.Entity<ClientStats>()
                .HasOne(x => x.Client)
                .WithOne(x => x.ClientStats)
                .HasForeignKey<ClientStats>(x => x.ClientId);

            BuildChangeTrackedModel<Client, long, ClientTrackedChange>(modelBuilder);
            BuildChangeTrackedModel<Project, long, ProjectTrackedChange>(modelBuilder);

            /* Jobs */

            modelBuilder.Entity<JobItem>()
                .HasOne(x => x.Job)
                .WithMany(x => x.JobItems)
                .HasForeignKey(x => x.JobId);

            modelBuilder.Entity<Job>()
                .HasIndex(x => x.Created);

            /* Seed Data */

            SeedData(modelBuilder);
        }

        private void BuildChangeTrackedModel<T, TId, TTrackedChange>(ModelBuilder modelBuilder)
            where T : class, IHasId<TId>, IChangeTracked<T, TId, TTrackedChange>
            where TTrackedChange : TrackedChange<T, TId>
        {
            modelBuilder.Entity<T>()
                .HasMany(x => x.TrackedChanges)
                .WithOne(x => x.Entity)
                .HasForeignKey(x => x.EntityId);

            modelBuilder.Entity<TTrackedChange>()
                .HasOne(x => x.ApplicationUser);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            SeedRoles(modelBuilder);
            SeedUsers(modelBuilder);
            SeedContent(modelBuilder);
            SeedAppData(modelBuilder);
        }

        private static class RoleIds
        {
            public const long SuperAdmin = 100;
            public const long ProjectManager = 101;
            public const long ProjectViewer = 102;
            public const long ContentManager = 103;
        }

        private void SeedRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationRole>().HasData(new ApplicationRole()
            {
                Id = RoleIds.SuperAdmin,
                Name = ApplicationRoleNames.SuperAdmin,
                NormalizedName = ApplicationRoleNames.SuperAdmin
            });

            modelBuilder.Entity<ApplicationRole>().HasData(new ApplicationRole()
            {
                Id = RoleIds.ProjectManager,
                Name = ApplicationRoleNames.ProjectManager,
                NormalizedName = ApplicationRoleNames.ProjectManager
            });

            modelBuilder.Entity<ApplicationRole>().HasData(new ApplicationRole()
            {
                Id = RoleIds.ProjectViewer,
                Name = ApplicationRoleNames.ProjectViewer,
                NormalizedName = ApplicationRoleNames.ProjectViewer
            });

            modelBuilder.Entity<ApplicationRole>().HasData(new ApplicationRole()
            {
                Id = RoleIds.ContentManager,
                Name = ApplicationRoleNames.ContentManager,
                NormalizedName = ApplicationRoleNames.ContentManager
            });
        }

        private void SeedUsers(ModelBuilder modelBuilder)
        {
            var passwordHasher = new PasswordHasher<ApplicationUser>();

            modelBuilder.Entity<ApplicationUser>().HasData(new ApplicationUser()
            {
                Id = 101,
                UserName = "admin",
                NormalizedUserName = "admin".ToUpper(),
                Email = "admin@test.com",
                NormalizedEmail = "admin@test.com".ToUpper(),
                EmailConfirmed = true,
                PasswordHash = passwordHasher.HashPassword(null, "abcd1234"),
                SecurityStamp = string.Empty,
                FirstName = "Admin",
                LastName = "Admin"
            });

            modelBuilder.Entity<ApplicationUser>().HasData(new ApplicationUser()
            {
                Id = 102,
                UserName = "chris.wilson@northwoodsoft.com",
                NormalizedUserName = "chris.wilson@northwoodsoft.com".ToUpper(),
                Email = "chris.wilson@northwoodsoft.com",
                NormalizedEmail = "chris.wilson@northwoodsoft.com".ToUpper(),
                EmailConfirmed = true,
                PasswordHash = passwordHasher.HashPassword(null, "abcd1234"),
                SecurityStamp = string.Empty,
                FirstName = "Chris",
                LastName = "Wilson"
            });

            foreach (long userId in new long[] { 101, 102 })
            {
                foreach (long roleId in new long[] { RoleIds.SuperAdmin, RoleIds.ProjectManager, RoleIds.ProjectViewer, RoleIds.ContentManager })
                {
                    modelBuilder.Entity<ApplicationUserRole>().HasData(new ApplicationUserRole()
                    {
                        RoleId = roleId,
                        UserId = userId
                    });
                }
            }

            modelBuilder.Entity<ApplicationUser>().HasData(new ApplicationUser()
            {
                Id = 103,
                UserName = "john.doe@test.com",
                NormalizedUserName = "john.doe@test.com".ToUpper(),
                Email = "john.doe@test.com",
                NormalizedEmail = "john.doe@test.com".ToUpper(),
                EmailConfirmed = true,
                PasswordHash = passwordHasher.HashPassword(null, "abcd1234"),
                SecurityStamp = string.Empty,
                FirstName = "John",
                LastName = "Doe"
            });

            foreach (long roleId in new long[] { RoleIds.ProjectManager, RoleIds.ProjectViewer })
            {
                modelBuilder.Entity<ApplicationUserRole>().HasData(new ApplicationUserRole()
                {
                    RoleId = roleId,
                    UserId = 103
                });
            }

            modelBuilder.Entity<ApplicationUser>().HasData(new ApplicationUser()
            {
                Id = 104,
                UserName = "jane.smith@test.com",
                NormalizedUserName = "jane.smith@test.com".ToUpper(),
                Email = "jane.smith@test.com",
                NormalizedEmail = "jane.smith@test.com".ToUpper(),
                EmailConfirmed = true,
                PasswordHash = passwordHasher.HashPassword(null, "abcd1234"),
                SecurityStamp = string.Empty,
                FirstName = "Jane",
                LastName = "Smith"
            });

            foreach (long roleId in new long[] { RoleIds.ProjectViewer })
            {
                modelBuilder.Entity<ApplicationUserRole>().HasData(new ApplicationUserRole()
                {
                    RoleId = roleId,
                    UserId = 104
                });
            }

            modelBuilder.Entity<ApplicationUser>().HasData(new ApplicationUser()
            {
                Id = 105,
                UserName = "jorge.garcia@test.com",
                NormalizedUserName = "jorge.garcia@test.com".ToUpper(),
                Email = "jorge.garcia@test.com",
                NormalizedEmail = "jorge.garcia@test.com".ToUpper(),
                EmailConfirmed = true,
                PasswordHash = passwordHasher.HashPassword(null, "abcd1234"),
                SecurityStamp = string.Empty,
                FirstName = "Jorge",
                LastName = "Garcia"
            });

            foreach (long roleId in new long[] { RoleIds.ContentManager })
            {
                modelBuilder.Entity<ApplicationUserRole>().HasData(new ApplicationUserRole()
                {
                    RoleId = roleId,
                    UserId = 105
                });
            }

        }

        private void SeedContent(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContentBlock>().HasData(new ContentBlock()
            {
                Id = 100,
                Slug = "password-reset-email",
                IsPage = false,
                Description = "The text that appears in a password reset message",
                Title = "Reset Your Password",
                Value = @"To reset your account, follow this link: %passwordResetUrl%",
                AllowedTokens = new List<AllowedToken>()
                {
                    new AllowedToken()
                    {
                        Token = "passwordResetUrl",
                        Description = "The URL for the user to reset their password"
                    }
                }
            });

            modelBuilder.Entity<ContentBlock>().HasData(new ContentBlock()
            {
                Id = 101,
                Slug = "about",
                IsPage = true,
                Description = "The text that appears on the About page",
                Title = "About Us",
                Value = "About us..."
            });

            modelBuilder.Entity<ContentBlock>().HasData(new ContentBlock()
            {
                Id = 102,
                Slug = "placeholder",
                IsPage = true,
                Description = "",
                Title = "Placeholder",
                Value = "This is a placeholder page. The underlying functionality has not yet been implemented."
            });

            modelBuilder.Entity<ContentBlock>().HasData(new ContentBlock()
            {
                Id = 103,
                Slug = "dashboard",
                IsPage = false,
                Description = "Content that appears on the Home/Dashboard page",
                Title = "Hello",
                Value = "Hello, world. Or whoever else is here. This content is editable within the app."
            });

            modelBuilder.Entity<ContentBlock>().HasData(new ContentBlock()
            {
                Id = 104,
                Slug = "help",
                IsPage = true,
                Description = "The help page that appears in the top nav",
                Title = "Help!",
                Value = "Need help? Don't we all."
            });
        }

        private void SeedAppData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>().HasData(new Client()
            {
                Id = 100,
                Name = "Acme, Inc.",
                Abbreviation = "ACME"
            });

            modelBuilder.Entity<Client>().HasData(new Client()
            {
                Id = 101,
                Name = "Northwoods",
                Abbreviation = "NWS"
            });

            modelBuilder.Entity<Project>().HasData(new Project()
            {
                Id = 100,
                ClientId = 100,
                Name = "Operation Purple Midnight",
                StartDate = new NodaTime.LocalDate(2021, 1, 1),
                EndDate = new NodaTime.LocalDate(2022, 12, 31),
                State = ProjectState.Open
            });

            modelBuilder.Entity<Project>().HasData(new Project()
            {
                Id = 101,
                ClientId = 101,
                Name = "Rapidest",
                StartDate = new NodaTime.LocalDate(2016, 9, 1),
                EndDate = new NodaTime.LocalDate(2019, 3, 30),
                State = ProjectState.Closed
            });

            modelBuilder.Entity<Project>().HasData(new Project()
            {
                Id = 102,
                ClientId = 101,
                Name = "Rapidester",
                StartDate = new NodaTime.LocalDate(2021, 3, 1),
                EndDate = new NodaTime.LocalDate(2021, 6, 30),
                State = ProjectState.OnHold
            });

            modelBuilder.Entity<Project>().HasData(new Project()
            {
                Id = 103,
                ClientId = 101,
                Name = "Rapidesterester",
                StartDate = new NodaTime.LocalDate(2021, 7, 1),
                EndDate = new NodaTime.LocalDate(2022, 6, 30),
                State = ProjectState.Open
            });
        }
    }
}
