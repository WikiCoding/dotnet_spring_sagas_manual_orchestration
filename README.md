## Saga Flow

1. POST CmdOrderMs ->
   OrderCreatedEvent (event saved in service db table source of truth Orders and Outbox Table OrdersOutbox) ->
   [SagaStart] - starts SagaObject with the command Id and Saga Correlation Id. Order Status = Waiting Confirmation ->
   OrderMs Publish [topic=order-created-topic] ->
   InventoryMs Consumer [topic=order-created-topic] ->
   Checks Current Stock Qty ->
   If enough quantity InventoryDeductedEvent / If not enough quantity InventoryDeductionRejectedEvent ->
   CmdInventoryMs Publish [topic=inventory-deducted-topic] / InventoryMs Publish [topic=inventory-deduction-rejected-topic] -> QueryInventoryMs consumes these
   OrderMs Consumer [topic=inventory-deducted-topic] + [topic=inventory-deduction-rejected-topic] ->
   If deducted OrderCompletedEvent / if not deducted OrderCanceledEvent ->
   Publish OrderCompletedEvent [topic=order-completed-topic] / Publish OrderCanceledEvent [topic=order-cancelled-topic] -> QueryOrderMs consumes these
