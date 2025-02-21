using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Office.CoverPageProps;
using SplittingScript;

namespace PractiseDiaryKNU
{
    public class TitlePage
    {
        public readonly bool IsEmpty;
        public readonly string University = string.Empty;
        public readonly string Faculty = string.Empty;
        public readonly string Department = string.Empty;
        public readonly string StudentName = string.Empty;
        public readonly string Course = string.Empty;
        public readonly string EducationalLevel = string.Empty;
        public readonly string Program = string.Empty;
        public readonly string Title = string.Empty;

        public TitlePage(List<OpenXmlElement> page)
        {
            if (page == null || page.Count == 0)
            {
                IsEmpty = true;
                return;
            }

            IsEmpty = false;
            Dictionary<string, string> data = new Dictionary<string, string>();
            string lastKey = "";

            foreach (var element in page)
            {
                 bool containsSignature = WordHelper.ContainsSignature(element);
                string text = WordHelper.RemoveImagesAndGetText(element).Trim();
                if (string.IsNullOrWhiteSpace(text)) continue;

                if (text.ToLower().StartsWith("київський національний університет"))
                    lastKey = "University";
                else if (text.ToLower().StartsWith("факультет"))
                    lastKey = "Faculty";
                else if (text.ToLower().StartsWith("кафедра") || text.ToLower().StartsWith("катедра"))
                    lastKey = "Department";
                else if (text.ToLower().StartsWith("студент"))
                    lastKey = "StudentName";
                else if (text.ToLower().Contains("курсу"))
                    lastKey = "Course";
                else if (text.ToLower().Contains("освітньо-кваліфікаційний рівень"))
                    lastKey = "EducationalLevel";
                else if (text.ToLower().Contains("напрям підготовки"))
                    lastKey = "Program";
                else if (text.ToLower().StartsWith("щоденник"))
                    lastKey = "Title";

                if (!string.IsNullOrEmpty(lastKey))
                {
                    if (!data.ContainsKey(lastKey))
                        data[lastKey] = text;
                    else
                        data[lastKey] += " " + text;
                }
            }

            University = data.GetValueOrDefault("University") ?? string.Empty;
            Faculty = data.GetValueOrDefault("Faculty") ?? string.Empty;
            Department = data.GetValueOrDefault("Department") ?? string.Empty;
            StudentName = data.GetValueOrDefault("StudentName")?.Replace("студента", "").Replace("студентки", "").Replace("студент(а/ки)", "").Trim() ?? string.Empty;
            Course = data.GetValueOrDefault("Course") ?? string.Empty;
            EducationalLevel = data.GetValueOrDefault("EducationalLevel")?.Replace("освітньо-кваліфікаційний рівень", "").Replace("«", "").Replace("»", "").Trim() ?? string.Empty;
            Program = data.GetValueOrDefault("Program")?.Replace("напрям підготовки", "").Replace("«", "").Replace("»", "").Trim() ?? string.Empty;
            Title = data.GetValueOrDefault("Title") ?? string.Empty;
        }

        public override string ToString()
        {
            if (IsEmpty)
                return "Інформація про титульну сторінку відсутня.";

            return "*Титульна сторінка:*\n".ToUpper()+
                $"*Університет:* {University}\n" +
                $"*Факультет:* {Faculty}\n" +
                $"*Кафедра:* {Department}\n" +
                $"*Студент:* {StudentName}\n" +
                $"*Курс:* {Course}\n" +
                $"*Рівень освіти:* {EducationalLevel}\n" +
                $"*Програма:* {Program}\n";
        }
    }
}