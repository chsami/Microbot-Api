using MicrobotApi.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Session = Stripe.Checkout.Session;

namespace MicrobotApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebHookController(ILogger<CheckoutApiController> logger, MicrobotContext microbotContext, IConfiguration configuration)
    : Controller
{
    [HttpPost]
    public async Task<IActionResult> Webhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var endpointSecret = configuration["Stripe:PriceSecret"]; // Set this to your Stripe webhook secret

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                endpointSecret
            );

            // Handle the event
            if (stripeEvent.Type == Events.ChargeSucceeded)
            {
                var session = stripeEvent.Data.Object as Session;
                // Handle the successful checkout session
                await HandleCheckoutSession(session);
            }
            else
            {
                // Handle other event types
                logger.LogInformation($"Unhandled event type: {stripeEvent.Type}");
            }

            return Ok();
        }
        catch (StripeException e)
        {
            logger.LogError(e, "Stripe webhook error");
            return BadRequest();
        }
    }
    
    private async Task HandleCheckoutSession(Session? session)
    {
        // Implement your business logic for handling a successful checkout session
        logger.LogInformation($"Payment for session {session?.Id} was successful.");
        // You can use session.PaymentIntentId to retrieve more details about the payment if needed
        var userId = session?.Metadata["userId"];

        if (userId == null)
        {
            logger.LogWarning("User id is null in checkout");
            return;
        }

        var user = await microbotContext.DiscordUsers.FirstOrDefaultAsync(x => x.DiscordId == userId);

        if (user == null)
        {
            logger.LogWarning("User not found");
            return;
        }
        
        var key = new ScriptKey()
        {
            Key = Guid.NewGuid().ToString(),
            Active = true
        };
        

        microbotContext.Keys.Add(key);
        
        user.Keys.Add(key);

        await microbotContext.SaveChangesAsync();
    }
}