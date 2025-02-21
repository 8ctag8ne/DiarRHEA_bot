using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFunctions;
using PractiseDiaryKNU;
using SplittingScript.DocumentFunctions;
using SplittingScript.PractiseDiaryKNU;

namespace SplittingScript.Checking
{
    public class PractiseDiaryChecker : ICheckingStrategy
    {
        private List<Func<PractiseDiary, CheckResult>> checks = new List<Func<PractiseDiary, CheckResult>>
        {
            PractiseDiaryFunctions.AssignmentIsNotEmpty,
            PractiseDiaryFunctions.AssignmentContainsDeadlines,
            PractiseDiaryFunctions.AssignmentContainsAllOverseers,
            PractiseDiaryFunctions.AssignmentContainsInitials,
            PractiseDiaryFunctions.AssignmentContainsPlaceOfPractise,
            PractiseDiaryFunctions.AssignmentContainsPrescript,
            PractiseDiaryFunctions.AssignmentContainsFacultyOverseer,
            PractiseDiaryFunctions.AssignmentContainsPractiseOverseer,
            PractiseDiaryFunctions.AsssignmentSignedByFacultyOverseer,
            PractiseDiaryFunctions.AsssignmentSignedByPractiseOverseer,
            PractiseDiaryFunctions.EvaluationIsNotEmpty,
            PractiseDiaryFunctions.TaskIsNotEmpty,
            PractiseDiaryFunctions.FacultyConclusionIsNotEmpty,
            PractiseDiaryFunctions.MainProvisionsIsNotEmpty,
            PractiseDiaryFunctions.RulesIsNotEmpty,
            PractiseDiaryFunctions.FacultyConclusionsSignedByFacultyOverseer,
            PractiseDiaryFunctions.FacultyConclusionsSignedByAllOverseers,
            PractiseDiaryFunctions.TaskSignedByFacultyOverseer,
            PractiseDiaryFunctions.EvaluationSignedByPractiseOverseer,
            PractiseDiaryFunctions.ScheduleIsNotEmpty,
            PractiseDiaryFunctions.ScheduleHasNoFiller,
            PractiseDiaryFunctions.ScheduleSignedByFacultyOverseer,
            PractiseDiaryFunctions.ScheduleSignedByPractiseOverseer,
            PractiseDiaryFunctions.TitlePageIsNotEmpty,
            PractiseDiaryFunctions.TitlePageContainsInitials,
            PractiseDiaryFunctions.TitlePageContainsCourseAndFaculty,
            PractiseDiaryFunctions.TitlePageContainsSpecialty,
            PractiseDiaryFunctions.TitlePageContainsUniversity,
            PractiseDiaryFunctions.EvaluationContainsInitials,
            PractiseDiaryFunctions.AllInitialsMatch,
            PractiseDiaryFunctions.EvaluationAndFacultyConclusionHaveTheSameDate,
            PractiseDiaryFunctions.AssignmentSignedByDean,
            PractiseDiaryFunctions.FacultyConclusionContainsNonEmptyDate,
            PractiseDiaryFunctions.EvaluationContainsNonEmptyDate,
        };

        private List<Func<PractiseDiary, Task<CheckResult[]>>> AICheck = new List<Func<PractiseDiary, Task<CheckResult[]>>>{
            APIFunctions.PractiseDiaryTitlePageHasCorrectCase_And_PractiseDiaryAssignmentHasCorrectCase_And_PractiseDiaryTaskHasInfinitiveAsync,
        };

        public async Task<Dictionary<string, List<string>>> AnalyzeAsync(IPractiseDocument document)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            foreach (var check in checks)
            {
                var res = check(document as PractiseDiary ?? throw new ArgumentNullException(nameof(document)));
                if (!res.Verdict)
                {
                    if (!result.ContainsKey(res.Section))
                        result[res.Section] = new List<string>();
                    
                    result[res.Section].Add(res.ToString());
                }
            }

            var resAI = await AICheck[0](document as PractiseDiary ?? throw new ArgumentNullException(nameof(document)));
            foreach(var rizz in resAI)
            {
                if (!rizz.Verdict)
                {
                    if (!result.ContainsKey(rizz.Section))
                        result[rizz.Section] = new List<string>();
                    
                    result[rizz.Section].Add(rizz.ToString());
                }
            }

            return result;
        }
    }
}