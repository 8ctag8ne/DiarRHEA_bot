using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using PractiseDiaryKNU;
using SplittingScript.DocumentFunctions;

namespace DocumentFunctions
{
    public static partial class PractiseDiaryFunctions
    {
        public static CheckResult EvaluationIsNotEmpty(PractiseDiary diary)
        {
            bool verdict = !diary.Evaluation.IsEmpty;
            return new CheckResult(verdict, "Характеристика й оцінка роботи студента на практиці", "Характеристика порожня", nameof(EvaluationIsNotEmpty));
        }

        public static CheckResult TaskIsNotEmpty(PractiseDiary diary) 
        {
            bool verdict = !diary.Task.IsEmpty;
            return new CheckResult(verdict, "Завдання на практику", "Завдання порожнє", nameof(TaskIsNotEmpty));
        }

        public static CheckResult FacultyConclusionIsNotEmpty(PractiseDiary diary)
        {
            bool verdict = !diary.FacultyConclusion.IsEmpty;
            return new CheckResult(verdict, "Висновок керівника практики від факультету про роботу студента", "Висновок керівника від факультету порожній", nameof(FacultyConclusionIsNotEmpty));
        }

        public static CheckResult MainProvisionsIsNotEmpty(PractiseDiary diary)
        {
            bool verdict = !diary.MainProvisions.IsEmpty;
            return new CheckResult(verdict, "Основні положення практики", "Основні положення порожні", nameof(MainProvisionsIsNotEmpty));
        }

        public static CheckResult RulesIsNotEmpty(PractiseDiary diary)
        {
            bool verdict = !diary.Rules.IsEmpty;
            return new CheckResult(verdict, "Правила ведення й оформлення щоденника", "Правила порожні", nameof(RulesIsNotEmpty));
        }

        public static CheckResult IsDocx(string filePath)
        {
            bool verdict = filePath.EndsWith(".docx");
            return new CheckResult(verdict, "Формат файлу", "Файл не у форматі DOCX", nameof(IsDocx));
        }

        public static CheckResult IsPdf(string filePath)
        {
            bool verdict = filePath.EndsWith(".pdf");
            return new CheckResult(verdict, "Формат файлу", "Файл не у форматі PDF", nameof(IsPdf));
        }

        public static CheckResult EvaluationContainsInitials(PractiseDiary diary)
        {
            bool verdict = diary.Evaluation.StudentName != string.Empty && diary.Evaluation.StudentName.ToUpper() != "ПІБ";
            return new CheckResult(verdict, "Характеристика й оцінка роботи студента на практиці", "Характеристика не містить ПІБ студента", nameof(EvaluationContainsInitials));
        }

        public static CheckResult EvaluationAndFacultyConclusionHaveTheSameDate(PractiseDiary diary)
        {
            bool verdict = diary.Evaluation.Date.Replace("_", "").Replace("\"", "").Trim() == diary.FacultyConclusion.Date.Replace("_", "").Replace("\"", "").Trim();
            return new CheckResult(verdict, "Щоденник", "Характеристика та висновок мають різні дати",  nameof(EvaluationAndFacultyConclusionHaveTheSameDate));
        }
        //new
        public static CheckResult EvaluationContainsNonEmptyDate(PractiseDiary diary)
        {
            bool verdict = true;
            string date = diary.Evaluation.Date.Split(" ")[0].Replace("\"", "").Replace("_", "").Replace("”", "").Replace("“", "").Trim();
            if (date.Length == 0)
            {
                verdict = false;
            }

            foreach(var el in date)
            {
                if(!char.IsDigit(el))
                {
                    verdict = false;
                }
            }

            return new CheckResult(verdict, "Характеристика й оцінка роботи студента на практиці", "Характеристика має порожню або некорректну дату.",  nameof(EvaluationContainsNonEmptyDate));
        }
        //new
        public static CheckResult FacultyConclusionContainsNonEmptyDate(PractiseDiary diary)
        {
            bool verdict = true;
            string date = diary.FacultyConclusion.Date.Split(" ")[0].Replace("\"", "").Replace("_", "").Replace("”", "").Replace("“", "").Trim();;
            if (date.Length == 0)
            {
                verdict = false;
            }

            foreach(var el in date)
            {
                if(!char.IsDigit(el))
                {
                    verdict = false;
                }
            }

            return new CheckResult(verdict, "Висновок керівника практики від факультету про роботу студента", "Висновок має порожню або некорректну дату.",  nameof(FacultyConclusionContainsNonEmptyDate));
        }

