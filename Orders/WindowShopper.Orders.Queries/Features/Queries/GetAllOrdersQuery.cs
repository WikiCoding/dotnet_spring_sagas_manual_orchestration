using MediatR;
using WindowShopper.Orders.Queries.Repository;

namespace WindowShopper.Orders.Queries.Features.Queries;

public record GetAllOrdersQuery() : IRequest<IEnumerable<OrderDataModel>>;