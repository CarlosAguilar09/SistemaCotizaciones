using ClosedXML.Excel;
using SistemaCotizaciones.Models;
using SistemaCotizaciones.Repositories;
using System.Data;
using System.Globalization;

namespace SistemaCotizaciones.Services
{
    public class ImportResult<T>
    {
        public DataTable PreviewData { get; set; } = new();
        public List<T> ValidItems { get; set; } = new();
        public int ValidCount => ValidItems.Count;
        public int ErrorCount { get; set; }
        public int TotalRows { get; set; }
    }

    public class ImportExportService
    {
        private readonly ProductRepository _productRepo = new();
        private readonly ClienteRepository _clienteRepo = new();
        private readonly MaterialRepository _materialRepo = new();
        private readonly AreaPricingPresetRepository _presetRepo = new();

        private const string DataSheet = "Datos";
        private const string InstructionsSheet = "Instrucciones";

        #region Template Generation

        public void GenerateProductTemplate(string filePath)
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet(DataSheet);

            string[] headers = ["Tipo", "Nombre", "Descripción", "Precio"];
            WriteHeaders(ws, headers);

            ws.Cell(2, 1).Value = "Producto";
            ws.Cell(2, 2).Value = "Letrero de Canal";
            ws.Cell(2, 3).Value = "Letrero iluminado para exterior";
            ws.Cell(2, 4).Value = 3500;

            ws.Cell(3, 1).Value = "Servicio";
            ws.Cell(3, 2).Value = "Diseño de Logotipo";
            ws.Cell(3, 3).Value = "Diseño profesional de logotipo";
            ws.Cell(3, 4).Value = 2500;

            StyleExampleRows(ws, 2, 3, headers.Length);
            AutoFitColumns(ws, headers.Length);

            AddInstructions(wb, "Productos", [
                "Tipo — 'Producto' o 'Servicio'. Si se deja vacío, se asume 'Producto'.",
                "Nombre — Obligatorio. El nombre del producto o servicio.",
                "Descripción — Opcional. Una descripción breve del producto.",
                "Precio — El precio unitario en pesos. Si se deja vacío, se asume 0."
            ]);

            wb.SaveAs(filePath);
        }

        public void GenerateClientTemplate(string filePath)
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet(DataSheet);

            string[] headers = ["Nombre", "Teléfono", "Correo Electrónico"];
            WriteHeaders(ws, headers);

            ws.Cell(2, 1).Value = "Tacos El Gordo";
            ws.Cell(2, 2).Value = "686-555-0101";
            ws.Cell(2, 3).Value = "contacto@tacoselgordo.mx";

            ws.Cell(3, 1).Value = "Farmacia San Martín";
            ws.Cell(3, 2).Value = "686-555-0202";
            ws.Cell(3, 3).Value = "";

            StyleExampleRows(ws, 2, 3, headers.Length);
            AutoFitColumns(ws, headers.Length);

            AddInstructions(wb, "Clientes", [
                "Nombre — Obligatorio. El nombre del cliente o empresa.",
                "Teléfono — Opcional. Número de teléfono de contacto.",
                "Correo Electrónico — Opcional. Dirección de correo electrónico."
            ]);

