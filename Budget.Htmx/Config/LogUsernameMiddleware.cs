using Serilog.Context;

namespace Budget.Htmx.Config;

public class LogUsernameMiddleware
{
    private readonly RequestDelegate _next;

    public LogUsernameMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task Invoke(HttpContext context)
    {
        LogContext.PushProperty("Username", context.User.Identity?.Name ?? "Anonymous");

        return _next(context);
    }
}