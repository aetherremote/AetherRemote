using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AetherRemoteServer.Domain.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class LocalHostOnlyAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var ip = context.HttpContext.Connection.RemoteIpAddress;
        if (ip is null || IPAddress.IsLoopback(ip) is false)
            context.Result = new ForbidResult();
    }
}