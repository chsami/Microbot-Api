using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Stripe.Identity;

namespace MicrobotApi.Controllers;

[Route("create-checkout-session")]
[ApiController]
public class CheckoutApiController : Controller
{
    private readonly ILogger<CheckoutApiController> _logger;
    private readonly IConfiguration _configuration;

    public CheckoutApiController(ILogger<CheckoutApiController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }
    
    [HttpPost]
    [Authorize]
    public ActionResult Create([FromBody] CreateCheckOutRequest createCheckOutRequest)
    {
        var domain = _configuration["Discord:RedirectUri"];
        var options = new SessionCreateOptions
        {
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    // Provide the exact Price ID (for example, pr_1234) of the product you want to sell
                    Price = _configuration["Stripe:PriceSecret"],
                    Quantity = 1,
                },
            },
            Mode = "payment",
            SuccessUrl = domain + "/success",
            CancelUrl = domain + "/cancel",
            AutomaticTax = new SessionAutomaticTaxOptions { Enabled = true },
            PaymentIntentData = new SessionPaymentIntentDataOptions()
            {
                Metadata = new Dictionary<string, string>
                {
                    { "userId", createCheckOutRequest.UserId } // Add user ID as metadata
                },
            }
        };
        var service = new SessionService();
        Session session = service.Create(options);

        Response.Headers.Append("Location", session.Url);

        return Ok(session);
    }
}

public class CreateCheckOutRequest
{
    public string UserId { get; set; }
}

