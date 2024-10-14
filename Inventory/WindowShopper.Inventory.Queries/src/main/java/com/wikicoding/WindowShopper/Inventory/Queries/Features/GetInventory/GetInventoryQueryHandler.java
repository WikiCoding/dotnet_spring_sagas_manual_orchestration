package com.wikicoding.WindowShopper.Inventory.Queries.Features.GetInventory;

import com.wikicoding.WindowShopper.Inventory.Queries.Abstractions.IRequestHandler;
import com.wikicoding.WindowShopper.Inventory.Queries.Features.Queries.GetInventoryQuery;
import com.wikicoding.WindowShopper.Inventory.Queries.Model.Inventory;
import com.wikicoding.WindowShopper.Inventory.Queries.Repository.InventoryRepository;
import lombok.AllArgsConstructor;
import org.springframework.stereotype.Service;

@Service
@AllArgsConstructor
public class GetInventoryQueryHandler implements IRequestHandler<GetInventoryQuery, Iterable<Inventory>> {
    private final InventoryRepository inventoryRepository;

    @Override
    public Iterable<Inventory> Handle(GetInventoryQuery command) {
        return inventoryRepository.findAll();
    }
}
