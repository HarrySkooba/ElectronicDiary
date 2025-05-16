using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace Server.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class AuthorizeRoleAttribute : AuthorizeAttribute
    {
        public AuthorizeRoleAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }
    }
}
