package com.wikicoding.WindowShopper.Inventory.Commands.Features.UpdateStock;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.wikicoding.WindowShopper.Inventory.Commands.Abstractions.IRequestHandler;
import com.wikicoding.WindowShopper.Inventory.Commands.Features.Commands.UpdateStockCommand;
import com.wikicoding.WindowShopper.Inventory.Commands.Features.CreateStock.CreateStockCommandHandler;
import com.wikicoding.WindowShopper.Inventory.Commands.Features.Events.StockUpdatedEvent;
import com.wikicoding.WindowShopper.Inventory.Commands.Infrastructure.KafkaProducer;
import com.wikicoding.WindowShopper.Inventory.Commands.Model.Inventory;
import com.wikicoding.WindowShopper.Inventory.Commands.Model.InventoryEvent;
import com.wikicoding.WindowShopper.Inventory.Commands.Repository.InventoryEventRepository;
import com.wikicoding.WindowShopper.Inventory.Commands.Repository.InventoryRepository;
import lombok.AllArgsConstructor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;
import java.util.Optional;

@Service
@AllArgsConstructor
public class UpdateStockCommandHandler implements IRequestHandler<UpdateStockCommand, StockUpdatedEvent> {
    private final InventoryRepository inventoryRepository;
    private final InventoryEventRepository inventoryEventRepository;
    private final Logger logger = LoggerFactory.getLogger(UpdateStockCommandHandler.class);
    private final KafkaProducer kafkaProducer;

    @Override
    public StockUpdatedEvent Handle(UpdateStockCommand command) {
        logger.info("Updating stock.");
        Optional<Inventory> inv = inventoryRepository.findById(command.getProductId());

        if (inv.isEmpty()) {
            logger.error("Product not found.");
            throw new NullPointerException("Not found");
        }

        Inventory inventory = inv.get();
        inventory.setCurrentQty(command.getProductQty());
        inventoryRepository.save(inventory);

        StockUpdatedEvent stockUpdatedEvent = new StockUpdatedEvent(inventory.getProductId(), inventory.getCurrentQty());
        inventoryEventRepository.save(new InventoryEvent(0, "",
                stockUpdatedEvent.getClass().getName(), LocalDateTime.now()));

        ObjectMapper objectMapper = new ObjectMapper();
        try {
            String message = objectMapper.writeValueAsString(stockUpdatedEvent);

            String updatedTopic = "stock-updated-topic";
            kafkaProducer.sendMessage(updatedTopic, message);
        } catch (JsonProcessingException e) {
            throw new RuntimeException(e);
        }

        return stockUpdatedEvent;
    }
}
