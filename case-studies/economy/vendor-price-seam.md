# Vendor Price: A Narrow Validated Seam

Tainted Economy deliberately chose `TradeUtils.Price` as its first live mutation because it can change the final price while preserving the rest of the native transaction.

The private validation plan records buy/sell throwaway-save validation and disable/reload rollback for the core lane.

## Lesson

Start with one terminal scalar owned by native code, keep mutation default-off, preserve native blocked cases, and validate rollback before broadening into stock, loot or reward systems.
