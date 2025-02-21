using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SplittingScript.DocumentFunctions
{
    public class CheckResult
    {
        public bool Verdict { get; }
        public string Section { get; }
        public string Message { get; }

        public string Function {get; set;}

        public CheckResult(bool verdict, string section, string message, string function)
        {
            Verdict = verdict;
            Section = section;
            Message = message;
            Function = function;
        }

        public override string ToString()
        {
            return Verdict ? $"✅{Function}" : $"❌ {Message}";
        }
        public string ToStringDebug()
        {
            return Verdict ? $"✅ {Section}: {Function}" : $"❌ {Section}: {Message}";
        }
    }
}