using AIHUB_Server.Application.Common.Interfaces;

namespace AIHUB_Server.Common.Helpers
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // If we use e.g. Auth0 in production, we can use:
        // https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-6.0
        // and skip this middleware. But it is also not bad, it works correctly.
        public static bool InvokeInSignalR(HttpContext context, IUserService userService)
        {
            bool response = false; // At default make it false. If everythings is right, userService will return true

            var token = context.Request.Query["access_token"];

            var path = context.Request.Path;
            if (!string.IsNullOrEmpty(token) &&
                (path.StartsWithSegments("/streamcmd")))
            {
                // Moved to userService, otherwise this method wouldn't be static.
                response = userService.AttachUserToContext(context, userService, token); 
            }

            return response;
        }

        public async Task Invoke(HttpContext context, IUserService userService)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                userService.AttachUserToContext(context, userService, token);

            await _next(context);
        }
    }
}
