using PractiseDiaryKNU;
using SplittingScript.DocumentFunctions;

namespace DocumentFunctions
{
    public static partial class PractiseDiaryFunctions
    {
        public static CheckResult TitlePageIsNotEmpty(PractiseDiary diary)
        {
            return new CheckResult(!diary.TitlePage.IsEmpty, "Титульна сторінка", "Титульна сторінка порожня", nameof(TitlePageIsNotEmpty));
        }

        public static CheckResult TitlePageContainsInitials(PractiseDiary diary)
        {
            bool verdict = diary.TitlePage.StudentName != null && diary.TitlePage.StudentName.Length > 0;
            return new CheckResult(verdict, "Титульна сторінка", "Титульна сторінка не містить ПІБ студента", nameof(TitlePageContainsInitials));
        }

        public static CheckResult TitlePageContainsCourseAndFaculty(PractiseDiary diary)
        {
            bool verdict = diary.TitlePage.Course != null && diary.TitlePage.Course.Length > 0 && diary.TitlePage.Faculty != null && diary.TitlePage.Faculty.Length > 0;
            return new CheckResult(verdict, "Титульна сторінка", "Титульна сторінка не містить курсу та факультету студента", nameof(TitlePageContainsCourseAndFaculty));
        }

        public static CheckResult TitlePageContainsSpecialty(PractiseDiary diary)
        {
            bool verdict = diary.TitlePage.Program != null && diary.TitlePage.Program.Length > 0;
            return new CheckResult(verdict, "Титульна сторінка", "Титульна сторінка не містить спеціальності студента", nameof(TitlePageContainsSpecialty));
        }

        public static CheckResult TitlePageContainsUniversity(PractiseDiary diary)
        {
            bool verdict = diary.TitlePage.University != null && diary.TitlePage.University.Length > 0;
            return new CheckResult(verdict, "Титульна сторінка", "Титульна сторінка не містить університету студента", nameof(TitlePageContainsUniversity));
        }
    }
}