package com.wikicoding.WindowShopper.Inventory.Commands.Model;

import jakarta.persistence.*;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.time.LocalDateTime;

@Entity
@Table(name = "inventory_events")
@NoArgsConstructor
@AllArgsConstructor
@Data
public class InventoryEvent {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private int id = 0;
    private String orderId;
    private String eventType;
    private LocalDateTime createdAt = LocalDateTime.now();
}
