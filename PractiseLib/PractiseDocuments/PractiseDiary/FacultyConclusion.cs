using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using SplittingScript;

namespace PractiseDiaryKNU
{
    public class FacultyConclusion
    {
        public bool IsEmpty = true;
        public readonly string Conclusion = string.Empty;
        public readonly string Date = string.Empty;
        public readonly string Mark = string.Empty;

        public readonly bool HasFacultyOverseerSignature = false;
        public readonly bool HasAllOverseersSignatures = false;

        public FacultyConclusion(List<OpenXmlElement>page)
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

                if (text.ToLower().StartsWith("залікова"))
                    lastKey = "Mark";
                else if(text.ToLower().EndsWith("року"))
                    lastKey = "Date";
                else if(text.ToLower().StartsWith("на основі") || element is Table || element.Descendants<Table>().Any())
                    lastKey = "Conclusion";
                else if(text.ToLower().Contains("підписи членів"))
                    lastKey = "OverseersSign";
                else if(text.ToLower().Contains("підпис керівника"))
                    lastKey = "FacultyOverseerSign";

                if(containsSignature)
                {
                    if(lastKey == "OverseersSign") HasAllOverseersSignatures = true;
                    if(lastKey == "FacultyOverseerSign") HasFacultyOverseerSignature = true;
                }

                if (!string.IsNullOrEmpty(lastKey))
                {
                    if (!data.ContainsKey(lastKey))
                        data[lastKey] = text;
                    else
                        data[lastKey] += " " + text;
                }
            }
            Conclusion = data.GetValueOrDefault("Conclusion")?.Trim() ?? string.Empty;
            Mark = data.GetValueOrDefault("Mark")?.Replace("Залікова оцінка з практики:", "").Trim() ?? string.Empty;
            Date = data.GetValueOrDefault("Date")?.Trim() ?? string.Empty;
        }

        public override string ToString()
        {
            if (IsEmpty)
                return "Інформація про характеристику й оцінку роботи відсутня.";

            return "*Висновок керівника практики від факультету про роботу студента:*\n".ToUpper()+
                $"*Висновок:* {Conclusion}\n" +
                $"*Оцінка:* {Mark}\n" +
                $"*Дата:* {Date}\n"+
                $"*Підпис керівника практики від факультету:* {(HasFacultyOverseerSignature ? "Так" : "Ні")}\n" +
                $"*Підписи членів комісії:* {(HasAllOverseersSignatures ? "Так" : "Ні")}\n";
        }
    }
}