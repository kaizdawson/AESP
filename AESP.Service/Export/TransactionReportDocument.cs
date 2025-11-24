using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Export
{
    public class TransactionReportDocument : IDocument
    {
        public List<TransactionReportItem> Items { get; set; }
        public string GeneratedAt { get; set; }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);

                // ---------------- HEADER ----------------
                page.Header().Row(row =>
                {
                    row.RelativeColumn().Column(col =>
                    {
                        col.Item().Text("AESP - Transaction Report")
                            .FontSize(24).SemiBold().FontColor(Colors.Blue.Darken2);

                        col.Item().Text($"Generated at: {GeneratedAt}")
                            .FontSize(10).FontColor(Colors.Grey.Darken1);
                    });
                });

                // ---------------- TABLE ----------------
                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(col =>
                    {
                        col.RelativeColumn(2);   // Transaction ID
                        col.RelativeColumn(2);   // UserName
                        col.RelativeColumn(1.2f);// Type
                        col.RelativeColumn(1.2f);// Money
                        col.RelativeColumn(1);   // Coin
                        col.RelativeColumn(1);   // Status
                        col.RelativeColumn(1.5f);// Created
                    });

                    // ----- HEADER -----
                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderStyle).Text("Transaction ID");
                        header.Cell().Element(HeaderStyle).Text("User Name");
                        header.Cell().Element(HeaderStyle).Text("Type");
                        header.Cell().Element(HeaderStyle).Text("Money");
                        header.Cell().Element(HeaderStyle).Text("Coin");
                        header.Cell().Element(HeaderStyle).Text("Status");
                        header.Cell().Element(HeaderStyle).Text("Created");
                    });

                    // ----- ROWS -----
                    bool isEven = false;

                    foreach (var item in Items)
                    {
                        var bg = isEven ? Colors.Grey.Lighten4 : Colors.White;
                        isEven = !isEven;

                        table.Cell().Element(x => CellStyle(x, bg)).Text(item.TransactionId);
                        table.Cell().Element(x => CellStyle(x, bg)).Text(item.UserName);
                        table.Cell().Element(x => CellStyle(x, bg)).Text(item.Type);
                        table.Cell().Element(x => CellStyle(x, bg)).Text($"{item.AmountMoney:N0} đ");
                        table.Cell().Element(x => CellStyle(x, bg)).Text($"{item.AmountCoin:N0}");
                        table.Cell().Element(x => CellStyle(x, bg)).Text(item.Status);
                        table.Cell().Element(x => CellStyle(x, bg)).Text(item.CreatedAt.ToString("dd/MM/yyyy HH:mm"));
                    }
                });

                // ---------------- FOOTER ----------------
                page.Footer().AlignCenter().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(10));
                    text.Span("AESP Report • ").FontColor(Colors.Grey.Darken1);
                    text.Span("© 2025").FontColor(Colors.Grey.Darken1);
                });
            });   // <-- đóng Page
        }


        // ----------- STYLE HELPERS --------------
        private IContainer HeaderStyle(IContainer container)
        {
            return container.Padding(5)
                .Background(Colors.Grey.Lighten2)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Darken1)
                .DefaultTextStyle(x => x.SemiBold());
        }

        private IContainer CellStyle(IContainer container, string bg)
        {
            return container.Padding(5)
                .Background(bg)
                .BorderBottom(0.5f)
                .BorderColor(Colors.Grey.Lighten3);
        }
    }
    
       
        public class TransactionReportItem
        {
            public string TransactionId { get; set; }
            public string UserName { get; set; }
            public string Type { get; set; }
            public decimal AmountMoney { get; set; }
            public decimal AmountCoin { get; set; }
            public string Status { get; set; }
            public string OrderCode { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }


