using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace StudentApi.Authorization
{
    // This authorization handler enforces the ownership rule for student resources.
    // It checks whether the current user is either:
    // - An Admin (full access), OR
    // - The owner of the student record being requested
    public class StudentOwnerOrAdminHandler : AuthorizationHandler<StudentOwnerOrAdminRequirement, int>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, StudentOwnerOrAdminRequirement requirement, int studentID)
        {
            // Admin override
            if(context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            string userID = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Ownership Check
            if(int.TryParse(userID, out int authenticatedUserID) && studentID == authenticatedUserID)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    }
}
