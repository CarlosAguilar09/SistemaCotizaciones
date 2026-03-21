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

        private const int ValidityDays = 15;
        // Letter page (21.59cm) minus 2cm margins each side
        private const double ContentWidth = 17.5;

        public void ExportQuoteToPdf(Quote quote, List<QuoteItem> items, string filePath)
        {
            string? logoTempPath = null;
            try
            {
                logoTempPath = ExtractLogoToTempFile();
                var document = CreateDocument(quote, items, logoTempPath);

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

        private Document CreateDocument(Quote quote, List<QuoteItem> items, string? logoPath)
        {
            var document = new Document();
            var quoteNumber = FormatQuoteNumber(quote);
            document.Info.Title = $"Cotización {quoteNumber}";
            document.Info.Author = "CUBO Signs";

            DefineStyles(document);

            var section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.Letter;
            section.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(2);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(2);
            section.PageSetup.RightMargin = Unit.FromCentimeter(2);

            AddCompanyHeader(section, logoPath);
            AddAccentSeparator(section);
            AddQuoteAndClientInfo(section, quote, quoteNumber);
            AddItemsTable(section, items);
            AddTotal(section, quote);
            AddNotes(section, quote);
            AddTermsAndConditions(section, quote);
            AddFooter(section);

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

        private void AddCompanyHeader(Section section, string? logoPath)
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

            var p = infoCell.AddParagraph("CUBO SIGNS");
            p.Style = "CompanyName";
            p.Format.Alignment = ParagraphAlignment.Right;

            p = infoCell.AddParagraph("Av. José Joaquín Fernández de Lizardi #801-2, Esq. Río Mocorito");
            p.Style = "CompanyInfo";
            p.Format.Alignment = ParagraphAlignment.Right;

            p = infoCell.AddParagraph("Mexicali, Baja California, México, 21290");
            p.Style = "CompanyInfo";
            p.Format.Alignment = ParagraphAlignment.Right;

            p = infoCell.AddParagraph("Tel: 686 370 7018  |  cubosigns.ventas@gmail.com");
            p.Style = "CompanyInfo";
            p.Format.Alignment = ParagraphAlignment.Right;
            p.Format.SpaceBefore = Unit.FromCentimeter(0.1);

            p = infoCell.AddParagraph("Instagram: @cubo_signs");
            p.Style = "CompanyInfo";
            p.Format.Alignment = ParagraphAlignment.Right;
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

        private void AddQuoteAndClientInfo(Section section, Quote quote, string quoteNumber)
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
            p.AddText(quote.Date.AddDays(ValidityDays).ToString("dd/MM/yyyy"));

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
            var p = section.AddParagraph();
            p.Format.SpaceBefore = Unit.FromCentimeter(0.5);
            p.Format.Alignment = ParagraphAlignment.Right;
            p.Style = "TotalLabel";
            p.AddText($"Total: {quote.Total:C2}");
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

        private void AddTermsAndConditions(Section section, Quote quote)
        {
            var p = section.AddParagraph();
            p.Format.SpaceBefore = Unit.FromCentimeter(1);
            p.Format.Borders.Top.Width = 0.5;
            p.Format.Borders.Top.Color = BorderLight;
            p.Format.SpaceAfter = Unit.FromCentimeter(0.3);

            p = section.AddParagraph("TÉRMINOS Y CONDICIONES");
            p.Style = "TermsTitle";
            p.Format.SpaceAfter = Unit.FromCentimeter(0.2);

            var terms = new[]
            {
                "Precios expresados en Moneda Nacional (MXN).",
                "Los precios no incluyen IVA.",
                $"Cotización válida por {ValidityDays} días a partir de la fecha de emisión.",
                "Se requiere un 50% de anticipo para iniciar la producción.",
                "Tiempos de entrega sujetos a confirmación al momento de la orden.",
                "Los colores impresos pueden variar ligeramente respecto a los visualizados en pantalla."
            };

            foreach (var term in terms)
            {
                p = section.AddParagraph($"•  {term}");
                p.Style = "TermsBody";
                p.Format.LeftIndent = Unit.FromCentimeter(0.5);
                p.Format.SpaceAfter = Unit.FromPoint(2);
            }
        }

        #endregion

        #region Footer

        private void AddFooter(Section section)
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

            p = row.Cells[0].AddParagraph("CUBO Signs — Mexicali, B.C.");
            p.Style = "Footer";

            p = row.Cells[1].AddParagraph($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}");
            p.Style = "Footer";
            p.Format.Alignment = ParagraphAlignment.Right;
        }

        #endregion

        #region Logo Helper

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
