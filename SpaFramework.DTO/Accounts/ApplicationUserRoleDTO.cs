using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.DTO.Accounts
{
    public class ApplicationUserRoleDTO : IDTO
    {
        public long Id { get; set; }

        public ApplicationUserDTO ApplicationUser { get; set; }
        public long ApplicationUserId { get; set; }

        public ApplicationRoleDTO ApplicationRole { get; set; }
        public long ApplicationRoleId { get; set; }
    }
}
