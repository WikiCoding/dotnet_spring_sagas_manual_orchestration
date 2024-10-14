package com.wikicoding.WindowShopper.Inventory.Commands.Repository;

import com.wikicoding.WindowShopper.Inventory.Commands.Model.InventoryEvent;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

@Repository
public interface InventoryEventRepository extends JpaRepository<InventoryEvent, Integer> {
}
