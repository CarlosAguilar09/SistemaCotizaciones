using System.Text.Json;

namespace SistemaCotizaciones.Helpers
{
    /// <summary>
    /// Serializes and deserializes calculation metadata stored in QuoteItem.CalculationData.
    /// </summary>
    public static class CalculationDetailHelper
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // --- Area calculation data ---

        public class AreaCalcData
        {
            public decimal Width { get; set; }
            public decimal Height { get; set; }
            public decimal PricePerUnit { get; set; }
            public string Unit { get; set; } = "m²";
            public int? MaterialOptionId { get; set; }
            public string? MaterialLabel { get; set; }
        }

        public static string ToJson(AreaCalcData data) =>
            JsonSerializer.Serialize(data, JsonOptions);

        public static AreaCalcData? ParseAreaData(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try { return JsonSerializer.Deserialize<AreaCalcData>(json, JsonOptions); }
            catch { return null; }
        }

        // --- Custom fabrication calculation data ---

        public class CostLine
        {
            public string Label { get; set; } = string.Empty;
            public decimal Amount { get; set; }
        }

        public class CustomCalcData
        {
            public List<CostLine> Lines { get; set; } = new();
        }

        public static string ToJson(CustomCalcData data) =>
            JsonSerializer.Serialize(data, JsonOptions);

        public static CustomCalcData? ParseCustomData(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try { return JsonSerializer.Deserialize<CustomCalcData>(json, JsonOptions); }
            catch { return null; }
        }

        /// <summary>
        /// Builds a human-readable summary of calculation details for display in the app.
        /// </summary>
        public static string GetReadableSummary(string pricingType, string? calculationData)
        {
            switch (pricingType)
            {
                case "Area":
                    var area = ParseAreaData(calculationData);
                    if (area == null) return "Sin detalles de cálculo.";
                    decimal computedArea = area.Width * area.Height;
                    var lines = new List<string>
                    {
                        $"Ancho: {area.Width:0.##} m",
                        $"Alto: {area.Height:0.##} m",
                        $"Área: {computedArea:0.##} {area.Unit}",
                        $"Precio por {area.Unit}: {area.PricePerUnit:C2}",
                        $"Subtotal calculado: {(computedArea * area.PricePerUnit):C2}"
                    };
                    if (!string.IsNullOrEmpty(area.MaterialLabel))
                        lines.Insert(0, $"Material: {area.MaterialLabel}");
                    return string.Join("\n", lines);

                case "Personalizado":
                    var custom = ParseCustomData(calculationData);
                    if (custom == null || custom.Lines.Count == 0) return "Sin detalles de cálculo.";
                    var result = custom.Lines
                        .Select(l => $"• {l.Label}: {l.Amount:C2}")
                        .ToList();
                    decimal total = custom.Lines.Sum(l => l.Amount);
                    result.Add($"\nTotal calculado: {total:C2}");
                    return string.Join("\n", result);

                default:
                    return "Sin detalles de cálculo adicionales.";
            }
        }
    }
}
