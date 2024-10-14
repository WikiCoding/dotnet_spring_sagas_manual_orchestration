using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WindowShopper.Orders.Commands.Features.Commands;
using WindowShopper.Orders.Commands.Repository;

namespace WindowShopper.Orders.Commands.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly OrdersDbContext _context;

    public OrdersController(IMediator mediator, OrdersDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand createOrderCommand)
    {
        var orderCreated = await _mediator.Send(createOrderCommand);
        return CreatedAtAction(nameof(CreateOrder), orderCreated);
    }

    // For debugging Purposes
    [HttpGet]
    public async Task<IActionResult> GetOrderEvents()
    {
        var orderEvents = await _context.OrderEvents.ToListAsync();

        return Ok(orderEvents);
    }
}