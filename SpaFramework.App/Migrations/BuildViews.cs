using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Migrations
{
    public static class InitialViews
    {
        public static void BuildInitialViews(this MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
if object_id('ClientStats','v') is not null
    drop view ClientStats;
");

            migrationBuilder.Sql(@"
create view ClientStats
as
	select
		c.Id as ClientId,
		count(*) as NumberOfProjects,
		min(p.StartDate) as FirstStartDate,
		max(p.EndDate) as LastEndDate
	from Clients c
		join Projects p on p.ClientId = c.Id
	group by c.Id
");
        }
    }
}
