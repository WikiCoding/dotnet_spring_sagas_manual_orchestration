package com.wikicoding.WindowShopper.Inventory.Commands.Features.Events;

import lombok.AllArgsConstructor;
import lombok.Getter;

@Getter
@AllArgsConstructor
public class StockUpdatedEvent {
    private final int productId;
    private final int productQty;
}
