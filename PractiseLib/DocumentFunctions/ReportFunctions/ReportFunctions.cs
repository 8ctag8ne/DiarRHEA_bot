using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SplittingScript.PractiseDocuments.PractiseReport;

namespace SplittingScript.DocumentFunctions
{
    public static class PractiseReportFunctions
    {
        public static CheckResult HasCorrectCourse(PractiseReport report)
        {
            string course = "3";
            bool verdict = report.Course == course;
            return new CheckResult(verdict, "Інформація про студента", $"Значення курсу має бути {course}.", nameof(HasCorrectCourse));
        }
        public static CheckResult HasCorrectGroup(PractiseReport report)
        {
            string[]groups = ["ТТП-31", "ТТП-32"];
            bool verdict = groups.Contains(report.Group);
            return new CheckResult(verdict, "Інформація про студента", $"Група має бути з цього переліку: {string.Join(", ", groups)}.", nameof(HasCorrectGroup));
        }

        public static CheckResult HasValidName(PractiseReport report)
        {
            bool verdict = report.StudentName.Split(" ").Length == 3;
            return new CheckResult(verdict, "Інформація про студента", $"ПІБ записане неправильно або є неповним.", nameof(HasValidName));
        }
        public static CheckResult HasSignature(PractiseReport report)
        {
            bool verdict = report.IsSigned;
            return new CheckResult(verdict, "Загальна інформація", $"Документ не має підпису студента.", nameof(HasSignature));
        }
        public static CheckResult HasCorrectEndDate(PractiseReport report)
        {
            string endDate = "2 березня 2025";
            bool verdict = report.Date == endDate;
            return new CheckResult(verdict, "Загальна інформація", $"Документ має неправильну або відсутню дату.", nameof(HasCorrectEndDate));
        }
    }
}