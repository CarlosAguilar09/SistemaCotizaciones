using System.Reflection;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using SistemaCotizaciones.Models;
using MigraDocColor = MigraDoc.DocumentObjectModel.Color;

namespace SistemaCotizaciones.Services
{
    public class PdfExportService
    {
        private static readonly MigraDocColor BrandPrimary = new(45, 52, 54);
        private static readonly MigraDocColor BrandAccent = new(225, 112, 85);
        private static readonly MigraDocColor RowAlt = new(248, 249, 250);
        private static readonly MigraDocColor TextGray = new(100, 100, 100);
        private static readonly MigraDocColor BorderLight = new(206, 214, 224);

        // Letter page (21.59cm) minus 2cm margins each side
        private const double ContentWidth = 17.5;

        private readonly AppSettingService _settingService = new();

        public void ExportQuoteToPdf(Quote quote, List<QuoteItem> items, string filePath)
        {
            var settings = _settingService.Get();
            ExportQuoteToPdf(quote, items, filePath, settings);
        }

        public void ExportQuoteToPdf(Quote quote, List<QuoteItem> items, string filePath, AppSetting settings)
        {
            string? logoTempPath = null;
            try
            {
                logoTempPath = ResolveLogo(settings);
                var document = CreateDocument(quote, items, logoTempPath, settings);

                var renderer = new PdfDocumentRenderer(true);
                renderer.Document = document;
                renderer.RenderDocument();
                renderer.PdfDocument.Save(filePath);
            }
            finally
            {
                if (logoTempPath != null && File.Exists(logoTempPath))
                {
                    try { File.Delete(logoTempPath); } catch { /* temp file cleanup best-effort */ }
                }
            }
        }

        private static string FormatQuoteNumber(Quote quote)
        {
            return $"COT-{quote.Date.Year}-{quote.Id:D3}";
        }

        #region Document Setup

        private Document CreateDocument(Quote quote, List<QuoteItem> items, string? logoPath, AppSetting settings)
        {
            var document = new Document();
            var quoteNumber = FormatQuoteNumber(quote);
            document.Info.Title = $"Cotización {quoteNumber}";
            document.Info.Author = settings.CompanyName;

            DefineStyles(document);

            var section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.Letter;
            section.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(2);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(2);
            section.PageSetup.RightMargin = Unit.FromCentimeter(2);

            AddCompanyHeader(section, logoPath, settings);
            AddAccentSeparator(section);
            AddQuoteAndClientInfo(section, quote, quoteNumber, settings);
            AddItemsTable(section, items);
            AddTotal(section, quote);
            AddNotes(section, quote);
            AddTermsAndConditions(section, quote, settings);
            AddFooter(section, settings);

            return document;
        }

        private void DefineStyles(Document document)
        {
            var style = document.Styles["Normal"]!;
            style.Font.Name = "Arial";
            style.Font.Size = 10;
            style.Font.Color = BrandPrimary;

            style = document.Styles.AddStyle("CompanyName", "Normal");
            style.Font.Size = 16;
            style.Font.Bold = true;
            style.Font.Color = BrandPrimary;

            style = document.Styles.AddStyle("CompanyInfo", "Normal");
            style.Font.Size = 8;
            style.Font.Color = TextGray;

            style = document.Styles.AddStyle("QuoteTitle", "Normal");
            style.Font.Size = 20;
            style.Font.Bold = true;
            style.Font.Color = BrandPrimary;

            style = document.Styles.AddStyle("QuoteNumber", "Normal");
            style.Font.Size = 12;
            style.Font.Bold = true;
            style.Font.Color = BrandAccent;

            style = document.Styles.AddStyle("Label", "Normal");
            style.Font.Bold = true;
            style.Font.Size = 9;
            style.Font.Color = BrandPrimary;

            style = document.Styles.AddStyle("Value", "Normal");
            style.Font.Size = 9;

            style = document.Styles.AddStyle("TableHeader", "Normal");
            style.Font.Bold = true;
            style.Font.Size = 9;
            style.Font.Color = Colors.White;

            style = document.Styles.AddStyle("TableCell", "Normal");
            style.Font.Size = 9;

            style = document.Styles.AddStyle("TotalLabel", "Normal");
            style.Font.Size = 13;
            style.Font.Bold = true;
            style.Font.Color = BrandPrimary;

            style = document.Styles.AddStyle("TermsTitle", "Normal");
            style.Font.Size = 9;
            style.Font.Bold = true;
            style.Font.Color = BrandPrimary;

            style = document.Styles.AddStyle("TermsBody", "Normal");
            style.Font.Size = 8;
            style.Font.Color = TextGray;

            style = document.Styles.AddStyle("Footer", "Normal");
            style.Font.Size = 8;
            style.Font.Color = TextGray;
        }

