using Unit = QuestPDF.Infrastructure.Unit;

namespace StoreManagement.Application.Features.Invoices.Queries
{
    public class GenerateInvoicePdfQuery : IRequest<byte[]>
    {
        public PrintInvoiceDto InvoiceData { get; set; }
    }

    public class GenerateInvoicePdfQueryHandler : IRequestHandler<GenerateInvoicePdfQuery, byte[]>
    {
        public Task<byte[]> Handle(GenerateInvoicePdfQuery request, CancellationToken cancellationToken)
        {
            
            if (File.Exists("Vazirmatn-Regular.ttf"))
            {
                FontManager.RegisterFont(File.OpenRead("Vazirmatn-Regular.ttf"));
            }
            else if (File.Exists("C:/Windows/Fonts/tahoma.ttf"))
            {
                FontManager.RegisterFont(File.OpenRead("C:/Windows/Fonts/tahoma.ttf"));
            }


            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(20, Unit.Point);
                    page.PageColor(Colors.White); // تضمین پس‌زمینه سفید
                    page.DefaultTextStyle(style => style.FontFamily("Vazirmatn").FontSize(9).FontColor(Colors.Black));

                    page.Header()
                        .ContentFromRightToLeft()
                        .PaddingBottom(0.5f, Unit.Centimetre)
                        .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Row(row =>
                        {
                            row.ConstantItem(100).AlignCenter().Column(column =>
                            {
                                //column.Item().Image(File.OpenRead("Assets/Images/logo.png"), ImageScaling.FitArea);
                                column.Item().Text(request.InvoiceData.SellerName).Bold().FontSize(8);
                                column.Item().Text(request.InvoiceData.SellerAddress).FontSize(8);
                                column.Item().Text(request.InvoiceData.SellerPhone).FontSize(8);
                            });
                            row.Spacing(.5f, Unit.Centimetre);
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text("فاکتور فروش").FontSize(14);
                            });
                            row.Spacing(.5f, Unit.Centimetre);
                            row.RelativeItem().AlignLeft().Column(column =>
                            {
                                column.Item().Text($"شماره: {request.InvoiceData.InvoiceNumber}").AlignLeft();
                                column.Item().Text($"تاریخ: {request.InvoiceData.InvoiceDate:yyyy/MM/dd}").AlignLeft();
                            });
                        });

                    page.Content()
                        .ContentFromRightToLeft()
                        .PaddingVertical(0.5f, Unit.Centimetre)
                        .Column(col =>
                        {
                            col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(column =>
                            {
                                column.Item().Text("مشخصات خریدار").Bold();
                                column.Item().PaddingTop(5).Text($"نام و نشان : {request.InvoiceData.BuyerName} | شماره تماس : {request.InvoiceData.BuyerPhone} | آدرس : {request.InvoiceData.BuyerAddress}");
                            });
                            
                            col.Item().PaddingTop(10);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(0.5f);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("ردیف");
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("شرح کالا");
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("تعداد");
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("فی واحد");
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("% تخفیف");
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("% مالیات");
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("مبلغ کل");
                                });

                                foreach (var item in request.InvoiceData.Items)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.RowNumber.ToString());
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.ProductName);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Quantity.ToString());
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text($"{item.UnitPrice:N0}");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text($"{item.Discount:N0}");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text($"{item.Tax:N0}");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text($"{item.TotalPrice:N0}");
                                }
                            });
                        });

                    page.Footer()
                        .ContentFromRightToLeft()
                        .BorderTop(1).BorderColor(Colors.Grey.Lighten2)
                        .PaddingTop(10)
                        .Column(col =>
                        {
                            col.Item().AlignRight().Row(row =>
                            {
                                row.RelativeItem();
                                row.ConstantItem(200).Column(column =>
                                {
                                    column.Item().Row(r => { r.RelativeItem().Text("جمع کل:"); r.ConstantItem(100).AlignLeft().Text($"{request.InvoiceData.SubTotal:N0} ریال"); });
                                    column.Item().Row(r => { r.RelativeItem().Text("تخفیف:"); r.ConstantItem(100).AlignLeft().Text($"{request.InvoiceData.TotalDiscount:N0} ریال"); });
                                    column.Item().Row(r => { r.RelativeItem().Text("مبلغ نهایی:").Bold(); r.ConstantItem(100).AlignLeft().Text($"{request.InvoiceData.GrandTotal:N0} ریال").Bold(); });
                                });
                            });
                            
                            col.Item().PaddingTop(5).Text($"به حروف: {request.InvoiceData.GrandTotalInWords}");
                            
                            col.Item().PaddingTop(50).Row(row =>
                            {
                                row.RelativeItem().Column(column => {
                                    column.Item().AlignCenter().Text("امضاء خریدار");
                                    column.Item().PaddingTop(20).BorderTop(1).BorderColor(Colors.Grey.Lighten2);
                                });
                                row.ConstantItem(100);
                                row.RelativeItem().Column(column => {
                                    column.Item().AlignCenter().Text("امضاء فروشنده");
                                    column.Item().PaddingTop(20).BorderTop(1).BorderColor(Colors.Grey.Lighten2);
                                });
                            });
                        });
                });
            });
            
            return Task.FromResult(document.GeneratePdf());
        }
    }
}