            wb.SaveAs(filePath);
        }

        public void GenerateMaterialTemplate(string filePath)
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet(DataSheet);

            string[] headers = ["Material", "Unidad", "Descripción", "Variante", "Opción", "Precio"];
            WriteHeaders(ws, headers);

            ws.Cell(2, 1).Value = "Vinil";
            ws.Cell(2, 2).Value = "m²";
            ws.Cell(2, 3).Value = "";
            ws.Cell(2, 4).Value = "De impresión";
            ws.Cell(2, 5).Value = "Normal";
            ws.Cell(2, 6).Value = 160;

            ws.Cell(3, 1).Value = "Vinil";
            ws.Cell(3, 2).Value = "m²";
            ws.Cell(3, 3).Value = "";
            ws.Cell(3, 4).Value = "De impresión";
            ws.Cell(3, 5).Value = "c/suaje";
            ws.Cell(3, 6).Value = 240;

            ws.Cell(4, 1).Value = "Lona";
            ws.Cell(4, 2).Value = "m²";
            ws.Cell(4, 3).Value = "";
            ws.Cell(4, 4).Value = "Banner 13oz";
            ws.Cell(4, 5).Value = "Normal";
            ws.Cell(4, 6).Value = 180;

            StyleExampleRows(ws, 2, 4, headers.Length);
            AutoFitColumns(ws, headers.Length);

            AddInstructions(wb, "Materiales", [
                "Material — Obligatorio. Nombre del material (ej: Vinil, Lona).",
                "Unidad — Obligatorio. Unidad de medida (ej: m², pieza).",
                "Descripción — Opcional. Descripción del material.",
                "Variante — Obligatorio. Nombre de la variante (ej: De impresión, Banner 13oz).",
                "Opción — Obligatorio. Nombre de la opción (ej: Normal, c/suaje).",
                "Precio — El precio por unidad. Debe ser mayor o igual a 0.",
                "",
                "NOTA: Las filas con el mismo nombre de Material se agrupan automáticamente.",
                "Puede tener múltiples variantes por material y múltiples opciones por variante."
            ]);

            wb.SaveAs(filePath);
        }

        public void GeneratePresetTemplate(string filePath)
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet(DataSheet);

            string[] headers = ["Nombre", "Factor de Ancho", "Precio Base/m²", "Espesor (mm)", "Etiqueta", "Precio/m²"];
            WriteHeaders(ws, headers);

            ws.Cell(2, 1).Value = "Letras fabricadas";
            ws.Cell(2, 2).Value = 0.6;
            ws.Cell(2, 3).Value = 1200;
            ws.Cell(2, 4).Value = 10;
            ws.Cell(2, 5).Value = "PVC 10mm";
            ws.Cell(2, 6).Value = 900;

            ws.Cell(3, 1).Value = "Letras fabricadas";
            ws.Cell(3, 2).Value = 0.6;
            ws.Cell(3, 3).Value = 1200;
            ws.Cell(3, 4).Value = 20;
            ws.Cell(3, 5).Value = "PVC 20mm";
            ws.Cell(3, 6).Value = 1200;

            StyleExampleRows(ws, 2, 3, headers.Length);
            AutoFitColumns(ws, headers.Length);

            AddInstructions(wb, "Estándares de Precio", [
                "Nombre — Obligatorio. Nombre del estándar de precio.",
                "Factor de Ancho — Obligatorio. Factor multiplicador de ancho (ej: 0.6).",
                "Precio Base/m² — Obligatorio. Precio base por metro cuadrado.",
                "Espesor (mm) — Opcional. Espesor en milímetros para niveles de grosor.",
                "Etiqueta — Opcional. Etiqueta descriptiva del nivel (ej: PVC 10mm).",
                "Precio/m² — Precio por m² para este nivel de espesor.",
                "",
                "NOTA: Las filas con el mismo Nombre se agrupan automáticamente.",
                "Las columnas de espesor son opcionales — se usan para definir niveles de grosor."
            ]);

            wb.SaveAs(filePath);
        }

        #endregion

        #region Export

        public void ExportProducts(string filePath)
        {
            var products = _productRepo.GetAll();
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet(DataSheet);

            string[] headers = ["Tipo", "Nombre", "Descripción", "Precio"];
            WriteHeaders(ws, headers);

            for (int i = 0; i < products.Count; i++)
            {
                var p = products[i];
                int row = i + 2;
                ws.Cell(row, 1).Value = p.Type;
                ws.Cell(row, 2).Value = p.Name;
                ws.Cell(row, 3).Value = p.Description ?? "";
                ws.Cell(row, 4).Value = p.Price;
            }

            AutoFitColumns(ws, headers.Length);
            wb.SaveAs(filePath);
        }

        public void ExportClients(string filePath)
        {
            var clients = _clienteRepo.GetAll();
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet(DataSheet);

            string[] headers = ["Nombre", "Teléfono", "Correo Electrónico"];
            WriteHeaders(ws, headers);

            for (int i = 0; i < clients.Count; i++)
            {
                var c = clients[i];
                int row = i + 2;
                ws.Cell(row, 1).Value = c.Name;
                ws.Cell(row, 2).Value = c.Phone ?? "";
                ws.Cell(row, 3).Value = c.Email ?? "";
            }

            AutoFitColumns(ws, headers.Length);
            wb.SaveAs(filePath);
        }

        public void ExportMaterials(string filePath)
        {
            var materials = _materialRepo.GetAll();
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet(DataSheet);

            string[] headers = ["Material", "Unidad", "Descripción", "Variante", "Opción", "Precio"];
            WriteHeaders(ws, headers);

            int row = 2;
            foreach (var m in materials)
            {
                foreach (var v in m.Variants)
                {
                    foreach (var o in v.Options)
                    {
                        ws.Cell(row, 1).Value = m.Name;
                        ws.Cell(row, 2).Value = m.Unit;
                        ws.Cell(row, 3).Value = m.Description ?? "";
                        ws.Cell(row, 4).Value = v.Name;
                        ws.Cell(row, 5).Value = o.Name;
                        ws.Cell(row, 6).Value = o.Price;
                        row++;
                    }
                }
            }

            AutoFitColumns(ws, headers.Length);
            wb.SaveAs(filePath);
        }

        public void ExportPresets(string filePath)
        {
            var presets = _presetRepo.GetAll();
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet(DataSheet);

            string[] headers = ["Nombre", "Factor de Ancho", "Precio Base/m²", "Espesor (mm)", "Etiqueta", "Precio/m²"];
            WriteHeaders(ws, headers);

            int row = 2;
            foreach (var p in presets)
            {
                if (p.ThicknessTiers.Count == 0)
                {
                    ws.Cell(row, 1).Value = p.Name;
                    ws.Cell(row, 2).Value = p.WidthFactor;
                    ws.Cell(row, 3).Value = p.PricePerSquareMeter;
                    row++;
                }
                else
                {
                    foreach (var t in p.ThicknessTiers)
                    {
                        ws.Cell(row, 1).Value = p.Name;
                        ws.Cell(row, 2).Value = p.WidthFactor;
                        ws.Cell(row, 3).Value = p.PricePerSquareMeter;
                        ws.Cell(row, 4).Value = t.ThicknessMm;
                        ws.Cell(row, 5).Value = t.Label ?? "";
                        ws.Cell(row, 6).Value = t.PricePerSquareMeter;
                        row++;
                    }
                }
            }

            AutoFitColumns(ws, headers.Length);
            wb.SaveAs(filePath);
        }

        #endregion

        #region Parse & Validate

        public ImportResult<Product> ParseProducts(string filePath)
        {
            using var wb = new XLWorkbook(filePath);
            var ws = GetDataSheet(wb);

            var dt = new DataTable();
            dt.Columns.Add("Fila", typeof(int));
            dt.Columns.Add("Tipo", typeof(string));
            dt.Columns.Add("Nombre", typeof(string));
            dt.Columns.Add("Descripción", typeof(string));
            dt.Columns.Add("Precio", typeof(string));
            dt.Columns.Add("Estado", typeof(string));

            var validProducts = new List<Product>();
            int errorCount = 0;
            int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                var type = ws.Cell(row, 1).GetString().Trim();
                var name = ws.Cell(row, 2).GetString().Trim();
                var desc = ws.Cell(row, 3).GetString().Trim();
                var priceStr = ws.Cell(row, 4).GetString().Trim();

                if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(type) && string.IsNullOrEmpty(priceStr))
                    continue; // Skip completely empty rows

                var errors = new List<string>();

                if (string.IsNullOrEmpty(name))
                    errors.Add("Nombre es obligatorio");

                if (string.IsNullOrEmpty(type))
                    type = "Producto";
                else if (type != "Producto" && type != "Servicio")
                    errors.Add("Tipo debe ser 'Producto' o 'Servicio'");

                decimal price = 0;
                if (!string.IsNullOrEmpty(priceStr) && !TryParseDecimal(priceStr, out price))
                    errors.Add("Precio no es un número válido");
                else if (price < 0)
                    errors.Add("Precio debe ser mayor o igual a 0");

                string status = errors.Count == 0 ? "✅" : $"❌ {string.Join("; ", errors)}";
                dt.Rows.Add(row, type, name, desc, priceStr, status);

                if (errors.Count == 0)
                {
                    validProducts.Add(new Product
                    {
                        Type = type,
                        Name = name,
                        Description = string.IsNullOrEmpty(desc) ? null : desc,
                        Price = price
                    });
                }
                else
                {
                    errorCount++;
                }
            }

            return new ImportResult<Product>
            {
                PreviewData = dt,
                ValidItems = validProducts,
                ErrorCount = errorCount,
                TotalRows = dt.Rows.Count
            };
        }

        public ImportResult<Cliente> ParseClients(string filePath)
        {
            using var wb = new XLWorkbook(filePath);
            var ws = GetDataSheet(wb);

            var dt = new DataTable();
            dt.Columns.Add("Fila", typeof(int));
            dt.Columns.Add("Nombre", typeof(string));
            dt.Columns.Add("Teléfono", typeof(string));
            dt.Columns.Add("Correo", typeof(string));
            dt.Columns.Add("Estado", typeof(string));

            var validClients = new List<Cliente>();
            int errorCount = 0;
            int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                var name = ws.Cell(row, 1).GetString().Trim();
                var phone = ws.Cell(row, 2).GetString().Trim();
                var email = ws.Cell(row, 3).GetString().Trim();

                if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(phone) && string.IsNullOrEmpty(email))
                    continue;

                var errors = new List<string>();

                if (string.IsNullOrEmpty(name))
                    errors.Add("Nombre es obligatorio");

                string status = errors.Count == 0 ? "✅" : $"❌ {string.Join("; ", errors)}";
                dt.Rows.Add(row, name, phone, email, status);

                if (errors.Count == 0)
                {
                    validClients.Add(new Cliente
                    {
                        Name = name,
                        Phone = string.IsNullOrEmpty(phone) ? null : phone,
                        Email = string.IsNullOrEmpty(email) ? null : email
                    });
                }
                else
                {
                    errorCount++;
                }
            }

            return new ImportResult<Cliente>
            {
                PreviewData = dt,
                ValidItems = validClients,
                ErrorCount = errorCount,
                TotalRows = dt.Rows.Count
            };
        }

        public ImportResult<Material> ParseMaterials(string filePath)
        {
            using var wb = new XLWorkbook(filePath);
            var ws = GetDataSheet(wb);

            var dt = new DataTable();
            dt.Columns.Add("Fila", typeof(int));
            dt.Columns.Add("Material", typeof(string));
            dt.Columns.Add("Unidad", typeof(string));
            dt.Columns.Add("Descripción", typeof(string));
            dt.Columns.Add("Variante", typeof(string));
            dt.Columns.Add("Opción", typeof(string));
            dt.Columns.Add("Precio", typeof(string));
            dt.Columns.Add("Estado", typeof(string));

            // First pass: validate rows and collect valid data
            var validRows = new List<(string Material, string Unit, string Desc, string Variant, string Option, decimal Price)>();
            // Track first-seen Unit per material for consistency validation
            var materialUnits = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            int errorCount = 0;
            int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                var material = ws.Cell(row, 1).GetString().Trim();
                var unit = ws.Cell(row, 2).GetString().Trim();
                var desc = ws.Cell(row, 3).GetString().Trim();
                var variant = ws.Cell(row, 4).GetString().Trim();
                var option = ws.Cell(row, 5).GetString().Trim();
                var priceStr = ws.Cell(row, 6).GetString().Trim();

                if (string.IsNullOrEmpty(material) && string.IsNullOrEmpty(variant) && string.IsNullOrEmpty(option))
                    continue;

                var errors = new List<string>();

                if (string.IsNullOrEmpty(material))
                    errors.Add("Material es obligatorio");
                if (string.IsNullOrEmpty(unit))
                    errors.Add("Unidad es obligatoria");
                if (string.IsNullOrEmpty(variant))
                    errors.Add("Variante es obligatoria");
                if (string.IsNullOrEmpty(option))
                    errors.Add("Opción es obligatoria");

                // Validate unit consistency within the same material
                if (!string.IsNullOrEmpty(material) && !string.IsNullOrEmpty(unit))
                {
                    if (materialUnits.TryGetValue(material, out var existingUnit))
                    {
                        if (!string.Equals(existingUnit, unit, StringComparison.OrdinalIgnoreCase))
                            errors.Add($"Unidad inconsistente: '{unit}' vs '{existingUnit}' para material '{material}'");
                    }
                    else
                    {
                        materialUnits[material] = unit;
                    }
                }

                decimal price = 0;
                if (!string.IsNullOrEmpty(priceStr) && !TryParseDecimal(priceStr, out price))
                    errors.Add("Precio no es un número válido");
                else if (price < 0)
                    errors.Add("Precio debe ser mayor o igual a 0");

                string status = errors.Count == 0 ? "✅" : $"❌ {string.Join("; ", errors)}";
                dt.Rows.Add(row, material, unit, desc, variant, option, priceStr, status);

                if (errors.Count == 0)
                    validRows.Add((material, unit, desc, variant, option, price));
                else
                    errorCount++;
            }

            // Group valid rows into Material entities
            var materials = validRows
                .GroupBy(r => r.Material, StringComparer.OrdinalIgnoreCase)
                .Select(mg =>
                {
                    var first = mg.First();
                    return new Material
                    {
                        Name = mg.Key,
                        Unit = first.Unit,
                        Description = string.IsNullOrEmpty(first.Desc) ? null : first.Desc,
                        Variants = mg
                            .GroupBy(r => r.Variant, StringComparer.OrdinalIgnoreCase)
                            .Select(vg => new MaterialVariant
                            {
                                Name = vg.Key,
                                Options = vg.Select(r => new MaterialOption
                                {
                                    Name = r.Option,
                                    Price = r.Price
                                }).ToList()
                            }).ToList()
                    };
                }).ToList();

            return new ImportResult<Material>
            {
                PreviewData = dt,
                ValidItems = materials,
                ErrorCount = errorCount,
                TotalRows = dt.Rows.Count
            };
        }

        public ImportResult<AreaPricingPreset> ParsePresets(string filePath)
        {
            using var wb = new XLWorkbook(filePath);
            var ws = GetDataSheet(wb);

            var dt = new DataTable();
            dt.Columns.Add("Fila", typeof(int));
            dt.Columns.Add("Nombre", typeof(string));
            dt.Columns.Add("Factor de Ancho", typeof(string));
            dt.Columns.Add("Precio Base/m²", typeof(string));
            dt.Columns.Add("Espesor (mm)", typeof(string));
            dt.Columns.Add("Etiqueta", typeof(string));
            dt.Columns.Add("Precio/m²", typeof(string));
            dt.Columns.Add("Estado", typeof(string));

            var validRows = new List<(string Name, decimal WidthFactor, decimal BasePrice, decimal? Thickness, string? Label, decimal? TierPrice)>();
            // Track first-seen values per preset for consistency validation
            var presetValues = new Dictionary<string, (decimal WidthFactor, decimal BasePrice)>(StringComparer.OrdinalIgnoreCase);
            int errorCount = 0;
            int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = 2; row <= lastRow; row++)
            {
                var name = ws.Cell(row, 1).GetString().Trim();
                var widthStr = ws.Cell(row, 2).GetString().Trim();
                var basePriceStr = ws.Cell(row, 3).GetString().Trim();
                var thicknessStr = ws.Cell(row, 4).GetString().Trim();
                var label = ws.Cell(row, 5).GetString().Trim();
                var tierPriceStr = ws.Cell(row, 6).GetString().Trim();

                if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(widthStr) && string.IsNullOrEmpty(basePriceStr))
                    continue;

                var errors = new List<string>();

                if (string.IsNullOrEmpty(name))
                    errors.Add("Nombre es obligatorio");

                decimal widthFactor = 0;
                if (string.IsNullOrEmpty(widthStr))
                    errors.Add("Factor de Ancho es obligatorio");
                else if (!TryParseDecimal(widthStr, out widthFactor) || widthFactor <= 0)
                    errors.Add("Factor de Ancho debe ser un número mayor a 0");

                decimal basePrice = 0;
                if (string.IsNullOrEmpty(basePriceStr))
                    errors.Add("Precio Base/m² es obligatorio");
                else if (!TryParseDecimal(basePriceStr, out basePrice) || basePrice <= 0)
                    errors.Add("Precio Base/m² debe ser un número mayor a 0");

                // Validate consistency within the same preset
                if (!string.IsNullOrEmpty(name) && widthFactor > 0 && basePrice > 0)
                {
                    if (presetValues.TryGetValue(name, out var existing))
                    {
                        if (existing.WidthFactor != widthFactor)
                            errors.Add($"Factor de Ancho inconsistente: '{widthStr}' vs '{existing.WidthFactor}' para '{name}'");
                        if (existing.BasePrice != basePrice)
                            errors.Add($"Precio Base inconsistente: '{basePriceStr}' vs '{existing.BasePrice}' para '{name}'");
                    }
                    else
                    {
                        presetValues[name] = (widthFactor, basePrice);
                    }
                }

                decimal? thickness = null;
                decimal? tierPrice = null;

                if (!string.IsNullOrEmpty(thicknessStr))
                {
                    if (!TryParseDecimal(thicknessStr, out var t) || t <= 0)
                        errors.Add("Espesor debe ser un número mayor a 0");
                    else
                        thickness = t;
                }

                if (!string.IsNullOrEmpty(tierPriceStr))
                {
                    if (!TryParseDecimal(tierPriceStr, out var tp) || tp <= 0)
                        errors.Add("Precio/m² del nivel debe ser un número mayor a 0");
                    else
                        tierPrice = tp;
                }

                string status = errors.Count == 0 ? "✅" : $"❌ {string.Join("; ", errors)}";
                dt.Rows.Add(row, name, widthStr, basePriceStr, thicknessStr, label, tierPriceStr, status);

                if (errors.Count == 0)
                    validRows.Add((name, widthFactor, basePrice, thickness, string.IsNullOrEmpty(label) ? null : label, tierPrice));
                else
                    errorCount++;
            }

            // Group valid rows into preset entities
            var presets = validRows
                .GroupBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g =>
                {
                    var first = g.First();
                    var preset = new AreaPricingPreset
                    {
                        Name = g.Key,
                        WidthFactor = first.WidthFactor,
                        PricePerSquareMeter = first.BasePrice,
                        ThicknessTiers = g
                            .Where(r => r.Thickness.HasValue)
                            .Select(r => new ThicknessTier
                            {
                                ThicknessMm = r.Thickness!.Value,
                                Label = r.Label,
                                PricePerSquareMeter = r.TierPrice ?? r.BasePrice
                            }).ToList()
                    };
                    return preset;
                }).ToList();

            return new ImportResult<AreaPricingPreset>
            {
                PreviewData = dt,
                ValidItems = presets,
                ErrorCount = errorCount,
                TotalRows = dt.Rows.Count
            };
        }

        #endregion

        #region Commit

        public int CommitProducts(List<Product> products)
        {
            _productRepo.AddRange(products);
            return products.Count;
        }

        public int CommitClients(List<Cliente> clients)
        {
            _clienteRepo.AddRange(clients);
            return clients.Count;
        }

        public int CommitMaterials(List<Material> materials)
        {
            _materialRepo.AddRange(materials);
            return materials.Count;
        }

        public int CommitPresets(List<AreaPricingPreset> presets)
        {
            _presetRepo.AddRange(presets);
            return presets.Count;
        }

        #endregion

        #region Helpers

        private static void WriteHeaders(IXLWorksheet ws, string[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2D3436");
                cell.Style.Font.FontColor = XLColor.White;
            }
        }

        private static void StyleExampleRows(IXLWorksheet ws, int startRow, int endRow, int colCount)
        {
            for (int row = startRow; row <= endRow; row++)
            {
                for (int col = 1; col <= colCount; col++)
                {
                    ws.Cell(row, col).Style.Font.FontColor = XLColor.FromHtml("#636E72");
                    ws.Cell(row, col).Style.Font.Italic = true;
                }
            }
        }

        private static void AutoFitColumns(IXLWorksheet ws, int colCount)
        {
            for (int col = 1; col <= colCount; col++)
            {
                ws.Column(col).AdjustToContents();
                if (ws.Column(col).Width < 15)
                    ws.Column(col).Width = 15;
            }
        }

        private static void AddInstructions(XLWorkbook wb, string entityName, string[] lines)
        {
            var ws = wb.AddWorksheet(InstructionsSheet);
            ws.Cell(1, 1).Value = $"Instrucciones para importar {entityName}";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;

            ws.Cell(3, 1).Value = "Columnas:";
            ws.Cell(3, 1).Style.Font.Bold = true;

            for (int i = 0; i < lines.Length; i++)
                ws.Cell(4 + i, 1).Value = lines[i];

            int notesRow = 4 + lines.Length + 1;
            ws.Cell(notesRow, 1).Value = "Notas:";
            ws.Cell(notesRow, 1).Style.Font.Bold = true;
            ws.Cell(notesRow + 1, 1).Value = "• Los datos de ejemplo en la hoja 'Datos' son solo de referencia. Elimínelos antes de importar.";
            ws.Cell(notesRow + 2, 1).Value = "• La primera fila (encabezados) no se importa.";
            ws.Cell(notesRow + 3, 1).Value = "• Las filas completamente vacías se ignoran automáticamente.";

            ws.Column(1).Width = 80;
        }

        private static IXLWorksheet GetDataSheet(XLWorkbook wb)
        {
            // Try to find "Datos" sheet first, then fall back to first sheet
            if (wb.TryGetWorksheet(DataSheet, out var ws))
                return ws;
            return wb.Worksheet(1);
        }

        private static bool TryParseDecimal(string value, out decimal result)
        {
            // Handle both comma and dot as decimal separators
            value = value.Replace(",", ".");
            return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
        }

        #endregion
    }
}
