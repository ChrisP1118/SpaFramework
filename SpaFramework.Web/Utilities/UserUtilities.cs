using SpaFramework.App.Models.Data.Accounts;
using SpaFramework.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using SpaFramework.App.Models;
using SpaFramework.Core.Models;

namespace SpaFramework.Web.Utilities
{
    public static class UserUtilities
    {
        public static async Task<List<Claim>> GenerateClaims(IConfiguration configuration, UserManager<ApplicationUser> userManager, ApplicationUser applicationUser)
        {
            return await App.Utilities.UserUtilities.GenerateClaims(configuration, userManager, applicationUser);
        }

        public static async Task<string> GenerateJwtToken(IConfiguration configuration, UserManager<ApplicationUser> userManager, ApplicationUser applicationUser)
        {
            var claims = await GenerateClaims(configuration, userManager, applicationUser);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(configuration["JwtExpireDays"]));

            var token = new JwtSecurityToken(
                configuration["JwtIssuer"],
                configuration["JwtIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
