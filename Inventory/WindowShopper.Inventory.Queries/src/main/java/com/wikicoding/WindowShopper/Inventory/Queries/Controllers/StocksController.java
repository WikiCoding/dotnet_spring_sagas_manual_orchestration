package com.wikicoding.WindowShopper.Inventory.Queries.Controllers;

import com.wikicoding.WindowShopper.Inventory.Queries.Features.GetInventory.GetInventoryQueryHandler;
import com.wikicoding.WindowShopper.Inventory.Queries.Features.Queries.GetInventoryQuery;
import com.wikicoding.WindowShopper.Inventory.Queries.Model.Inventory;
import com.wikicoding.WindowShopper.Inventory.Queries.Repository.InventoryRepository;
import lombok.AllArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("/stocks")
@AllArgsConstructor
public class StocksController {
    private final GetInventoryQueryHandler queryHandler;

    @GetMapping
    public ResponseEntity<Iterable<Inventory>> GetInventory() {
        GetInventoryQuery getInventoryQuery = new GetInventoryQuery();
        return ResponseEntity.ok(queryHandler.Handle(getInventoryQuery));
    }
}
