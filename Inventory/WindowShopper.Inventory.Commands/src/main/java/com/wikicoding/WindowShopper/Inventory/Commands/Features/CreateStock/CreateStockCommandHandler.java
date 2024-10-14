package com.wikicoding.WindowShopper.Inventory.Commands.Features.CreateStock;

import com.wikicoding.WindowShopper.Inventory.Commands.Abstractions.IRequestHandler;
import com.wikicoding.WindowShopper.Inventory.Commands.Features.Commands.CreateStockCommand;
import com.wikicoding.WindowShopper.Inventory.Commands.Features.Events.StockCreatedEvent;
import com.wikicoding.WindowShopper.Inventory.Commands.Model.Inventory;
import com.wikicoding.WindowShopper.Inventory.Commands.Model.InventoryEvent;
import com.wikicoding.WindowShopper.Inventory.Commands.Repository.InventoryEventRepository;
import com.wikicoding.WindowShopper.Inventory.Commands.Repository.InventoryRepository;
import lombok.AllArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;

@Service
@Slf4j
@AllArgsConstructor
public class CreateStockCommandHandler implements IRequestHandler<CreateStockCommand, StockCreatedEvent> {
    private final InventoryRepository inventoryRepository;
    private final InventoryEventRepository inventoryEventRepository;
    private final Logger logger = LoggerFactory.getLogger(CreateStockCommandHandler.class);

    @Override
    public StockCreatedEvent Handle(CreateStockCommand command) {
        logger.info("Creating stock.");
        Inventory inventory = new Inventory(0, command.getQty());
        inventoryRepository.save(inventory);

        StockCreatedEvent stockCreatedEvent = new StockCreatedEvent(inventory.getProductId(), inventory.getCurrentQty());
        inventoryEventRepository.save(new InventoryEvent(0, "",
                stockCreatedEvent.getClass().getName(), LocalDateTime.now()));

        return stockCreatedEvent;
    }
}
