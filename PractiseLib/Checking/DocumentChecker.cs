using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SplittingScript.PractiseDiaryKNU;

namespace SplittingScript.Checking
{
    public class DocumentChecker
    {
        public ICheckingStrategy? Checker {get; set; }
        public DocumentChecker(ICheckingStrategy checker)
        {
            this.Checker = checker;
        }
        public DocumentChecker()
        {
            this.Checker = new PractiseDiaryChecker();
        }

        public async Task<string> CheckAsync(IPractiseDocument document)
        {
            var res = await Checker?.AnalyzeAsync(document);
            string ans = string.Empty;
            if (res == null || res.Count == 0)
            {
                return "✅*Помилок не знайдено :)*";
            }
            foreach(var section in res)
            {
                ans+=$"*{section.Key.ToUpper()}*"+":\n";
                foreach(var message in section.Value)
                {
                    ans += message+"\n";
                }
                ans+="\n";
            }
            return ans;
        }
    }
}