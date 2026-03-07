using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using SistemaCotizaciones.Models;
using MigraDocColor = MigraDoc.DocumentObjectModel.Color;

namespace SistemaCotizaciones.Services
{
    public class PdfExportService
    {
        public void ExportQuoteToPdf(Quote quote, List<QuoteItem> items, string filePath)
        {
            var document = CreateDocument(quote, items);

            var renderer = new PdfDocumentRenderer(true);
            renderer.Document = document;
            renderer.RenderDocument();
            renderer.PdfDocument.Save(filePath);
        }

        private Document CreateDocument(Quote quote, List<QuoteItem> items)
        {
            var document = new Document();
            document.Info.Title = $"Cotización #{quote.Id}";
            document.Info.Author = "SistemaCotizaciones";

            DefineStyles(document);

            var section = document.AddSection();
            section.PageSetup.TopMargin = Unit.FromCentimeter(2);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(2);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(2.5);
            section.PageSetup.RightMargin = Unit.FromCentimeter(2.5);

            AddHeader(section, quote);
            AddClientInfo(section, quote);
            AddItemsTable(section, items);
            AddTotal(section, quote);
            AddNotes(section, quote);
            AddFooter(section);

            return document;
        }

        private void DefineStyles(Document document)
        {
            var style = document.Styles["Normal"]!;
            style.Font.Name = "Arial";
            style.Font.Size = 10;

            style = document.Styles.AddStyle("Title", "Normal");
            style.Font.Size = 18;
            style.Font.Bold = true;
            style.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.5);

            style = document.Styles.AddStyle("Label", "Normal");
            style.Font.Bold = true;
            style.Font.Size = 10;

            style = document.Styles.AddStyle("TableHeader", "Normal");
            style.Font.Bold = true;
            style.Font.Size = 9;
            style.Font.Color = Colors.White;

            style = document.Styles.AddStyle("TableCell", "Normal");
            style.Font.Size = 9;

            style = document.Styles.AddStyle("TotalLabel", "Normal");
            style.Font.Size = 12;
            style.Font.Bold = true;

            style = document.Styles.AddStyle("Footer", "Normal");
            style.Font.Size = 8;
            style.Font.Color = Colors.Gray;
        }

        private void AddHeader(Section section, Quote quote)
        {
            var paragraph = section.AddParagraph($"COTIZACIÓN #{quote.Id}");
            paragraph.Style = "Title";
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            // Separator line
            paragraph = section.AddParagraph();
            paragraph.Format.Borders.Bottom.Width = 1;
            paragraph.Format.Borders.Bottom.Color = Colors.DarkGray;
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.5);
        }

        private void AddClientInfo(Section section, Quote quote)
        {
            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText("Cliente: ", "Label");
            paragraph.AddText(quote.ClientName);

            paragraph = section.AddParagraph();
            paragraph.AddFormattedText("Fecha: ", "Label");
            paragraph.AddText(quote.Date.ToString("dd/MM/yyyy"));

            paragraph = section.AddParagraph();
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.5);
        }

        private void AddItemsTable(Section section, List<QuoteItem> items)
        {
            var table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Borders.Color = Colors.DarkGray;
            table.Format.Font.Size = 9;

            // Define columns
            var colNum = table.AddColumn(Unit.FromCentimeter(1.2));
            colNum.Format.Alignment = ParagraphAlignment.Center;

            var colName = table.AddColumn(Unit.FromCentimeter(7));

            var colQty = table.AddColumn(Unit.FromCentimeter(2));
            colQty.Format.Alignment = ParagraphAlignment.Center;

            var colPrice = table.AddColumn(Unit.FromCentimeter(3));
            colPrice.Format.Alignment = ParagraphAlignment.Right;

            var colSubtotal = table.AddColumn(Unit.FromCentimeter(3));
            colSubtotal.Format.Alignment = ParagraphAlignment.Right;

            // Header row
            var headerRow = table.AddRow();
            headerRow.Shading.Color = new MigraDocColor(50, 50, 50);
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
                    row.Shading.Color = new MigraDocColor(245, 245, 245);

                row.Cells[0].AddParagraph((i + 1).ToString());
                row.Cells[1].AddParagraph(item.Description);
                row.Cells[2].AddParagraph(item.Quantity % 1 == 0 ? item.Quantity.ToString("0") : item.Quantity.ToString("0.##"));
                row.Cells[3].AddParagraph(item.UnitPrice.ToString("C2"));
                row.Cells[4].AddParagraph(item.Subtotal.ToString("C2"));
            }
        }

        private void AddTotal(Section section, Quote quote)
        {
            var paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.5);
            paragraph.Format.Alignment = ParagraphAlignment.Right;
            paragraph.Style = "TotalLabel";
            paragraph.AddText($"Total: {quote.Total:C2}");
        }

        private void AddNotes(Section section, Quote quote)
        {
            if (string.IsNullOrWhiteSpace(quote.Notes))
                return;

            var paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(1);
            paragraph.Format.Borders.Top.Width = 0.5;
            paragraph.Format.Borders.Top.Color = Colors.LightGray;
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.3);

            paragraph = section.AddParagraph();
            paragraph.AddFormattedText("Notas: ", "Label");
            paragraph.AddText(quote.Notes);
        }

        private void AddFooter(Section section)
        {
            var paragraph = section.AddParagraph();
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(2);
            paragraph.Style = "Footer";
            paragraph.Format.Alignment = ParagraphAlignment.Right;
            paragraph.AddText($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}");
        }
    }
}
