package com.wikicoding.WindowShopper.Inventory.Commands.Features.Commands;

import lombok.AllArgsConstructor;
import lombok.Getter;
import lombok.NoArgsConstructor;

@Getter
@NoArgsConstructor
@AllArgsConstructor
public class CheckStockCommand {
    private String orderId;
    private int productId;
    private int orderQty;
    private String timestamp;
}
