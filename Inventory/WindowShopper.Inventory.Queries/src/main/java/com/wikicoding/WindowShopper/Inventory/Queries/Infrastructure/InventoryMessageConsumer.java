package com.wikicoding.WindowShopper.Inventory.Queries.Infrastructure;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.wikicoding.WindowShopper.Inventory.Queries.Model.Inventory;
import com.wikicoding.WindowShopper.Inventory.Queries.Repository.InventoryRepository;
import lombok.AllArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.kafka.annotation.KafkaListener;
import org.springframework.stereotype.Component;

import java.util.Optional;

@Component
@AllArgsConstructor
@Slf4j
public class InventoryMessageConsumer {
    private final String createdTopic = "stock-created-topic";
    private final String updatedTopic = "stock-updated-topic";
    private final InventoryRepository inventoryRepository;
    private final Logger logger = LoggerFactory.getLogger(InventoryMessageConsumer.class);

    @KafkaListener(topics = createdTopic)
    public void listenCreate(String message) {
        ObjectMapper objectMapper = new ObjectMapper();

        try {
            Inventory inventory = objectMapper.readValue(message, Inventory.class);

            logger.info("Saving inventory: {}", message);
            inventoryRepository.save(inventory);
        } catch (JsonProcessingException e) {
            logger.error("Error getting inventory: {}", e.getMessage());
            throw new RuntimeException(e);
        }
    }

    @KafkaListener(topics = updatedTopic)
    public void listenUpdate(String message) {
        ObjectMapper objectMapper = new ObjectMapper();

        try {
            Inventory inventory = objectMapper.readValue(message, Inventory.class);
            Optional<Inventory> product = inventoryRepository.findById(inventory.getProductId());

            if(product.isEmpty()) {
                logger.error("Product Id not found in inventory: {}", inventory.getProductId());
                return;
            }

            Inventory inv = product.get();
            inv.setCurrentQty(inventory.getCurrentQty());

            logger.info("Updating inventory: {}", message);
            inventoryRepository.save(inv);
        } catch (JsonProcessingException e) {
            logger.error("Error parsing inventory msg: {}", e.getMessage());
            throw new RuntimeException(e);
        }
    }
}
