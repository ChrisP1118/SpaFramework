using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.DTO.Accounts
{
    public class ApplicationRoleDTO : IDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
