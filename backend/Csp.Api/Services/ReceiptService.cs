using System.Reflection;
using Csp.Api.DTOs;
using MySql.Data.MySqlClient;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Csp.Api.Services;

public class ReceiptService : IReceiptService
{
    private readonly string _connectionString;
    private readonly IConfiguration _configuration;

    public ReceiptService(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<PaymentReceiptDto?> GetReceiptDataAsync(int paymentId)
    {
        var sqlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Payments", "GetPaymentReceiptData.sql");
        var sql = await File.ReadAllTextAsync(sqlPath);

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@PaymentId", paymentId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new PaymentReceiptDto
            {
                PaymentId = reader.GetInt32(reader.GetOrdinal("PaymentId")),
                MemberId = reader.GetInt32(reader.GetOrdinal("MemberId")),
                MemberName = reader.GetString(reader.GetOrdinal("MemberName")),
                MemberEmail = reader.GetString(reader.GetOrdinal("MemberEmail")),
                BookTitle = reader.GetString(reader.GetOrdinal("BookTitle")),
                BookAuthor = reader.GetString(reader.GetOrdinal("BookAuthor")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                PaymentDate = reader.GetDateTime(reader.GetOrdinal("PaymentDate")),
                PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                RecordedByName = reader.IsDBNull(reader.GetOrdinal("RecordedByName")) 
                    ? "System" 
                    : reader.GetString(reader.GetOrdinal("RecordedByName")),
                TransactionId = reader.GetString(reader.GetOrdinal("TransactionId"))
            };
        }

        return null;
    }

    public Task<byte[]> GenerateReceiptPdfAsync(PaymentReceiptDto data)
    {
        // Configure QuestPDF license (Community license for open-source projects)
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(ComposeHeader);
                page.Content().Element(content => ComposeContent(content, data));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated on ");
                    text.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                });
            });
        });

        var pdfBytes = document.GeneratePdf();
        return Task.FromResult(pdfBytes);
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().BorderBottom(1).PaddingBottom(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Lector Library").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                    col.Item().Text("SLIIT Campus, Malabe, Sri Lanka").FontSize(10).FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(100).AlignRight().Text("PAYMENT RECEIPT").FontSize(14).Bold();
            });
        });
    }

    private void ComposeContent(IContainer container, PaymentReceiptDto data)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Spacing(15);

            // Transaction Details Section
            column.Item().Element(c => ComposeSection(c, "Transaction Details", new Dictionary<string, string>
            {
                { "Transaction ID", data.TransactionId },
                { "Receipt Number", $"RCP-{data.PaymentId:D6}" },
                { "Payment Date", data.PaymentDate.ToString("MMMM dd, yyyy HH:mm") },
                { "Payment Method", data.PaymentMethod }
            }));

            // Member Details Section
            column.Item().Element(c => ComposeSection(c, "Member Details", new Dictionary<string, string>
            {
                { "Member Name", data.MemberName },
                { "Member Email", data.MemberEmail },
                { "Member ID", data.MemberId.ToString() }
            }));

            // Book Details Section
            column.Item().Element(c => ComposeSection(c, "Book Details", new Dictionary<string, string>
            {
                { "Title", data.BookTitle },
                { "Author", data.BookAuthor }
            }));

            // Payment Details Section
            column.Item().Background(Colors.Grey.Lighten3).Padding(15).Column(col =>
            {
                col.Item().Text("Payment Summary").FontSize(14).Bold();
                col.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Text("Fine Amount Paid:").Bold();
                    row.ConstantItem(100).AlignRight().Text($"${data.Amount:F2}").FontSize(16).Bold().FontColor(Colors.Green.Darken2);
                });
            });

            // Footer Info
            column.Item().PaddingTop(20).Column(col =>
            {
                col.Item().Text($"Processed by: {data.RecordedByName}").FontSize(9).FontColor(Colors.Grey.Darken1);
                col.Item().PaddingTop(5).Text("Thank you for your payment!").FontSize(10).Italic();
            });
        });
    }

    private void ComposeSection(IContainer container, string title, Dictionary<string, string> fields)
    {
        container.Column(column =>
        {
            column.Item().Text(title).FontSize(12).Bold().FontColor(Colors.Blue.Darken1);
            column.Item().PaddingTop(5).PaddingLeft(10).Column(col =>
            {
                foreach (var field in fields)
                {
                    col.Item().PaddingVertical(3).Row(row =>
                    {
                        row.ConstantItem(120).Text($"{field.Key}:").FontColor(Colors.Grey.Darken2);
                        row.RelativeItem().Text(field.Value);
                    });
                }
            });
        });
    }
}
