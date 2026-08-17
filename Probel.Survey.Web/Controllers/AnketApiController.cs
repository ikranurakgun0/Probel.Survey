using Microsoft.AspNetCore.Mvc;
using Probel.Survey.Application.Anketler;

namespace Probel.Survey.Web.Controllers;

[ApiController]
[Route("api/anket")]
public class AnketApiController : ControllerBase
{
    private readonly IAnketService _anketService;
    public AnketApiController(IAnketService anketService) => _anketService = anketService;

    [HttpGet("{id}/rapor")]
    public async Task<IActionResult> GetRapor(long id, CancellationToken ct)
    {
        try
        {
            var rapor = await _anketService.GetRaporAsync(id, ct);
            return Ok(rapor);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