        #endregion

        #region Company Header

        private void AddCompanyHeader(Section section, string? logoPath, AppSetting settings)
        {
            var table = section.AddTable();
            table.Borders.Visible = false;

            table.AddColumn(Unit.FromCentimeter(4));
            table.AddColumn(Unit.FromCentimeter(ContentWidth - 4));

            var row = table.AddRow();
            row.VerticalAlignment = VerticalAlignment.Center;

            if (logoPath != null)
            {
                var image = row.Cells[0].AddImage(logoPath);
                image.Width = Unit.FromCentimeter(3);
                image.LockAspectRatio = true;
            }

            var infoCell = row.Cells[1];
            infoCell.Format.Alignment = ParagraphAlignment.Right;

            var p = infoCell.AddParagraph(settings.CompanyName);
            p.Style = "CompanyName";
            p.Format.Alignment = ParagraphAlignment.Right;

            if (!string.IsNullOrWhiteSpace(settings.RFC))
            {
                p = infoCell.AddParagraph($"RFC: {settings.RFC}");
                p.Style = "CompanyInfo";
                p.Format.Alignment = ParagraphAlignment.Right;
            }

            if (!string.IsNullOrWhiteSpace(settings.Address))
            {
                p = infoCell.AddParagraph(settings.Address);
                p.Style = "CompanyInfo";
                p.Format.Alignment = ParagraphAlignment.Right;
            }

            if (!string.IsNullOrWhiteSpace(settings.City))
            {
                p = infoCell.AddParagraph(settings.City);
                p.Style = "CompanyInfo";
                p.Format.Alignment = ParagraphAlignment.Right;
            }

            // Build contact line: phone | email
            var contactParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(settings.Phone))
                contactParts.Add($"Tel: {settings.Phone}");
            if (!string.IsNullOrWhiteSpace(settings.Email))
                contactParts.Add(settings.Email);

            if (contactParts.Count > 0)
            {
                p = infoCell.AddParagraph(string.Join("  |  ", contactParts));
                p.Style = "CompanyInfo";
                p.Format.Alignment = ParagraphAlignment.Right;
                p.Format.SpaceBefore = Unit.FromCentimeter(0.1);
            }

            // Website and social media
            var extraParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(settings.Website))
                extraParts.Add(settings.Website);
            if (!string.IsNullOrWhiteSpace(settings.SocialMedia))
                extraParts.Add(settings.SocialMedia);

