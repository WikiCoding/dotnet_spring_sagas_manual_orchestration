using MediatR;
using Microsoft.AspNetCore.Mvc;
using WindowShopper.Orders.Queries.Features.Queries;

namespace WindowShopper.Orders.Queries.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        var query = new GetAllOrdersQuery();
        var orders = await _mediator.Send(query);
        
        return Ok(orders);
    }
}