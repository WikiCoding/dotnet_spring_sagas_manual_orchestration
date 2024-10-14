using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WindowShopper.Orders.Commands.Infrastructure.Messaging.Abstractions
{
    public interface IConsumerMessageBus<T>
    {
        T Consume(CancellationToken cancellationToken);
    }
}