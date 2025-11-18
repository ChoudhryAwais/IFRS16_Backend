using IFRS16_Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace IFRS16_Backend.Middleware
{
    public class SingleSessionMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
    {
        private readonly RequestDelegate _next = next;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip middleware for endpoints explicitly allowed anonymously
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null)
            {
                await _next(context);
                return;
            }

            // Also skip for LoginController specifically
            var cad = endpoint?.Metadata?.GetMetadata<ControllerActionDescriptor>();
            if (cad != null && (string.Equals(cad.ControllerName, "Login", StringComparison.OrdinalIgnoreCase) || string.Equals(cad.ControllerName, "SessionToken", StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader["Bearer ".Length..].Trim();
                var handler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken;

                try
                {
                    jwtToken = handler.ReadJwtToken(token);
                }
                catch
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid token format.");
                    return;
                }

                var userIdClaim = jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.NameIdentifier
                    || c.Type == JwtRegisteredClaimNames.Sub
                    || string.Equals(c.Type, "nameid", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(c.Type, "sub", StringComparison.OrdinalIgnoreCase));

                if (userIdClaim != null)
                {
                    var userId = int.Parse(userIdClaim.Value);
                    var user = await db.SessionToken.FirstOrDefaultAsync(u => u.UserId == userId);

                    if (user == null || user.Token != token)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Session invalid or logged out from another device.");
                        return;
                    }
                }
                else
                {
                    // Token lacks NameIdentifier claim. Try to find a user that has this token stored.
                    var userByToken = await db.SessionToken.FirstOrDefaultAsync(u => u.Token == token);

                    // If token not found in DB or token doesn't match any user's current token, reject as invalid session
                    if (userByToken == null)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Session invalid or logged out from another device.");
                        return;
                    }

                    // If we found a user by token but token contained no user id claim, treat it as invalid
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Session invalid or logged out from another device.");
                    return;
                }
            }

            await _next(context);
        }
    }
}