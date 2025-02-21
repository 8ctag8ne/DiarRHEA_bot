using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SplittingScript.PractiseDiaryKNU;

namespace SplittingScript.Checking
{
    public interface ICheckingStrategy
    {
        Task<Dictionary<string, List<string>>> AnalyzeAsync(IPractiseDocument document);
    }
}