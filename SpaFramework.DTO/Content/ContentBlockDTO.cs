using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpaFramework.DTO.Content
{
    public class ContentBlockDTO : IDTO
    {
        public long Id { get; set; }

        public string ConcurrencyCheck { get; set; }

        public string Slug { get; set; }

        public bool IsPage { get; set; }

        public string Description { get; set; }

        public string Title { get; set; }

        public string Value { get; set; }

        public List<AllowedTokenDTO> AllowedTokens { get; set; }
    }
}
