namespace FamilyPulse.Api.Filters;

public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Extracts the command object (e.g., RegisterAnonymousFamilyCommand) directly from route arguments
        var command = context.Arguments.OfType<T>().FirstOrDefault();

        if (command is null)
        {
            return Results.BadRequest(new { Error = "Invalid or empty request payload." });
        }

        // Custom validation check example
        if (command is FamilyPulse.Application.Identity.Commands.RegisterAnonymousFamilyCommand registerCmd)
        {
            if (string.IsNullOrWhiteSpace(registerCmd.VirtualLandmarkId))
            {
                return Results.BadRequest(new { Error = "VirtualLandmarkId is required." });
            }
        }

        return await next(context);
    }
}