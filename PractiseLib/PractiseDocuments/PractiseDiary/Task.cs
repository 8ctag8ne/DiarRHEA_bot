using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using SplittingScript;

namespace PractiseDiaryKNU
{
    public class Task
    {
        public readonly bool IsEmpty = true;
        public string Content = string.Empty;
        public bool HasFacultyOverseerSignature = false;

        public Task(List<OpenXmlElement> page)
        {
            if (page == null || page.Count == 0)
            {
                IsEmpty = true;
                return;
            }
            IsEmpty = false;

            Dictionary<string, string> data = new Dictionary<string, string>();
            string lastKey = "";
            bool signArea = false;
            foreach (var element in page)
            {
                bool containsSignature = WordHelper.ContainsSignature(element);
                string text = WordHelper.RemoveImagesAndGetText(element).Trim();

                // if (string.IsNullOrWhiteSpace(text)) continue;

                if (text.ToLower().Contains("керівник практики"))
                {
                    signArea = true;
                    lastKey = "Sign";
                }
                else if(text.ToLower().Contains("завдання на практику"))
                    lastKey = "Title";
                else lastKey = "Content";

                if(containsSignature)
                {
                    HasFacultyOverseerSignature = true;
                }

                if (!string.IsNullOrEmpty(lastKey))
                {
                    if (!data.ContainsKey(lastKey))
                        data[lastKey] = text;
                    else
                        data[lastKey] += " " + text;
                }
            }
            Content = data.GetValueOrDefault("Content")?.Replace("(підпис)", "").Trim() ?? string.Empty;
        }
        public override string ToString()
        {
            return !IsEmpty ? $"*ЗАВДАННЯ НА ПРАКТИКУ:*\n {Content}\n"+$"*Підпис керівника практики від факультету:* {(HasFacultyOverseerSignature ? "Так" : "Ні")}" : "Завдання на практику відсутнє.\n";
        }
    }
}