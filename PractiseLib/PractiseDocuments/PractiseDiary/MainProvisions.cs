using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;

namespace PractiseDiaryKNU
{
    public class MainProvisions
    {
        public readonly bool IsEmpty = true;
        public readonly string Content = string.Empty;

        public MainProvisions(List<OpenXmlElement> page)
        {
            if (page == null || page.Count == 0)
            {
                IsEmpty = true;
                return;
            }
            IsEmpty = false;
            Content = string.Join(" ", page.Where(e => !string.IsNullOrEmpty(e.InnerText)).Select(e => e.InnerText).ToList()).Trim();
        }
        public override string ToString()
        {
            return !IsEmpty ? $"*ОСНОВНІ ПОЛОЖЕННЯ ПРАКТИКИ:*\n{Content}\n" : "Основні положення практики відсутні.";
        }
    }
}