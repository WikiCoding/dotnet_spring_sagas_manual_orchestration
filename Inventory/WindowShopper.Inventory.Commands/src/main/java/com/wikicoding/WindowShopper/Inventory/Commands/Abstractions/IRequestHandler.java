package com.wikicoding.WindowShopper.Inventory.Commands.Abstractions;

public interface IRequestHandler<C, R> {
    R Handle(C command);
}
