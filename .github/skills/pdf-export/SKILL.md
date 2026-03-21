---
name: pdf-export
description: Guide for modifying the PDF export functionality in SistemaCotizaciones. Use this when changing the PDF layout, adding new sections to quotes, or fixing PDF generation issues.
---

# PDF Export Patterns

## Technology

- **Library:** PdfSharp.MigraDoc.Standard 1.51.15
- **Prerequisite:** `Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)` must be called in `Program.Main()` before any PDF operations.

## Color Alias

Always use the `MigraDocColor` alias to avoid ambiguity with `System.Drawing.Color`:

```csharp
using MigraDocColor = MigraDoc.DocumentObjectModel.Color;
```

## Document Structure

The PDF follows this structure:

1. **Header** — "CUBO Signs" title + "Cotización" subtitle
2. **Client Info** — Client name, quote date
3. **Items Table** — Description, Quantity, Unit Price, Subtotal columns
4. **Total Row** — Right-aligned total
5. **Notes Section** — Optional notes from the quote
6. **Footer** — Generation date

## Style Definitions

Custom styles are defined once in `DefineStyles()`:

| Style Name | Purpose |
|-----------|---------|
| `Title` | Document title ("CUBO Signs") |
| `Label` | Field labels ("Cliente:", "Fecha:") |
| `TableHeader` | Table column headers |
| `TableCell` | Table data cells |
| `TotalLabel` | Total amount display |
| `Footer` | Footer text |

## Formatting Conventions

- **Currency:** `{value:C2}` with `CultureInfo("es-MX")` for Mexican peso formatting
- **Dates:** `{date:dd/MM/yyyy}` format
- **Table:** Alternating row colors for readability
- **Font:** Document uses standard fonts compatible with PdfSharp (no custom font embedding)

## Guidelines

- Keep the existing style system — define new styles in `DefineStyles()` rather than inline formatting.
- Test PDF generation with quotes that have all pricing types to ensure nothing breaks.
- The `CalculationData` JSON is **never** shown in PDFs — it's internal-only metadata.
- All text in the PDF is in Spanish.
- The service class (`PdfExportService`) has no database dependencies — it receives a `Quote` and `List<QuoteItem>` as parameters.
