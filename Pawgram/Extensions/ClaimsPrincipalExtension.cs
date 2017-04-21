using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Pawgram.Extensions
{
    /**
     * Based on http://stackoverflow.com/a/38284988
     * 
     * Replaces an alternative approach which calls the database by using
     *      @UserManager.GetUserAsync(User).Result.DisplayName
     * when retriveing nickname in a razore view, i.e _Loginpartial.cshtml
    **/
    public static class ClaimsPrincipalExtension
    {
        public static string GetDisplayName(this ClaimsPrincipal principal)
        {
            var displayName = principal.Claims.FirstOrDefault(c => c.Type == "DisplayName");
            return displayName?.Value;
        }
    }
}
