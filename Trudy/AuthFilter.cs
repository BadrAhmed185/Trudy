namespace Trudy
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    public class AuthFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Check if the session contains the "IsAuthenticated" flag
            var isAuthenticated = context.HttpContext.Session.GetString("IsAuthenticated");

            // If the user is not authenticated, redirect to the login page
            if (string.IsNullOrEmpty(isAuthenticated) || isAuthenticated != "true")
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }

            base.OnActionExecuting(context);
        }
    }

}
