using BikeRentalApi.Models;
using BikeRentalApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BikeRentalApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WebhookController(
    IConfiguration config,
    ConversationService conversation,
    ILogger<WebhookController> logger) : ControllerBase
{
    [HttpGet]
    public IActionResult Verify(
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.verify_token")] string token,
        [FromQuery(Name = "hub.challenge")] string challenge)
    {
        var verifyToken = config["WhatsApp:VerifyToken"];

        if (mode == "subscribe" && token == verifyToken)
            return Content(challenge, "text/plain");

        return Forbid();
    }

    [HttpPost]
    public async Task<IActionResult> Receive([FromBody] WebhookPayload payload)
    {
        foreach (var entry in payload.Entry)
        foreach (var change in entry.Changes)
        {
            var messages = change.Value.Messages;
            if (messages is null) continue;

            foreach (var message in messages)
            {
                if (message.Type != "text") continue;

                var from = message.From;
                var text = message.Text?.Body ?? string.Empty;

                logger.LogInformation("De: {From} | Texto: {Text}", from, text);
                await conversation.HandleAsync(from, text);
            }
        }

        return Ok();
    }
}
