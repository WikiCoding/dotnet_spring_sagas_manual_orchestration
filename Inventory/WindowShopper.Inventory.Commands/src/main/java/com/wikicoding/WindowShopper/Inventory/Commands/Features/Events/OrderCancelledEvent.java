package com.wikicoding.WindowShopper.Inventory.Commands.Features.Events;

import lombok.AllArgsConstructor;
import lombok.Getter;

@Getter
@AllArgsConstructor
public class OrderCancelledEvent {
    private final String orderId;
}
