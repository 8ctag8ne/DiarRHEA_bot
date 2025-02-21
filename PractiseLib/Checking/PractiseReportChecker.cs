using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFunctions;
using PractiseDiaryKNU;
using SplittingScript.DocumentFunctions;
using SplittingScript.PractiseDiaryKNU;
using SplittingScript.PractiseDocuments.PractiseReport;

namespace SplittingScript.Checking
{
    public class PractiseReportChecker : ICheckingStrategy
    {
        private List<Func<PractiseReport, CheckResult>> checks = new List<Func<PractiseReport, CheckResult>>
        {
            PractiseReportFunctions.HasValidName,
            PractiseReportFunctions.HasCorrectGroup,
            PractiseReportFunctions.HasCorrectCourse,
            PractiseReportFunctions.HasCorrectEndDate,
            PractiseReportFunctions.HasSignature
        };

        private List<Func<PractiseReport, Task<CheckResult>>> AICheck = new List<Func<PractiseReport, Task<CheckResult>>>
        {
            APIFunctions.PractiseReportHasConclusions,
        };

        public async Task<Dictionary<string, List<string>>> AnalyzeAsync(IPractiseDocument document)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            foreach (var check in checks)
            {
                var res = check(document as PractiseReport ?? throw new ArgumentNullException(nameof(document)));
                if (!res.Verdict)
                {
                    if (!result.ContainsKey(res.Section))
                        result[res.Section] = new List<string>();
                    
                    result[res.Section].Add(res.ToString());
                }
            }

            var resAI = await AICheck[0](document as PractiseReport ?? throw new ArgumentNullException(nameof(document)));
            if (!resAI.Verdict)
            {
                if (!result.ContainsKey(resAI.Section))
                    result[resAI.Section] = new List<string>();
                
                result[resAI.Section].Add(resAI.ToString());
            }

            return result;
        }
    }
}