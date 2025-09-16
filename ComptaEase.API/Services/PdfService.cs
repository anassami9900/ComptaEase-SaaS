using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ComptaEase.API.Models;

namespace ComptaEase.API.Services;

public class PdfService : IPdfService
{
    private readonly IWebHostEnvironment _environment;

    public PdfService(IWebHostEnvironment environment)
    {
        _environment = environment;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<string> GeneratePayslipAsync(Payroll payroll, Employee employee, Company company)
    {
        var fileName = $"bulletin_{employee.EmployeeNumber}_{payroll.PayrollPeriodYear}_{payroll.PayrollPeriodMonth:D2}.pdf";
        var bulletinsDir = Path.Combine(_environment.WebRootPath, "bulletins");
        
        if (!Directory.Exists(bulletinsDir))
            Directory.CreateDirectory(bulletinsDir);

        var filePath = Path.Combine(bulletinsDir, fileName);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Text($"Bulletin de Paie - {payroll.PayrollPeriodMonth:D2}/{payroll.PayrollPeriodYear}")
                    .SemiBold().FontSize(16).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        // Company Information
                        column.Item().ShowEntire().Column(col =>
                        {
                            col.Item().Text("INFORMATIONS ENTREPRISE").SemiBold().FontSize(12);
                            col.Item().LineHorizontal(1);
                            col.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text($"Entreprise: {company.Name}");
                                    if (!string.IsNullOrEmpty(company.Address))
                                        c.Item().Text($"Adresse: {company.Address}");
                                    if (!string.IsNullOrEmpty(company.Phone))
                                        c.Item().Text($"Téléphone: {company.Phone}");
                                });
                                row.RelativeItem().Column(c =>
                                {
                                    if (!string.IsNullOrEmpty(company.Email))
                                        c.Item().Text($"Email: {company.Email}");
                                    if (!string.IsNullOrEmpty(company.TaxId))
                                        c.Item().Text($"ID Fiscal: {company.TaxId}");
                                });
                            });
                        });

                        column.Item().PaddingTop(20);

                        // Employee Information
                        column.Item().ShowEntire().Column(col =>
                        {
                            col.Item().Text("INFORMATIONS EMPLOYE").SemiBold().FontSize(12);
                            col.Item().LineHorizontal(1);
                            col.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text($"Nom: {employee.FirstName} {employee.LastName}");
                                    c.Item().Text($"Matricule: {employee.EmployeeNumber}");
                                    c.Item().Text($"Poste: {employee.Position}");
                                });
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text($"Département: {employee.Department}");
                                    c.Item().Text($"Date d'embauche: {employee.HireDate:dd/MM/yyyy}");
                                    if (!string.IsNullOrEmpty(employee.CnssNumber))
                                        c.Item().Text($"CNSS: {employee.CnssNumber}");
                                });
                            });
                        });

                        column.Item().PaddingTop(20);

                        // Payroll Details
                        column.Item().ShowEntire().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("ELEMENTS DE PAIE").SemiBold();
                                header.Cell().Element(CellStyle).Text("MONTANT (MAD)").SemiBold();

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                }
                            });

                            // Earnings
                            table.Cell().Element(CellStyle).Text("Salaire de base");
                            table.Cell().Element(CellStyle).Text($"{employee.BaseSalary:F2}");

                            if (payroll.TotalAllowances > 0)
                            {
                                table.Cell().Element(CellStyle).Text("Primes et indemnités");
                                table.Cell().Element(CellStyle).Text($"{payroll.TotalAllowances:F2}");
                            }

                            table.Cell().Element(CellStyle).Text("SALAIRE BRUT").SemiBold();
                            table.Cell().Element(CellStyle).Text($"{payroll.GrossSalary:F2}").SemiBold();

                            // Deductions
                            table.Cell().Element(CellStyle).Text("CNSS Employé");
                            table.Cell().Element(CellStyle).Text($"-{payroll.CnssEmployee:F2}");

                            table.Cell().Element(CellStyle).Text("AMO Employé");
                            table.Cell().Element(CellStyle).Text($"-{payroll.AmoEmployee:F2}");

                            table.Cell().Element(CellStyle).Text("IGR");
                            table.Cell().Element(CellStyle).Text($"-{payroll.IgrTax:F2}");

                            if (payroll.OtherDeductions > 0)
                            {
                                table.Cell().Element(CellStyle).Text("Autres retenues");
                                table.Cell().Element(CellStyle).Text($"-{payroll.OtherDeductions:F2}");
                            }

                            table.Cell().Element(CellStyle).Text("TOTAL RETENUES").SemiBold();
                            table.Cell().Element(CellStyle).Text($"-{payroll.TotalDeductions:F2}").SemiBold();

                            table.Cell().Element(CellStyle).Text("SALAIRE NET").SemiBold().FontSize(12);
                            table.Cell().Element(CellStyle).Text($"{payroll.NetSalary:F2}").SemiBold().FontSize(12);

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                            }
                        });

                        column.Item().PaddingTop(20);

                        // Additional Information
                        column.Item().ShowEntire().Column(col =>
                        {
                            col.Item().Text("INFORMATIONS COMPLEMENTAIRES").SemiBold().FontSize(12);
                            col.Item().LineHorizontal(1);
                            col.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text($"Jours travaillés: {payroll.WorkedDays}");
                                    c.Item().Text($"Période: {payroll.PayrollPeriodMonth:D2}/{payroll.PayrollPeriodYear}");
                                });
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text($"Date de paiement: {payroll.PaymentDate:dd/MM/yyyy}");
                                    c.Item().Text($"Statut: {payroll.Status}");
                                });
                            });
                        });
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Généré le ");
                        x.Span($"{DateTime.Now:dd/MM/yyyy HH:mm}").SemiBold();
                        x.Span(" par ComptaEase SaaS");
                    });
            });
        });

        await Task.Run(() => document.GeneratePdf(filePath));
        return filePath;
    }
}