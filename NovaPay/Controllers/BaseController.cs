namespace NovaPay.Controllers
{
   using Microsoft.AspNetCore.Mvc;

namespace NovaPay.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected Guid CustomerId
        {
            get
            {
                var CustomerId =
                    User.FindFirst("customerId")?.Value;

                if (!Guid.TryParse(
                        CustomerId,
                        out var id))
                {
                    throw new UnauthorizedAccessException(
                        "Authenticated customer ID is missing or invalid.");
                }

                return id;
            }
        }

            protected string Actor
            {
                get
                {
                    // Prefer the authenticated user's subject/user ID
                    var actor =
                        User.FindFirst("sub")?.Value
                        ?? User.FindFirst("userId")?.Value
                        ?? User.FindFirst("name")?.Value
                        ?? User.Identity?.Name;

                    if (string.IsNullOrWhiteSpace(actor))
                    {
                        throw new UnauthorizedAccessException(
                            "Authenticated actor is missing.");
                    }

                    return actor;
                }
            }

            protected string TraceId
        {
            get
            {
                // First check if client supplied X-Trace-Id
                if (Request.Headers.TryGetValue(
                        "X-Trace-Id",
                        out var traceId) &&
                    !string.IsNullOrWhiteSpace(traceId))
                {
                    return traceId.ToString();
                }

                // Otherwise use ASP.NET Core's request trace ID
                return HttpContext.TraceIdentifier;
            }
        }
    }
}
}
