using System.Collections.Generic;

namespace SpaFramework.DTO.Accounts
{
    public class ApplicationUserDTO : IDTO
    {
        public long Id { get; set; }

        public string ConcurrencyCheck { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string DisplayName { get; set; }

        public List<ApplicationUserRoleDTO> Roles { get; set; }

        public bool HasPassword { get; set; }
    }
}
