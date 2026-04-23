using BikeRentalApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BikeRentalApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WhatsAppController(WhatsAppService whatsApp, IConfiguration config) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> Send()
    {
        var to = config["WhatsApp:To"]!;
        var result = await whatsApp.SendMessageAsync(to, "Oi! Tudo bem?");
        return Ok(result);
    }
}
