using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Export
{
    public class PurchaseReportDocument : IDocument
    {
        public List<PurchaseReportItem> Items { get; set; }
        public string GeneratedAt { get; set; } = "";

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(20);

                page.Header().Text("Purchase Items Management Report")
                    .FontSize(20).SemiBold().AlignCenter();

                page.Content().Column(col =>
                {
                    col.Item().Text($"Generated on: {GeneratedAt}").FontSize(11);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(100);  // PurchaseId
                            columns.ConstantColumn(100);  // UserID
                            columns.RelativeColumn();     // ItemType
                            columns.RelativeColumn();     // ItemName
                            columns.ConstantColumn(80);   // Amount
                            columns.ConstantColumn(120);  // CreatedAt
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Text("Purchase ID").SemiBold();
                            header.Cell().Text("User ID").SemiBold();
                            header.Cell().Text("Item Type").SemiBold();
                            header.Cell().Text("Item Name").SemiBold();
                            header.Cell().Text("Coin").SemiBold();
                            header.Cell().Text("Created At").SemiBold();
                        });

                        // Data
                        foreach (var row in Items)
                        {
                            table.Cell().Text(row.PurchaseId);
                            table.Cell().Text(row.UserId);
                            table.Cell().Text(row.ItemType);
                            table.Cell().Text(row.ItemName);
                            table.Cell().Text($"{row.AmountCoin} coin");
                            table.Cell().Text(row.CreatedAt.ToString("dd/MM/yyyy HH:mm"));
                        }
                    });
                });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Page ").FontSize(10);
                        text.CurrentPageNumber().FontSize(10);
                    });
            });
        }
    }

    public class PurchaseReportItem
    {
        public string PurchaseId { get; set; }
        public string UserId { get; set; }
        public string ItemType { get; set; }
        public string ItemName { get; set; }
        public decimal AmountCoin { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
