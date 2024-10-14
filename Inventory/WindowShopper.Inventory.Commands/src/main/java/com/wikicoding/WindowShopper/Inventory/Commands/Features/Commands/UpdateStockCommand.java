package com.wikicoding.WindowShopper.Inventory.Commands.Features.Commands;

import lombok.AllArgsConstructor;
import lombok.Getter;

@Getter
@AllArgsConstructor
public class UpdateStockCommand {
    private final int productId;
    private final int productQty;
}
