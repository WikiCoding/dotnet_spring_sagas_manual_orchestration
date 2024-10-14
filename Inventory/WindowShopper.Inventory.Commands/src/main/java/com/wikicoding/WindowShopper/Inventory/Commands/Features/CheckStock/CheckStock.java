package com.wikicoding.WindowShopper.Inventory.Commands.Features.CheckStock;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.wikicoding.WindowShopper.Inventory.Commands.Features.Commands.CheckStockCommand;
import com.wikicoding.WindowShopper.Inventory.Commands.Features.CreateStock.CreateStockCommandHandler;
import com.wikicoding.WindowShopper.Inventory.Commands.Features.Events.OrderCancelledEvent;
import com.wikicoding.WindowShopper.Inventory.Commands.Features.Events.OrderCompletedEvent;
import com.wikicoding.WindowShopper.Inventory.Commands.Infrastructure.KafkaProducer;
import com.wikicoding.WindowShopper.Inventory.Commands.Model.Inventory;
import com.wikicoding.WindowShopper.Inventory.Commands.Model.InventoryEvent;
import com.wikicoding.WindowShopper.Inventory.Commands.Repository.InventoryEventRepository;
import com.wikicoding.WindowShopper.Inventory.Commands.Repository.InventoryRepository;
import lombok.AllArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.kafka.annotation.KafkaListener;
import org.springframework.stereotype.Component;

import java.time.LocalDateTime;
import java.util.Optional;

@Component
@AllArgsConstructor
@Slf4j
public class CheckStock {
    private final String topic = "order-created-topic";
    private final String successTopic = "inventory-deducted-topic";
    private final String failureTopic = "inventory-deduction-rejected-topic";
    private final InventoryRepository inventoryRepository;
    private final InventoryEventRepository inventoryEventRepository;
    private final KafkaProducer kafkaProducer;
    private final Logger logger = LoggerFactory.getLogger(CheckStock.class);

    @KafkaListener(topics = topic)
    public void listen(String message) {
        ObjectMapper mapper = new ObjectMapper();

        try {
            CheckStockCommand checkStockCommand = mapper.readValue(message, CheckStockCommand.class);

            logger.info("Received and mapped message to CheckStockCommand {}", message);

            Optional<Inventory> inventory = inventoryRepository.findById(checkStockCommand.getProductId());

            if (inventory.isEmpty() || inventory.get().getCurrentQty() <= checkStockCommand.getOrderQty()) {
                produceOrderCancelledEvent(checkStockCommand, mapper);
                return;
            }

            logger.info("Found product with productId {} with qty {}", checkStockCommand.getProductId(),
                    inventory.get().getCurrentQty());
            Inventory inv = inventory.get();

            produceOrderCompletedEvent(inv, checkStockCommand, mapper);
        } catch (Exception e) {
            logger.error("Error in the consumer. Message: {}", e.getMessage());
            throw new RuntimeException(e);
        }
    }

    private void produceOrderCompletedEvent(Inventory inv, CheckStockCommand checkStockCommand, ObjectMapper mapper) throws JsonProcessingException {
        logger.info("Product has enough stock, updating the current stock after the order");
        inv.setCurrentQty(inv.getCurrentQty() - checkStockCommand.getOrderQty());

        inventoryRepository.save(inv);

        // publish to inventory-deducted-topic
        OrderCompletedEvent orderCompletedEvent = new OrderCompletedEvent(checkStockCommand.getOrderId());

        // save the event in the db
        logger.info("Saving OrderCompletedEvent to the db");
        inventoryEventRepository.save(new InventoryEvent(0, checkStockCommand.getOrderId(),
                orderCompletedEvent.getClass().getName(), LocalDateTime.now()));

        String jsonString = mapper.writeValueAsString(orderCompletedEvent);

        logger.info("Producing success response for OrderId {} to topic {}", checkStockCommand.getOrderId(), successTopic);
        kafkaProducer.sendMessage(successTopic, jsonString);
    }

    private void produceOrderCancelledEvent(CheckStockCommand checkStockCommand, ObjectMapper mapper) throws JsonProcessingException {
        logger.info("Didn't find product/not enough stock for productId {}", checkStockCommand.getProductId());
        // publish to inventory-deduction-rejected-topic
        OrderCancelledEvent orderCancelledEvent = new OrderCancelledEvent(checkStockCommand.getOrderId());

        // save the event in the db
        logger.info("Saving OrderCancelledEvent to the db");
        inventoryEventRepository.save(new InventoryEvent(0, checkStockCommand.getOrderId(),
                orderCancelledEvent.getClass().getName(), LocalDateTime.now()));

        String jsonString = mapper.writeValueAsString(orderCancelledEvent);

        logger.info("Producing failure response for OrderId {} to topic {}", checkStockCommand.getOrderId(), failureTopic);
        kafkaProducer.sendMessage(failureTopic, jsonString);
    }
}
