using Application.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public string? UserId =>
        _accessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}
