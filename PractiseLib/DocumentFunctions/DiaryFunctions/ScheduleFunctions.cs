using PractiseDiaryKNU;
using SplittingScript.DocumentFunctions;

namespace DocumentFunctions
{
    public static partial class PractiseDiaryFunctions
    {
        public static CheckResult ScheduleIsNotEmpty(PractiseDiary diary)
        {
            return new CheckResult(!diary.Schedule.IsEmpty, "Календарний графік проходження практики", "Календарний графік порожній", nameof(ScheduleIsNotEmpty));
        }
        public static CheckResult ScheduleHasNoFiller(PractiseDiary diary)
        {
            bool verdict = diary.Schedule.Rows[0] != ("1", "2", "3", "4", "5");
            return new CheckResult(verdict, "Календарний графік проходження практики", "Календарний графік містить зразок рядка (1 2 3 4 5)", nameof(ScheduleHasNoFiller));
        }
        public static CheckResult ScheduleSignedByFacultyOverseer(PractiseDiary diary)
        {
            return new CheckResult(diary.Schedule.HasFacultyOverseerSignature, "Календарний графік проходження практики", "Календарний графік не підписано керівником від факультету", nameof(ScheduleSignedByFacultyOverseer));
        }
        public static CheckResult ScheduleSignedByPractiseOverseer(PractiseDiary diary)
        {
            return new CheckResult(diary.Schedule.HasPractiseOverseerSignature, "Календарний графік проходження практики", "Календарний графік не підписано керівником практики", nameof(ScheduleSignedByPractiseOverseer));
        }
    }
}