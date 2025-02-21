using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SplittingScript.PractiseDiaryKNU;

namespace SplittingScript.PractiseDocuments.PractiseReport
{
    public class PractiseReport : IPractiseDocument
    {
        public readonly bool IsEmpty;
        public readonly string University = string.Empty;
        public readonly string Faculty = string.Empty;
        public readonly string Department = string.Empty;
        public readonly string Title = string.Empty;
        public readonly string Course = string.Empty;
        public readonly string Group = string.Empty;
        public readonly string StudentName = string.Empty;
        public readonly string Content = string.Empty;
        public readonly string Date = string.Empty;
        public readonly bool IsSigned = false;

        public PractiseReport(string inputFilePath)
        {
            using (WordprocessingDocument doc = WordprocessingDocument.Open(inputFilePath, false))
            {
                Body body = doc.MainDocumentPart.Document.Body;
                List<OpenXmlElement> elements = body.Elements<OpenXmlElement>().ToList();

                if (elements == null || elements.Count == 0)
                {
                    IsEmpty = true;
                    return;
                }

                IsEmpty = false;
                string lastKey = "";
                List<string> contentParts = new List<string>();
                bool isExtractingContent = false;
                Dictionary<string, string> data = new Dictionary<string, string>();

                foreach (var element in elements)
                {
                    bool containsSignature = WordHelper.ContainsSignature(element);
                    bool IsBold = WordHelper.IsBoldText(element);
                    string text = WordHelper.RemoveImagesAndGetText(element).Trim();
                    if (string.IsNullOrWhiteSpace(text)) continue;

                    if (text.StartsWith("Київський національний університет"))
                        lastKey = "University";
                    else if (text.StartsWith("Факультет"))
                        lastKey = "Faculty";
                    else if (text.StartsWith("Кафедра"))
                        lastKey = "Department";
                    else if (text.StartsWith("Звіт"))
                        lastKey = "Title";
                    else if (text.StartsWith("студент"))
                        lastKey = "CourseGroup";
                    else if (text.StartsWith("Дата:"))
                        lastKey = "Date";
                    else if (lastKey == "CourseGroup" && string.IsNullOrEmpty(data.GetValueOrDefault("StudentName")))
                        lastKey = "StudentName";
                    else if (lastKey == "StudentName")
                        lastKey = "Content";
                    if (containsSignature)
                        IsSigned = true;

                    if (lastKey == "Content" && !text.StartsWith("Дата:"))
                    {
                        isExtractingContent = true;
                        contentParts.Add(text);
                        continue;
                    }
                    else if (text.StartsWith("Дата:"))
                    {
                        isExtractingContent = false;
                        lastKey = "Date";
                    }

                    if (!string.IsNullOrEmpty(lastKey) && !isExtractingContent)
                    {
                        if (!data.ContainsKey(lastKey))
                            data[lastKey] = text;
                        else
                            data[lastKey] += " " + text;
                    }
                }

                // Обробка курс/група
                if (data.TryGetValue("CourseGroup", out string courseGroupText))
                {
                    var parts = courseGroupText.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        Course = parts[0].Replace("студентки", "").Replace("студента", "").Replace("курсу", "").Trim();
                        Group = parts[1].Replace("група", "").Replace("групи", "").Trim();
                    }
                }

                // Обробка ПІБ (три слова після `курс` та `група`)
                if (data.TryGetValue("StudentName", out string studentNameText))
                {
                    var nameParts = studentNameText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (nameParts.Length > 0)
                    {
                        StudentName = string.Join(" ", nameParts[0..Math.Min(nameParts.Length, 3)]);
                    }
                }

                University = data.GetValueOrDefault("University") ?? string.Empty;
                Faculty = data.GetValueOrDefault("Faculty") ?? string.Empty;
                Department = data.GetValueOrDefault("Department") ?? string.Empty;
                Title = data.GetValueOrDefault("Title") ?? string.Empty;
                Content = string.Join(" ", contentParts).Trim();
                Date = data.GetValueOrDefault("Date")?.Replace("Дата:", "").Replace("Підпис:", "").Trim() ?? string.Empty;
            }
        }

        public override string ToString()
        {
            if (IsEmpty)
                return "Інформація про звіт відсутня.";

            return $"{Title.ToUpper()}:\n" +
                $"Університет: {University}\n" +
                $"Факультет: {Faculty}\n" +
                $"Кафедра: {Department}\n" +
                $"Курс: {Course}\n" +
                $"Група: {Group}\n" +
                $"ПІБ: {StudentName}\n" +
                $"Дата: {Date}\n" +
                $"Підпис: {(IsSigned ? "Є" : "Немає")}\n" +
                $"Зміст:{Content}";
        }

        public string PrintParsedContent()
        {
            return this.ToString();
        }
    }
}