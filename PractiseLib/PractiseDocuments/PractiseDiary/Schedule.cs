using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using SplittingScript;

namespace PractiseDiaryKNU
{
    public class Schedule
    {
        public readonly bool IsEmpty;
        public readonly List<(string Number, string Name, string BeginDate, string EndDate, string Annotations)> Rows = new List<(string, string, string, string, string)>();

        public readonly bool HasFacultyOverseerSignature = false;
        public readonly bool HasPractiseOverseerSignature = false;

        public Schedule(List<OpenXmlElement> page)
        {
            if (page == null || page.Count == 0)
            {
                IsEmpty = true;
                return;
            }

            IsEmpty = false;
            Table table = page.OfType<Table>().FirstOrDefault();

            if (table != null)
            {
                foreach (var row in table.Elements<TableRow>().Skip(2)) // Пропускаємо заголовки
                {
                    var cells = row.Elements<TableCell>().ToList();
                    if (cells.Count >= 5)
                    {
                        string number = cells[0].InnerText.Trim('.').Trim();
                        string name = cells[1].InnerText.Trim();
                        string beginDate = cells[2].InnerText.Trim();
                        string endDate = cells[3].InnerText.Trim();
                        string annotations = cells[4].InnerText.Trim();
                        Rows.Add((number, name, beginDate, endDate, annotations));
                    }
                }
            }
            Dictionary<string, string> data = new Dictionary<string, string>();
            var lastKey = string.Empty;
            bool signArea = false;
            foreach(var element in page)
            {
                if(element is Table)
                {
                    continue;
                }

                bool containsSignature = WordHelper.ContainsSignature(element);
                string text = WordHelper.RemoveImagesAndGetText(element).Trim();

                if(text.ToLower().StartsWith("підпис"))
                {
                    signArea = true;
                }

                if(text.ToLower().Contains("кафедри") || text.ToLower().Contains("підприємства") || text.ToLower().Contains("катедри"))
                {
                    lastKey = "PractiseOverseer";
                    if(signArea &&containsSignature) HasPractiseOverseerSignature = true;
                }
                if(text.ToLower().Contains("факультету"))
                {
                    lastKey = "FacultyOverseer";
                    if(signArea &&containsSignature) HasFacultyOverseerSignature = true;
                }

                if (signArea && containsSignature)
                {
                    if (lastKey == "PractiseOverseer") HasPractiseOverseerSignature = true;
                    if (lastKey == "FacultyOverseer") HasFacultyOverseerSignature = true;
                }
            }
        }

        public override string ToString()
        {
            if (IsEmpty)
                return "Інформація про календарний графік відсутня.";

            string result = "*Календарний графік проходження практики:*\n".ToUpper();
            foreach (var row in Rows)
            {
                result += $"№ {row.Number}: {row.Name} ({row.BeginDate} - {row.EndDate}) Примітки: {row.Annotations}\n";
            }
            result += "*Підписи керівників практики:* \n" +
                    $"_від кафедри/підприємства:_ {(HasPractiseOverseerSignature ? "Так" : "Ні")} \n" +
                    $"_від факультету:_ {(HasFacultyOverseerSignature ? "Так" : "Ні")}\n";
            return result;
        }
    }
}