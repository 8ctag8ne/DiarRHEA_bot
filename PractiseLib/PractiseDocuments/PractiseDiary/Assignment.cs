using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using SplittingScript;

namespace PractiseDiaryKNU
{
    public class Assignment
    {
        public readonly bool IsEmpty;
        public readonly string StudentName = string.Empty;
        public readonly string PlaceOfPractise = string.Empty;
        public readonly string BeginDate = string.Empty;
        public readonly string EndDate = string.Empty;
        public readonly string Prescript = string.Empty;
        public readonly string FacultyOverseer = string.Empty;
        public readonly string PractiseOverseer = string.Empty;
        public readonly string Dean = string.Empty;
        public readonly List<string> Overseers = new List<string>();
        public readonly bool HasPractiseOverseerSignature = false;
        public readonly bool HasFacultyOverseerSignature = false;
        public readonly bool HasDeanSignature = false;

        public Assignment(List<OpenXmlElement> page)
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
                bool overseers = false;

                if (containsSignature)
                {
                    if (lastKey == "PractiseOverseer") HasPractiseOverseerSignature = true;
                    if (lastKey == "FacultyOverseer") HasFacultyOverseerSignature = true;
                    if (lastKey == "Dean") HasDeanSignature = true;
                }

                if (string.IsNullOrWhiteSpace(text)) continue;

                if (text.ToLower().StartsWith("студент"))
                    lastKey = "StudentName";
                else if (text.ToLower().StartsWith("направляється на виробничу практику"))
                    lastKey = "PlaceOfPractise";
                else if (text.ToLower().StartsWith("термін практики"))
                    lastKey = "Dates";
                else if (text.ToLower().StartsWith("наказ №"))
                    lastKey = "Prescript";
                else if (text.ToLower().StartsWith("керівник практики від катедри") || text.ToLower().StartsWith("керівник практики від кафедри"))
                    lastKey = "PractiseOverseer";
                else if (text.ToLower().StartsWith("керівник практики від факультету"))
                    lastKey = "FacultyOverseer";
                else if (text.ToLower().Contains("декан факультету"))
                    lastKey = "Dean";
                if (text.ToLower().Contains("доц.") || text.ToLower().Contains("ас."))
                    overseers = true;

                if (!string.IsNullOrEmpty(lastKey))
                {
                    if (overseers)
                        Overseers.Add(text.Replace("Керівник практики від ", "").Replace("кафедри", "").Replace("катедри", "").Replace("підприємства", "").Replace("/", "").Replace("(посада, прізвище, ім'я та по батькові, підпис)", "").Replace("_", ""));
                    if (!data.ContainsKey(lastKey))
                        data[lastKey] = text;
                    else
                        data[lastKey] += " " + text;
                }
            }

            Overseers = Overseers.SelectMany(o => o.Split(",", StringSplitOptions.RemoveEmptyEntries))
                                .Select(o => o.Trim()).ToList();

            StudentName = data.GetValueOrDefault("StudentName")?.Replace("Студент(ка)", "").Replace("Студентка", "").Replace("Студент", "").Trim('(').Trim() ?? string.Empty;
            PlaceOfPractise = data.GetValueOrDefault("PlaceOfPractise")?.Replace("направляється на виробничу практику на", "")?.Replace("(назва кафедри/підприємства)", "").Replace("(назва катедри/підприємства)", "").Replace("_", "").Trim() ?? string.Empty;
            Prescript = data.GetValueOrDefault("Prescript")?.Replace("Наказ", "").Trim() ?? string.Empty;
            FacultyOverseer = data.GetValueOrDefault("FacultyOverseer")?.Replace("Керівник практики від факультету: ", "").Replace("Декан факультету ______________________________доцент. Кашпур О.Ф. (підпис) Печатка факультету", "") ?? string.Empty;
            PractiseOverseer = data.GetValueOrDefault("PractiseOverseer")?.Replace("Керівник практики від ", "").Replace("кафедри", "").Replace("катедри", "").Replace("підприємства", "").Replace("/", "").Replace("(посада, прізвище, ім'я та по батькові, підпис)", "").Replace("_", "").Trim() ?? string.Empty;
            Dean = data.GetValueOrDefault("Dean")?.Replace("Декан факультету", "").Replace("(підпис)", "").Replace("_", "").Replace("Печатка факультету", "").Trim() ?? string.Empty;

            string dates = data.GetValueOrDefault("Dates") ?? "";
            if (dates.Contains("з") && dates.Contains("по"))
            {
                var parts = dates.Split(new string[] { "з", "по" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    BeginDate = parts[1].Trim();
                    EndDate = parts[2].Replace("року", "").Trim();
                }
            }
        }

        public override string ToString()
        {
            if (IsEmpty)
                return "Інформація про направлення на практику відсутня.";

            return $"*НАПРАВЛЕННЯ НА ПРАКТИКУ:*\n" +
                $"*Студент:* {StudentName}\n" +
                $"*Місце практики:* {PlaceOfPractise}\n" +
                $"*Початок:* {BeginDate}, *Кінець:* {EndDate}\n" +
                $"*Наказ:* {Prescript}\n" +
                $"*Керівник від факультету:* {FacultyOverseer} (Підпис: {(HasFacultyOverseerSignature ? "Так" : "Ні")})\n" +
                $"*Керівник від кафедри/підприємства:* {PractiseOverseer} (Підпис: {(HasPractiseOverseerSignature ? "Так" : "Ні")})\n" +
                $"*Декан Факультету:* {Dean} (Підпис: {(HasDeanSignature ? "Так" : "Ні")})\n" +
                $"*Керівники:* {string.Join(", ", Overseers)}\n";
        }
    }


}