using Microsoft.AspNetCore.Mvc;

namespace BikeRentalApi.Controllers;

[ApiController]
[Route("[controller]")]
public class BikeRentalController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] RentalRequest request)
    {
        return Ok();
    }
}

public record RentalRequest(string Message);
