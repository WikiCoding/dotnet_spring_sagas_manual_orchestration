package com.wikicoding.WindowShopper.Inventory.Commands.Features.Commands;

import lombok.AllArgsConstructor;
import lombok.Getter;
import lombok.NoArgsConstructor;

@Getter
@NoArgsConstructor
@AllArgsConstructor
public class CreateStockCommand {
    private int productId;
    private int qty;
}
