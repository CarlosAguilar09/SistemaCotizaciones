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

            // Piece-based calculation fields (optional — null means direct-dimension mode)
            public string? Text { get; set; }
            public int? PieceCount { get; set; }
            public decimal? WidthFactor { get; set; }
            public int? PresetId { get; set; }
            public string? PresetName { get; set; }
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

                    // Piece-based mode
                    if (area.PieceCount.HasValue && area.PieceCount.Value > 0)
                    {
                        decimal pieceWidth = area.Width;
                        decimal pieceHeight = area.Height;
                        decimal areaPerPiece = pieceWidth * pieceHeight;
                        decimal totalArea = areaPerPiece * area.PieceCount.Value;
                        var pieceLines = new List<string>();
                        if (!string.IsNullOrEmpty(area.PresetName))
                            pieceLines.Add($"Estándar: {area.PresetName}");
                        if (!string.IsNullOrEmpty(area.Text))
                            pieceLines.Add($"Texto: {area.Text}");
                        pieceLines.Add($"Número de piezas: {area.PieceCount.Value}");
                        pieceLines.Add($"Altura por pieza: {pieceHeight:0.##} m");
                        if (area.WidthFactor.HasValue)
                            pieceLines.Add($"Factor de ancho: {area.WidthFactor.Value:0.##}");
                        pieceLines.Add($"Ancho estimado: {pieceWidth:0.##} m");
                        pieceLines.Add($"Área por pieza: {areaPerPiece:0.####} {area.Unit}");
                        pieceLines.Add($"Área total: {totalArea:0.####} {area.Unit}");
                        pieceLines.Add($"Precio por {area.Unit}: {area.PricePerUnit:C2}");
                        pieceLines.Add($"Costo total calculado: {(totalArea * area.PricePerUnit):C2}");
                        if (!string.IsNullOrEmpty(area.MaterialLabel))
                            pieceLines.Insert(0, $"Material: {area.MaterialLabel}");
                        return string.Join("\n", pieceLines);
                    }

                    // Direct-dimension mode (original)
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
