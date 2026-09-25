using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.Contracts;
using Security.Identity;
using Security.Services;

namespace API.Controllers;

[ApiController]
[Authorize(Roles = AppRoles.SuperAdmin)]
[Route("api/users")]
public sealed class UsersController(ISecurityService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<UserResponse>> List(CancellationToken cancellationToken) =>
        service.ListUsersAsync(cancellationToken);

    [HttpGet("{id:guid}")]
    public Task<UserResponse> GetById(Guid id, CancellationToken cancellationToken) =>
        service.GetUserAsync(id, cancellationToken);

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(
        [FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await service.CreateUserAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPatch("{id:guid}/role")]
    public Task<UserResponse> ChangeRole(Guid id, [FromBody] ChangeUserRoleRequest request, CancellationToken cancellationToken) =>
        service.ChangeRoleAsync(id, request, cancellationToken);

    [HttpPatch("{id:guid}/status")]
    public Task<UserResponse> SetStatus(Guid id, [FromBody] SetUserStatusRequest request, CancellationToken cancellationToken) =>
        service.SetStatusAsync(id, request, cancellationToken);
}
