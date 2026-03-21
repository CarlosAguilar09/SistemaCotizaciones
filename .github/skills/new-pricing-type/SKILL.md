---
name: new-pricing-type
description: Guide for adding new pricing types to the quotation system. Use this when asked to add a new way to calculate prices for quote items, such as hourly rates, packages, or volume-based pricing.
---

# Adding a New Pricing Type

The system supports multiple pricing strategies for quote items via the `QuoteItem.PricingType` string discriminator. This guide covers how to add a new one.

## Existing Pricing Types

| PricingType | Description | Data Source |
|-------------|-------------|-------------|
| `"Fijo"` | Fixed-price catalog item | `ProductId`, `UnitPrice` |
| `"Material"` | Material option selection | `MaterialOptionId`, `UnitPrice` |
| `"Area"` | Area-based (width × height × price) | `CalculationData` (JSON) |
| `"Personalizado"` | Custom cost lines | `CalculationData` (JSON) |

## Steps to Add a New Pricing Type

### 1. Define the PricingType String

Choose a descriptive Spanish string (e.g., `"PorHora"`, `"Paquete"`). This is stored in `QuoteItem.PricingType`. No enum modification needed.

### 2. Create a Data Shape (if using CalculationData)

In `Helpers/CalculationDetailHelper.cs`, add a new data class:

```csharp
public class MyCalcData
{
    public decimal Rate { get; set; }
    public int Hours { get; set; }
    // ... fields specific to this pricing strategy
}
```

Add serialization and parsing methods following the existing pattern:

```csharp
public static string ToJson(MyCalcData data) =>
    JsonSerializer.Serialize(data);

public static MyCalcData? ParseMyData(string? json)
{
    if (string.IsNullOrWhiteSpace(json)) return null;
    try { return JsonSerializer.Deserialize<MyCalcData>(json); }
    catch { return null; }
}
```

### 3. Update GetReadableSummary

In `CalculationDetailHelper.GetReadableSummary()`, add a case for the new type:

```csharp
case "PorHora":
    var myData = ParseMyData(calculationData);
    if (myData == null) return "Sin detalle";
    return $"Tarifa: {myData.Rate:C2}/hr × {myData.Hours} hrs";
```

### 4. Add Calculation Logic

In `Services/QuoteCalculationService.cs`, add a method if the pricing type needs computed subtotals:

```csharp
public decimal CalculateMySubtotal(MyCalcData data)
{
    return data.Rate * data.Hours;
}
```

### 5. Add UI Panel in QuoteFormView

In `Views/QuoteFormView.cs`:

1. Add a `RadioButton` for the new pricing type alongside the existing ones.
2. Create a panel with the input controls for this pricing type.
3. Show/hide the panel when the radio button is selected.
4. In the "Add Item" handler, construct a `QuoteItem` with:
   - `PricingType = "PorHora"` (your new type string)
   - `Description` = user-facing description in Spanish
   - `UnitPrice` = calculated or entered price
   - `Quantity` = quantity
   - `Subtotal` = `UnitPrice * Quantity`
   - `CalculationData` = serialized JSON (if applicable)

## Key Principles

- **`UnitPrice` and `Subtotal` are final snapshots** — they represent the definitive price the client sees.
- **`CalculationData` is optional internal metadata** — never shown in PDFs, used only for internal reference in `QuoteDetailView`.
- **Users can always override** the description and price manually, regardless of pricing type.
- **No schema changes needed** — `PricingType` is a string field, and `CalculationData` is a flexible JSON text field.

## Checklist

- [ ] PricingType string chosen (Spanish, descriptive)
- [ ] Data shape class added to `CalculationDetailHelper` (if using JSON metadata)
- [ ] `ToJson` / `Parse*Data` methods added
- [ ] `GetReadableSummary()` updated with new case
- [ ] Calculation method added to `QuoteCalculationService` (if needed)
- [ ] Radio button and UI panel added to `QuoteFormView`
- [ ] QuoteItem construction logic in "Add Item" handler
- [ ] All UI text in Spanish
- [ ] `dotnet build` passes
