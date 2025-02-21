using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;

namespace PractiseDiaryKNU
{
    public class Rules
    {
        public readonly bool IsEmpty = true;
        public readonly string Content = string.Empty;

        public Rules(List<OpenXmlElement> page)
        {
            if (page == null || page.Count == 0)
            {
                IsEmpty = true;
                return;
            }
            IsEmpty = false;
            Content = string.Join(" ", page.Where(e => !string.IsNullOrEmpty(e.InnerText)).Select(e => e.InnerText).ToList());
        }
        public override string ToString()
        {
            return !IsEmpty ? "*Правила ведення й оформлення щоденника:*\n".ToUpper()+$"{Content}\n" : "Правила ведення й оформлення щоденника відсутні.";
        }
    }
}