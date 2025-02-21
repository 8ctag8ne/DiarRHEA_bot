using PractiseDiaryKNU;
using SplittingScript.DocumentFunctions;

namespace DocumentFunctions
{
    public static partial class PractiseDiaryFunctions
    {
        public static CheckResult AssignmentIsNotEmpty(PractiseDiary diary)
        {
            return new CheckResult(!diary.Assignment.IsEmpty, "Направлення на практику", "Направлення відсутнє в документі", nameof(AssignmentIsNotEmpty));
        }
        public static CheckResult AssignmentContainsInitials(PractiseDiary diary)
        {
            bool verdict = diary.Assignment.StudentName != null && diary.Assignment.StudentName.Length > 0;
            return new CheckResult(verdict, "Направлення на практику", "Направлення не містить ПІБ студента", nameof(AssignmentContainsInitials));
        }

        public static CheckResult AssignmentContainsPlaceOfPractise(PractiseDiary diary)
        {
            bool verdict = diary.Assignment.PlaceOfPractise != null && diary.Assignment.PlaceOfPractise.Length > 0;
            return new CheckResult(verdict, "Направлення на практику", "Направлення не містить місця практики", nameof(AssignmentContainsPlaceOfPractise));
        }

        public static CheckResult AssignmentContainsDeadlines(PractiseDiary diary)
        {
            bool verdict = diary.Assignment.BeginDate != null && diary.Assignment.BeginDate.Length > 0
                && diary.Assignment.EndDate != null && diary.Assignment.EndDate.Length > 0;
            return new CheckResult(verdict, "Направлення на практику", "Направлення не містить термінів проходження практики", nameof(AssignmentContainsDeadlines));
        }

        public static CheckResult AssignmentContainsPrescript(PractiseDiary diary)
        {
            bool verdict = diary.Assignment.Prescript != null && diary.Assignment.Prescript.Length > 0;
            return new CheckResult(verdict, "Направлення на практику", "Направлення не містить наказу", nameof(AssignmentContainsPrescript));
        }

        public static CheckResult AssignmentContainsFacultyOverseer(PractiseDiary diary)
        {
            bool verdict = diary.Assignment.FacultyOverseer != null && diary.Assignment.FacultyOverseer.Length > 0
                && diary.Assignment.FacultyOverseer.ToLower().Contains("омельчук");
            return new CheckResult(verdict, "Направлення на практику", "Направлення не містить керівника від факультету", nameof(AssignmentContainsFacultyOverseer));
        }

        public static CheckResult AssignmentContainsAllOverseers(PractiseDiary diary)
        {
            bool verdict = diary.Assignment.Overseers != null && diary.Assignment.Overseers.Count > 0
                && diary.Assignment.Overseers.Any(e => e.Contains("Зубенко"))
                && diary.Assignment.Overseers.Any(e => e.Contains("Свистунов"))
                && diary.Assignment.Overseers.Any(e => e.Contains("Шишацький"))
                && diary.Assignment.Overseers.Any(e => e.Contains("Галавай"))
                && diary.Assignment.Overseers.Any(e => e.Contains("Пушкаренко"));
            return new CheckResult(verdict, "Направлення на практику", "Направлення не містить усіх необхідних керівників", nameof(AssignmentContainsAllOverseers));
        }

        public static CheckResult AssignmentContainsPractiseOverseer(PractiseDiary diary)
        {
            bool verdict = diary.Assignment.PractiseOverseer != null && diary.Assignment.PractiseOverseer.Length > 0;
            return new CheckResult(verdict, "Направлення на практику", "Направлення не містить керівника практики", nameof(AssignmentContainsPractiseOverseer));
        }

        public static CheckResult AsssignmentSignedByFacultyOverseer(PractiseDiary diary)
        {
            bool verdict = diary.Assignment.HasFacultyOverseerSignature;
            return new CheckResult(verdict, "Направлення на практику", "Направлення не підписано керівником від факультету", nameof(AsssignmentSignedByFacultyOverseer));
        }

        public static CheckResult AsssignmentSignedByPractiseOverseer(PractiseDiary diary)
        {
            bool verdict = diary.Assignment.HasPractiseOverseerSignature;
            return new CheckResult(verdict, "Направлення на практику", "Направлення не підписано керівником практики", nameof(AsssignmentSignedByPractiseOverseer));
        }
        public static CheckResult AssignmentSignedByDean(PractiseDiary diary)
        {
            bool verdict = diary.Assignment.HasDeanSignature;
            return new CheckResult(verdict, "Направлення на практику", "Направлення не підписано деканом", nameof(AssignmentSignedByDean));
        }
    }
}