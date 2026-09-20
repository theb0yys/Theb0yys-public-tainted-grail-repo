# Recipes and Economy

Crafting and economy meet at existing item identity, ingredient ownership, prices and merchant/station services.

## Current rule

- let inventory own item quantities;
- let crafting own ingredient consumption and product creation;
- let merchant/economy systems own buy/sell pricing;
- do not use crafting UI state as the authority for inventory or price state.

When a mod changes prices, keep that change on the native vendor-price surface. When a mod changes crafting, keep the transaction on the native crafting/inventory path.