        public static CheckResult FacultyConclusionsSignedByFacultyOverseer(PractiseDiary diary)
        {
            bool verdict = diary.FacultyConclusion.HasFacultyOverseerSignature;
            return new CheckResult(verdict, "Висновок керівника практики від факультету про роботу студента", "Висновок не підписано керівником від факультету", nameof(FacultyConclusionsSignedByFacultyOverseer));
        }

        public static CheckResult FacultyConclusionsSignedByAllOverseers(PractiseDiary diary)
        {
            bool verdict = diary.FacultyConclusion.HasAllOverseersSignatures;
            return new CheckResult(verdict, "Висновок керівника практики від факультету про роботу студента", "Висновок не підписано всіма керівниками", nameof(FacultyConclusionsSignedByAllOverseers));
        }

        public static CheckResult TaskSignedByFacultyOverseer(PractiseDiary diary)
        {
            bool verdict = diary.Task.HasFacultyOverseerSignature;
            return new CheckResult(verdict, "Завдання на практику", "Завдання не підписано керівником від факультету", nameof(TaskSignedByFacultyOverseer));
        }

        public static CheckResult EvaluationSignedByPractiseOverseer(PractiseDiary diary)
        {
            bool verdict = diary.Evaluation.HasOverseerSignature;
            return new CheckResult(verdict, "Характеристика й оцінка роботи студента на практиці", "Характеристика не підписана керівником практики", nameof(EvaluationSignedByPractiseOverseer));
        }

        //new
        public static CheckResult AllInitialsMatch(PractiseDiary diary)
        {
            try
            {
                // Нормалізація - переведення в нижній регістр та видалення крапок
                string[] titleName = diary.TitlePage.StudentName.ToLower().Replace(".", "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string[] evalName = diary.Evaluation.StudentName.ToLower().Replace(".", "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string[] assignName = diary.Assignment.StudentName.ToLower().Replace(".", "").Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // Перевірка на пусті значення
                if (titleName.Length == 0 || evalName.Length == 0 || assignName.Length == 0)
                {
                    return new CheckResult(false, "Щоденник", "Одне або більше імен порожні",  nameof(AllInitialsMatch));
                }

                // Порівняння всіх пар імен
                bool match = CompareTwoNames(titleName, evalName) && 
                            CompareTwoNames(titleName, assignName) && 
                            CompareTwoNames(evalName, assignName);

                return new CheckResult(match, "Щоденник", "ПІБ не збігаються у характеристиці, титульній сторінці та направленні",  nameof(AllInitialsMatch));
            }
            catch (Exception ex)
            {
                return new CheckResult(false, "Щоденник", $"Помилка при порівнянні імен: {ex.Message}", nameof(AllInitialsMatch));
            }
        }

        private static bool CompareTwoNames(string[] name1, string[] name2)
        {
            // Якщо різна кількість частин імені
            if (name1.Length != name2.Length) 
                return false;

            for (int i = 0; i < name1.Length; i++)
            {
                string part1 = name1[i];
                string part2 = name2[i];
                bool isInitial1 = part1.Length == 1;
                bool isInitial2 = part2.Length == 1;

                // Якщо хоча б одна частина - ініціал
                if (isInitial1 || isInitial2)
                {
                    if (isInitial1 && isInitial2)
                    {
                        // Обидві частини - ініціали
                        if (part1 != part2) return false;
                    }
                    else
                    {
                        // Одна частина - ініціал, інша - повне слово
                        string initial = isInitial1 ? part1 : part2;
                        string fullWord = isInitial1 ? part2 : part1;
                        if (fullWord[0] != initial[0]) return false;
                    }
                }
                else
                {
                    // Обидві частини - повні слова
                    // Для прізвища та імені відкидаємо останні 2 літери
                    // Для по-батькові відкидаємо останні 4 літери
                    int charsToCompare = (i == 2) ? 
                        Math.Min(part1.Length - 4, part2.Length - 4) : 
                        Math.Min(part1.Length - 2, part2.Length - 2);

                    if (charsToCompare <= 0) return false;

                    if (part1.Substring(0, charsToCompare) != part2.Substring(0, charsToCompare))
                        return false;
                }
            }

            return true;
        }
    }
}