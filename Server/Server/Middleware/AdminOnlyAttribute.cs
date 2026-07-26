using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Server.Middleware
{
    /// <summary>
    /// Authorization filter that only lets admins through. The client sends the
    /// logged-in user's id in the "X-User-Id" header; the filter looks that user
    /// up in the database and rejects the request unless the user exists, is not
    /// blocked, and has IsAdmin = true.
    ///
    /// NOTE: this is a stopgap. The id in the header is not cryptographically
    /// proven, so a caller who knows a real admin's id could still impersonate
    /// them. A proper fix is a signed token (JWT) issued at login.
    /// </summary>
    public class AdminOnlyAttribute : ActionFilterAttribute
    {
        public const string UserIdHeader = "X-User-Id";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var headers = context.HttpContext.Request.Headers;

            if (!headers.TryGetValue(UserIdHeader, out var rawUserId) ||
                !int.TryParse(rawUserId, out int userId))
            {
                context.Result = new UnauthorizedObjectResult("Missing or invalid user identity.");
                return;
            }

            try
            {
                var user = Server.BL.User.GetUserById(userId);

                if (user == null || user.IsBlocked || !user.IsAdmin)
                {
                    context.Result = new ObjectResult("Admin privileges are required.")
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                    return;
                }
            }
            catch (Exception ex)
            {
                Server.Logging.AppLogger.LogException(ex);
                context.Result = new ObjectResult("An error occurred while authorizing the request.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
