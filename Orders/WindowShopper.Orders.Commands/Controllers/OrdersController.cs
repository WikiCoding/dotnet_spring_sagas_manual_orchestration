using MediatR;
using Microsoft.AspNetCore.Mvc;
using WindowShopper.Orders.Commands.Features.Commands;

namespace WindowShopper.Orders.Commands.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand createOrderCommand)
    {
        var orderCreated = await _mediator.Send(createOrderCommand);
        return CreatedAtAction(nameof(CreateOrder), orderCreated);
    }
}