            if (extraParts.Count > 0)
            {
                p = infoCell.AddParagraph(string.Join("  |  ", extraParts));
                p.Style = "CompanyInfo";
                p.Format.Alignment = ParagraphAlignment.Right;
            }
        }

        private void AddAccentSeparator(Section section)
        {
            var p = section.AddParagraph();
            p.Format.SpaceBefore = Unit.FromCentimeter(0.3);
            p.Format.Borders.Bottom.Width = 2;
            p.Format.Borders.Bottom.Color = BrandAccent;
            p.Format.SpaceAfter = Unit.FromCentimeter(0.4);
        }

        #endregion

        #region Quote & Client Info

        private void AddQuoteAndClientInfo(Section section, Quote quote, string quoteNumber, AppSetting settings)
        {
            var p = section.AddParagraph("COTIZACIÓN");
            p.Style = "QuoteTitle";

            p = section.AddParagraph(quoteNumber);
            p.Style = "QuoteNumber";
            p.Format.SpaceAfter = Unit.FromCentimeter(0.4);

            var table = section.AddTable();
            table.Borders.Visible = false;
            var halfWidth = ContentWidth / 2;
            table.AddColumn(Unit.FromCentimeter(halfWidth));
            table.AddColumn(Unit.FromCentimeter(halfWidth));

            // Row 1: Date + Client name
            var row = table.AddRow();

            p = row.Cells[0].AddParagraph();
            p.AddFormattedText("Fecha: ", "Label");
            p.AddText(quote.Date.ToString("dd/MM/yyyy"));

            p = row.Cells[1].AddParagraph();
            p.AddFormattedText("Cliente: ", "Label");
            p.AddText(quote.ClientName);

            // Row 2: Validity + Client phone
            row = table.AddRow();

            p = row.Cells[0].AddParagraph();
            p.AddFormattedText("Vigencia: ", "Label");
            p.AddText(quote.Date.AddDays(settings.QuoteValidityDays).ToString("dd/MM/yyyy"));

            if (quote.Cliente != null && !string.IsNullOrWhiteSpace(quote.Cliente.Phone))
            {
                p = row.Cells[1].AddParagraph();
                p.AddFormattedText("Teléfono: ", "Label");
                p.AddText(quote.Cliente.Phone);
            }

            // Row 3: Client email (if available)
            if (quote.Cliente != null && !string.IsNullOrWhiteSpace(quote.Cliente.Email))
            {
                row = table.AddRow();
                p = row.Cells[1].AddParagraph();
                p.AddFormattedText("Correo: ", "Label");
                p.AddText(quote.Cliente.Email);
            }

            p = section.AddParagraph();
            p.Format.SpaceAfter = Unit.FromCentimeter(0.3);
        }

        #endregion

        #region Items Table

        private void AddItemsTable(Section section, List<QuoteItem> items)
        {
            var table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Borders.Color = BorderLight;
            table.Format.Font.Size = 9;

            var colNum = table.AddColumn(Unit.FromCentimeter(1));
            colNum.Format.Alignment = ParagraphAlignment.Center;

            table.AddColumn(Unit.FromCentimeter(8.5));

            var colQty = table.AddColumn(Unit.FromCentimeter(2));
            colQty.Format.Alignment = ParagraphAlignment.Center;

            var colPrice = table.AddColumn(Unit.FromCentimeter(3));
            colPrice.Format.Alignment = ParagraphAlignment.Right;

            var colSubtotal = table.AddColumn(Unit.FromCentimeter(3));
            colSubtotal.Format.Alignment = ParagraphAlignment.Right;

            // Header row
            var headerRow = table.AddRow();
            headerRow.Shading.Color = BrandPrimary;
            headerRow.Format.Font.Color = Colors.White;
            headerRow.Format.Font.Bold = true;
            headerRow.HeadingFormat = true;
            headerRow.VerticalAlignment = VerticalAlignment.Center;
            headerRow.Height = Unit.FromCentimeter(0.7);

            headerRow.Cells[0].AddParagraph("#");
            headerRow.Cells[1].AddParagraph("Producto / Servicio");
            headerRow.Cells[2].AddParagraph("Cantidad");
            headerRow.Cells[3].AddParagraph("Precio Unit.");
            headerRow.Cells[4].AddParagraph("Subtotal");

            // Data rows
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var row = table.AddRow();
                row.VerticalAlignment = VerticalAlignment.Center;
                row.Height = Unit.FromCentimeter(0.6);

                if (i % 2 == 1)
                    row.Shading.Color = RowAlt;

                row.Cells[0].AddParagraph((i + 1).ToString());
                row.Cells[1].AddParagraph(item.Description);
                row.Cells[2].AddParagraph(item.Quantity % 1 == 0
                    ? item.Quantity.ToString("0")
                    : item.Quantity.ToString("0.##"));
                row.Cells[3].AddParagraph(item.UnitPrice.ToString("C2"));
                row.Cells[4].AddParagraph(item.Subtotal.ToString("C2"));
            }
        }

        #endregion

        #region Total & Notes

        private void AddTotal(Section section, Quote quote)
        {
            var totalsTable = section.AddTable();
            totalsTable.Borders.Visible = false;
            totalsTable.Format.Font.Size = 10;

            // Two columns: label (right-aligned) + value (right-aligned)
            var labelWidth = ContentWidth - 4;
            totalsTable.AddColumn(Unit.FromCentimeter(labelWidth));
            totalsTable.AddColumn(Unit.FromCentimeter(4));

            totalsTable.Format.SpaceBefore = Unit.FromCentimeter(0.5);

            // Subtotal row (always shown)
            var row = totalsTable.AddRow();
            row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
            row.Cells[1].Format.Alignment = ParagraphAlignment.Right;
            row.Cells[0].AddParagraph("Subtotal:");
            row.Cells[1].AddParagraph(quote.Subtotal.ToString("C2"));

            // Discount row (only if discount > 0)
            if (quote.DiscountPercent > 0)
            {
                row = totalsTable.AddRow();
                row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[1].Format.Alignment = ParagraphAlignment.Right;
                var p = row.Cells[0].AddParagraph($"Descuento ({quote.DiscountPercent:0.##}%):");
                p.Format.Font.Color = BrandAccent;
                var pVal = row.Cells[1].AddParagraph($"-{quote.DiscountAmount:C2}");
                pVal.Format.Font.Color = BrandAccent;
            }

            // IVA row (only if IVA > 0)
            if (quote.IvaRate > 0)
            {
                row = totalsTable.AddRow();
                row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[1].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[0].AddParagraph($"IVA ({quote.IvaRate:0.##}%):");
                row.Cells[1].AddParagraph(quote.IvaAmount.ToString("C2"));
            }

            // Total row (always shown, bold)
            row = totalsTable.AddRow();
            row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
            row.Cells[1].Format.Alignment = ParagraphAlignment.Right;
            row.Format.Font.Bold = true;
            row.Format.Font.Size = 13;
            row.Cells[0].AddParagraph("Total:");
            row.Cells[1].AddParagraph(quote.Total.ToString("C2"));
        }

        private void AddNotes(Section section, Quote quote)
        {
            if (string.IsNullOrWhiteSpace(quote.Notes))
                return;

            var p = section.AddParagraph();
            p.Format.SpaceBefore = Unit.FromCentimeter(0.8);

            p = section.AddParagraph();
            p.AddFormattedText("Notas: ", "Label");
            p.AddText(quote.Notes);
        }

        #endregion

        #region Terms & Conditions

        private void AddTermsAndConditions(Section section, Quote quote, AppSetting settings)
        {
            var p = section.AddParagraph();
            p.Format.SpaceBefore = Unit.FromCentimeter(1);
            p.Format.Borders.Top.Width = 0.5;
            p.Format.Borders.Top.Color = BorderLight;
            p.Format.SpaceAfter = Unit.FromCentimeter(0.3);

            p = section.AddParagraph("TÉRMINOS Y CONDICIONES");
            p.Style = "TermsTitle";
            p.Format.SpaceAfter = Unit.FromCentimeter(0.2);

            var terms = _settingService.GetTerms(settings);

            foreach (var term in terms)
            {
                var resolved = _settingService.ResolveTermPlaceholders(term, quote, settings);
                p = section.AddParagraph($"•  {resolved}");
                p.Style = "TermsBody";
                p.Format.LeftIndent = Unit.FromCentimeter(0.5);
                p.Format.SpaceAfter = Unit.FromPoint(2);
            }
        }

        #endregion

        #region Footer

        private void AddFooter(Section section, AppSetting settings)
        {
            var p = section.AddParagraph();
            p.Format.SpaceBefore = Unit.FromCentimeter(1.5);
            p.Format.Borders.Top.Width = 0.5;
            p.Format.Borders.Top.Color = BorderLight;
            p.Format.SpaceAfter = Unit.FromCentimeter(0.2);

            var table = section.AddTable();
            table.Borders.Visible = false;
            table.AddColumn(Unit.FromCentimeter(ContentWidth / 2));
            table.AddColumn(Unit.FromCentimeter(ContentWidth / 2));

            var row = table.AddRow();

            var footerLeft = settings.CompanyName;
            if (!string.IsNullOrWhiteSpace(settings.City))
            {
                // Extract just the city name (before first comma if present)
                var cityShort = settings.City.Split(',')[0].Trim();
                footerLeft += $" — {cityShort}";
            }

            p = row.Cells[0].AddParagraph(footerLeft);
            p.Style = "Footer";

            p = row.Cells[1].AddParagraph($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}");
            p.Style = "Footer";
            p.Format.Alignment = ParagraphAlignment.Right;
        }

        #endregion

        #region Logo Helper

        /// <summary>
        /// Resolves the logo to a temp file path. Uses custom logo if set, otherwise embedded resource.
        /// </summary>
        private static string? ResolveLogo(AppSetting settings)
        {
            // Try custom logo first
            if (!string.IsNullOrEmpty(settings.LogoPath) && File.Exists(settings.LogoPath))
            {
                var tempPath = Path.Combine(Path.GetTempPath(),
                    $"cubosigns_logo_{Guid.NewGuid():N}{Path.GetExtension(settings.LogoPath)}");
                File.Copy(settings.LogoPath, tempPath, overwrite: true);
                return tempPath;
            }

            // Fall back to embedded resource
            return ExtractLogoToTempFile();
        }

        private static string? ExtractLogoToTempFile()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("logo.png", StringComparison.OrdinalIgnoreCase));

            if (resourceName == null)
                return null;

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                return null;

            var tempPath = Path.Combine(Path.GetTempPath(), $"cubosigns_logo_{Guid.NewGuid():N}.png");
            using var fs = File.Create(tempPath);
            stream.CopyTo(fs);
            return tempPath;
        }

        #endregion
    }
}
