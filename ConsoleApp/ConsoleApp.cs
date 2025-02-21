using SplittingScript;
using PractiseDiaryKNU;
using DocumentFormat.OpenXml.Office2010.CustomUI;
using System.Runtime.CompilerServices;
using DocumentFunctions;
using SplittingScript.Checking;
using SplittingScript.PractiseDocuments.PractiseReport;
using SplittingScript.PractiseDiaryKNU;

public static class ConsoleApp
{
    public static void Main()
    {
        var checker = new DocumentChecker();

        while (true)
        {
            Console.WriteLine("Enter your command: ");
            string input = Console.ReadLine()?.Trim() ?? string.Empty;

            if (input.ToLower() == "exit") break;
            
            var parts = input.Split(" ", 2);
            if (parts.Length < 2) continue;

            string command = parts[0].ToLower();
            string filePath = parts[1];

            var document = LoadDocument(filePath);
            if (document == null) continue;

            try
            {
                switch (command)
                {
                    case "analyze":
                        Console.WriteLine(document.PrintParsedContent());
                        break;

                    case "check":
                        checker.Checker = GetCheckerForDocument(document);
                        Console.WriteLine(checker.CheckAsync(document));
                        break;

                    default:
                        Console.WriteLine("Unknown command.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred:\n{ex.Message}");
            }
        }
    }

    private static IPractiseDocument? LoadDocument(string filePath)
    {
        try
        {
            var report = new PractiseReport(filePath);
            if (report.Title.ToLower().Contains("звіт"))
                return report;

            var diary = new PractiseDiary(filePath);
            if (diary.TitlePage.Title.ToLower().Contains("щоденник"))
                return diary;

            throw new Exception("Unable to determine document type.");
        }
        catch
        {
            Console.WriteLine("Unable to process document");
            return null;
        }
    }

    private static ICheckingStrategy GetCheckerForDocument(IPractiseDocument document)
    {
        return document switch
        {
            PractiseDiary => new PractiseDiaryChecker(),
            PractiseReport => new PractiseReportChecker(),
            _ => throw new InvalidOperationException("Unsupported document type")
        };
    }
}
