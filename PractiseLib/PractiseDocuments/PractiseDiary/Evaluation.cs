using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using SplittingScript;

namespace PractiseDiaryKNU
{
    public class Evaluation
    {
        public readonly bool IsEmpty = true;
        public readonly string Characteristics = string.Empty;
        public readonly string Mark = string.Empty;
        public readonly string Date = string.Empty;
        public readonly string StudentName = string.Empty;
        public readonly bool HasOverseerSignature = false;

        public Evaluation(List<OpenXmlElement>page)
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

                // if (string.IsNullOrWhiteSpace(text)) continue;

                if (text.ToLower().StartsWith("оцінка з практики:"))
                    lastKey = "Mark";
                else if(text.ToLower().EndsWith("року"))
                    lastKey = "Date";
                else if(text.ToLower().StartsWith("студент"))
                    lastKey = "Characteristics";
                else if(text.ToLower().StartsWith("підпис"))
                    lastKey = "Sign";

                if(containsSignature)
                {
                    HasOverseerSignature = true;
                }

                if (!string.IsNullOrEmpty(lastKey))
                {
                    if (!data.ContainsKey(lastKey))
                        data[lastKey] = text;
                    else
                        data[lastKey] += " " + text;
                }
            }
            Characteristics = data.GetValueOrDefault("Characteristics")?.Trim() ?? string.Empty;
            Mark = data.GetValueOrDefault("Mark")?.Replace("Оцінка з практики:", "").Replace("_", "").Trim() ?? string.Empty;
            Date = data.GetValueOrDefault("Date")?.Trim() ?? string.Empty;
            StudentName = ExtractName(Characteristics) ?? string.Empty;
        }

        public static string? ExtractName(string text)
        {
            // Шаблони для різних форматів ПІБ
            string[] patterns = new[]
            {
                // Повне ПІБ (три слова)
                @"Студент,\s+([А-ЯІЇЄ][а-яіїє']+(?:-[А-ЯІЇЄ][а-яіїє']+)?\s+[А-ЯІЇЄ][а-яіїє']+(?:-[А-ЯІЇЄ][а-яіїє']+)?\s+[А-ЯІЇЄ][а-яіїє']+(?:-[А-ЯІЇЄ][а-яіїє']+)?),\s+за\s+час",
                
                // ПІБ з ініціалами (Прізвище І.Б.)
                @"Студент,\s+([А-ЯІЇЄ][а-яіїє']+(?:-[А-ЯІЇЄ][а-яіїє']+)?\s+[А-ЯІЇЄ]\.\s*[А-ЯІЇЄ]\.),\s+за\s+час",
                
                // Тільки прізвище та ім'я
                @"Студент,\s+([А-ЯІЇЄ][а-яіїє']+(?:-[А-ЯІЇЄ][а-яіїє']+)?\s+[А-ЯІЇЄ][а-яіїє']+(?:-[А-ЯІЇЄ][а-яіїє']+)?),\s+за\s+час",
                
                // Тільки прізвище
                @"Студент,\s+([А-ЯІЇЄ][а-яіїє']+(?:-[А-ЯІЇЄ][а-яіїє']+)?),\s+за\s+час"
            };

            foreach (string pattern in patterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups[1].Value.Trim();
                }
            }

            return null; // Якщо ПІБ не знайдено
        }

        public override string ToString()
        {
            if (IsEmpty)
                return "Інформація про характеристику й оцінку роботи відсутня.";

            return "*Характеристика й оцінка роботи студента на практиці:*\n".ToUpper() +
                $"*Характеристика:* {Characteristics}\n" +
                $"*ПІБ:* {StudentName}\n" +
                $"*Оцінка:* {Mark}\n" +
                $"*Дата:* {Date}\n"+
                $"*Підпис:* {(HasOverseerSignature ? "Так": "Ні")}\n";
        }
    }
}