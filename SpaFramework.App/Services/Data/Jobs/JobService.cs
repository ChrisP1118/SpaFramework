using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpaFramework.App.DAL;
using SpaFramework.App.Models.Data.Accounts;
using SpaFramework.App.Models.Data.Jobs;
using SpaFramework.Core.Models;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Services.Data.Jobs
{
    public class JobService : EntityWriteService<Job, long>
    {
        private readonly IClock _clock;

        public JobService(ApplicationDbContext dbContext, IConfiguration configuration, UserManager<ApplicationUser> userManager, IValidator<Job> validator, ILogger<EntityWriteService<Job, long>> logger, IClock clock) : base(dbContext, configuration, userManager, validator, logger)
        {
            _clock = clock;
        }

        protected override async Task<IQueryable<Job>> ApplyIdFilter(IQueryable<Job> queryable, long id)
        {
            return queryable.Where(x => x.Id == id);
        }

        protected override async Task<IQueryable<Job>> ApplyReadSecurity(ApplicationUser applicationUser, IQueryable<Job> queryable)
        {
            // Site admins can read all users
            if (await _userManager.IsInRoleAsync(applicationUser, ApplicationRoleNames.SuperAdmin))
                return queryable;

            return queryable.Where(x => false);
        }

        protected override async Task<bool> CanWrite(ApplicationUser applicationUser, Job dataModel, Dictionary<string, object> extraData)
        {
            if (await _userManager.IsInRoleAsync(applicationUser, ApplicationRoleNames.SuperAdmin))
                return true;

            return false;
        }

        private async Task<bool> Increment(long jobId, string columnName)
        {
            bool isDone = false;

            var connectionString = _configuration.GetConnectionString("Default");
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                await sqlConnection.OpenAsync();

                var sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.CommandText = $"UPDATE Jobs SET {columnName} = {columnName} + 1, Updated = @Timestamp OUTPUT INSERTED.ExpectedCount, INSERTED.SuccessCount, INSERTED.FailureCount WHERE Id = @JobId";
                sqlCommand.Parameters.Add(new SqlParameter("@JobId", jobId));
                sqlCommand.Parameters.Add(new SqlParameter("@Timestamp", _clock.GetCurrentInstant().ToDateTimeUtc()));

                var sqlDataReader = await sqlCommand.ExecuteReaderAsync();
                if (await sqlDataReader.ReadAsync())
                {
                    long expectedCount = (long)sqlDataReader["ExpectedCount"];
                    long successCount = (long)sqlDataReader["SuccessCount"];
                    long failureCount = (long)sqlDataReader["FailureCount"];

                    isDone = expectedCount == successCount + failureCount;
                }

                if (isDone)
                {
                    sqlCommand = sqlConnection.CreateCommand();
                    sqlCommand.CommandText = "UPDATE Jobs SET Updated = @Timestamp, Ended = @Timestamp WHERE Id = @JobId";
                    sqlCommand.Parameters.Add(new SqlParameter("@JobId", jobId));
                    sqlCommand.Parameters.Add(new SqlParameter("@Timestamp", _clock.GetCurrentInstant().ToDateTimeUtc()));

                    await sqlCommand.ExecuteNonQueryAsync();
                }

                await sqlConnection.CloseAsync();
            }

            return isDone;
        }

        public async Task<bool> IncrementSuccessCount(long jobId)
        {
            return await Increment(jobId, "SuccessCount");
        }

        public async Task<bool> IncrementFailureCount(long jobId)
        {
            return await Increment(jobId, "FailureCount");
        }
    }
}
