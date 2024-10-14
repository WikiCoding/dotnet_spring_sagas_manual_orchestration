package com.wikicoding.WindowShopper.Inventory.Queries.Abstractions;

public interface IRequestHandler<C, R> {
    R Handle(C command);
}
