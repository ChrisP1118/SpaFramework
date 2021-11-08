using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpaFramework.App.DAL;
using SpaFramework.App.Models.Data;
using SpaFramework.App.Models.Data.Accounts;
using SpaFramework.App.Models.Data.Clients;
using SpaFramework.App.Models.Data.Clients.Validators;
using SpaFramework.App.Models.Data.Content;
using SpaFramework.App.Models.Data.Content.Validators;
using SpaFramework.App.Models.Data.Donors.Validators;
using SpaFramework.App.Models.Data.Jobs;
using SpaFramework.App.Models.Data.Jobs.Validators;
using SpaFramework.App.Models.Service.WorkItems.Echo;
using SpaFramework.App.Services.Data;
using SpaFramework.App.Services.Data.Accounts;
using SpaFramework.App.Services.Data.Clients;
using SpaFramework.App.Services.Data.Content;
using SpaFramework.App.Services.Data.Jobs;
using SpaFramework.App.Services.WorkItems;
using SpaFramework.Providers.Twilio.Services;

namespace SpaFramework.App
{
    public static class ServiceBuilder
    {
        /// <summary>
        /// Configures the EF database connection and identity. This is used in both the web and worker layer.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddDataServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options => 
                options.UseSqlServer(configuration.GetConnectionString("Default"), 
                x =>
                {
                    x.UseNodaTime();
                    x.CommandTimeout(300);
                }));

            // Add Identity
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
        }

        /// <summary>
        /// Configures the application services. This is used in both the web and worker layer.
        /// </summary>
        /// <param name="services"></param>
        public static void AddAppServices(this IServiceCollection services)
        {
            services.AddEntityServices();
            services.AddEntityValidators();
            services.AddWorkItemServices();

            services.AddHttpClient();

            services.AddTransient<ITwilioSmsService, TwilioSmsService>();
        }

        private static void AddTransientListReadWriteService<TDataModel, TId, TService>(this IServiceCollection services)
            where TDataModel : class, IEntity
            where TService : class, IEntityWriteService<TDataModel, TId>
        {
            services.AddTransient<IEntityListService<TDataModel>, TService>();
            services.AddTransient<IEntityReadService<TDataModel, TId>, TService>();
            services.AddTransient<IEntityWriteService<TDataModel, TId>, TService>();
            services.AddTransient<TService, TService>();
        }

        private static void AddTransientListReadService<TDataModel, TId, TService>(this IServiceCollection services)
            where TDataModel : class, IEntity
            where TService : class, IEntityReadService<TDataModel, TId>
        {
            services.AddTransient<IEntityListService<TDataModel>, TService>();
            services.AddTransient<IEntityReadService<TDataModel, TId>, TService>();
            services.AddTransient<TService, TService>();
        }

        private static void AddTransientListService<TDataModel, TService>(this IServiceCollection services)
            where TDataModel : class, IEntity
            where TService : class, IEntityListService<TDataModel>
        {
            services.AddTransient<IEntityListService<TDataModel>, TService>();
            services.AddTransient<TService, TService>();
        }

        public static void AddEntityServices(this IServiceCollection services)
        {
            services.AddTransientListReadWriteService<ApplicationUser, long, ApplicationUserService>();
            services.AddTransientListReadWriteService<ApplicationRole, long, ApplicationRoleService>();
            services.AddTransientListReadWriteService<ContentBlock, long, ContentBlockService>();
            services.AddTransient<ContentBlockService, ContentBlockService>();

            services.AddTransientListReadWriteService<Client, long, ClientService>();
            services.AddTransientListReadWriteService<Project, long, ProjectService>();
            services.AddTransientListReadService<ClientStats, long, ClientStatsService>();

            services.AddTransientListReadWriteService<Job, long, JobService>();
            services.AddTransientListReadWriteService<JobItem, long, JobItemService>();
        }

        public static void AddEntityValidators(this IServiceCollection services)
        {
            services.AddSingleton<IValidator<ApplicationUser>, ApplicationUserValidator>();
            services.AddSingleton<IValidator<ApplicationRole>, ApplicationRoleValidator>();
            services.AddSingleton<IValidator<ContentBlock>, ContentBlockValidator>();

            services.AddSingleton<IValidator<Client>, ClientValidator>();
            services.AddSingleton<IValidator<Project>, ProjectValidator>();

            services.AddSingleton<IValidator<Job>, JobValidator>();
            services.AddSingleton<IValidator<JobItem>, JobItemValidator>();
        }

        public static void AddWorkItemServices(this IServiceCollection services)
        {
            services.AddSingleton<IWorkItemService<EchoWorkItem>, WorkItemService<EchoWorkItem>>();

            services.AddTransient<ISecureWorkItemService<EchoWorkItem>, SecureWorkItemService<EchoWorkItem>>();
        }
    }
}
