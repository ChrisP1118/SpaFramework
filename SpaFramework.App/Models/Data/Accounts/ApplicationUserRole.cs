using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Models.Data.Accounts
{
    public class ApplicationUserRole : IdentityUserRole<long>, IEntity, IHasId<long>
    {
        public long GetId() => this.Id;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
        public ApplicationRole ApplicationRole { get; set; }

        [NotMapped]
        public long ApplicationUserId
        {
            get { return UserId; }
            set { UserId = value; }
        }

        [NotMapped]
        public long ApplicationRoleId
        {
            get { return RoleId; }
            set { RoleId = value; }
        }
    }
}
