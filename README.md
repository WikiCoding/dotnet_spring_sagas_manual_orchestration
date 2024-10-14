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

## Docker
1. Postgresql
```bash
docker run -d -p 5432:5432 --name psql-db -e POSTGRES_USERNAME=postgres -e POSTGRES_PASSWORD=postgres postgres
# if any database needs creation
docker exec -it psql-db bash
psql -U postgres
CREATE DATABASE "cmd-inventory";
```

2. Kafka
```bash 
docker run -d -p 9092:9092 --name broker apache/kafka
```

3. Creating the Kafka Topics
```bash
docker exec -it broker bash
cd opt/kafka/bin
./kafka-topics.sh --bootstrap-server localhost:9092 --create --topic order-created-topic
./kafka-topics.sh --bootstrap-server localhost:9092 --create --topic inventory-deducted-topic
./kafka-topics.sh --bootstrap-server localhost:9092 --create --topic inventory-deduction-rejected-topic
```