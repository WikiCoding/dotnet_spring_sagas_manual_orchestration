using MediatR;
using Microsoft.EntityFrameworkCore;
using WindowShopper.Orders.Queries.Features.Queries;
using WindowShopper.Orders.Queries.Repository;

namespace WindowShopper.Orders.Queries.Features.GetAllOrders;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, IEnumerable<OrderDataModel>>
{
    private readonly OrdersDbContext _dbContext;

    public GetAllOrdersQueryHandler(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<OrderDataModel>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Orders.ToListAsync(cancellationToken);
    }
}