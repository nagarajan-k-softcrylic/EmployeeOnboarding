using EmployeeOnboarding.Function.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EmployeeOnboarding.Function.Services;

public class WelcomeLetterGenerator : IWelcomeLetterGenerator
{
    public WelcomeLetterGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(EmployeeCreatedMessage employee)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header().Text("Welcome to the Company!").FontSize(20).Bold();

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                {
                    column.Spacing(10);
                    column.Item().Text($"Dear {employee.Name},");
                    column.Item().Text(
                        $"Congratulations and welcome aboard! We are excited to have you join the {employee.Department} department " +
                        $"as part of our team (Employee Code: {employee.EmployeeCode}).");
                    column.Item().Text(
                        "Your onboarding process has been initiated. Please reach out to HR for any questions regarding your " +
                        "start date, documentation, or benefits enrollment.");
                    column.Item().Text($"We look forward to working with you, {employee.Name}!");
                    column.Item().PaddingTop(20).Text("Best regards,");
                    column.Item().Text("HR Team");
                });

                page.Footer().AlignCenter().Text($"Generated on {DateTime.UtcNow:yyyy-MM-dd}");
            });
        });

        return document.GeneratePdf();
    }
}
