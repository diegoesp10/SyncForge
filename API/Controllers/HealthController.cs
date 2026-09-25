using Application.Health;
using Contracts.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController(IHealthService service) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<HealthResponse>> Get(CancellationToken cancellationToken)
    {
        var response = await service.GetAsync(cancellationToken);
        return response.Status == "ok" ? Ok(response) : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }
}
