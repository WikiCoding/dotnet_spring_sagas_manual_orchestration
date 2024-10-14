package com.wikicoding.WindowShopper.Inventory.Commands.Controllers;

import com.wikicoding.WindowShopper.Inventory.Commands.Features.Commands.CreateStockCommand;
import com.wikicoding.WindowShopper.Inventory.Commands.Features.Commands.UpdateStockCommand;
import com.wikicoding.WindowShopper.Inventory.Commands.Features.CreateStock.CreateStockCommandHandler;
import com.wikicoding.WindowShopper.Inventory.Commands.Features.Events.StockCreatedEvent;
import com.wikicoding.WindowShopper.Inventory.Commands.Features.Events.StockUpdatedEvent;
import com.wikicoding.WindowShopper.Inventory.Commands.Features.UpdateStock.UpdateStockCommandHandler;
import lombok.AllArgsConstructor;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/stocks")
@AllArgsConstructor
public class StocksController {
    private final CreateStockCommandHandler createStockCommandHandler;
    private final UpdateStockCommandHandler updateStockCommandHandler;

    @PostMapping
    public ResponseEntity<StockCreatedEvent> CreateInventory(@RequestBody CreateStockCommand command) {
        if (command.getQty() <= 0 || command.getProductId() <= 0)
            return ResponseEntity.status(HttpStatus.BAD_REQUEST).body(null);

        StockCreatedEvent stockCreatedEvent = createStockCommandHandler.Handle(command);

        return ResponseEntity.status(HttpStatus.CREATED).body(stockCreatedEvent);
    }

    @PatchMapping("{id}")
    public ResponseEntity<StockUpdatedEvent> UpdateInventory(@PathVariable(name = "id") int id,
                                                             @RequestParam(name = "qty") int qty) {
        if (id <= 0 || qty <= 0)
            return ResponseEntity.status(HttpStatus.BAD_REQUEST).body(null);

        UpdateStockCommand updateStockCommand = new UpdateStockCommand(id, qty);

        try {
            StockUpdatedEvent stockUpdatedEvent = updateStockCommandHandler.Handle(updateStockCommand);

            return ResponseEntity.status(HttpStatus.OK).body(stockUpdatedEvent);
        } catch (NullPointerException e) {
            return ResponseEntity.status(HttpStatus.NOT_FOUND).body(null);
        }
    }
}
