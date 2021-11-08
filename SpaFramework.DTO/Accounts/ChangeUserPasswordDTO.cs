using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.DTO.Accounts
{
    public class ChangeUserPasswordDTO
    {
        public long ApplicationUserId { get; set; }
        public string NewPassword { get; set; }
    }
}
