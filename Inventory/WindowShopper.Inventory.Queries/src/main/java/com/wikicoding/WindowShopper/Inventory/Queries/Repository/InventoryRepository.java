package com.wikicoding.WindowShopper.Inventory.Queries.Repository;

import com.wikicoding.WindowShopper.Inventory.Queries.Model.Inventory;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

@Repository
public interface InventoryRepository extends JpaRepository<Inventory, Integer> {
}
