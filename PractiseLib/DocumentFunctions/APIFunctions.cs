using PractiseDiaryKNU;
using SplittingScript.DocumentFunctions;
using Mscc.GenerativeAI;
using System.Threading.Tasks;
using SplittingScript.PractiseDocuments.PractiseReport;

namespace DocumentFunctions;

public static class APIFunctions
{
    public static CheckResult PractiseDiaryScheduleHasCorrectEndDate(PractiseDiary diary, string EndDate = "02.03")
    {
        throw new NotImplementedException();
    }
    public static async Task<CheckResult[]> PractiseDiaryTitlePageHasCorrectCase_And_PractiseDiaryAssignmentHasCorrectCase_And_PractiseDiaryTaskHasInfinitiveAsync(PractiseDiary diary)
    {
        try
        {

            var Negroid = GeminiAssistant.Instance;
            string prompt = $@"
        Ти — лінгвістичний асистент і в тебе є кілька завдань. 
        1. Твоє завдання — визначати, чи подане ім’я у називному відмінку української мови. 
        Відповідай лише 1 (так) або 0 (ні). Якщо ім’я некоректне або містить помилки, також відповідай 0.

        Приклади:
        Петренко Іван Володимирович → 1
        Коваленко Марія Ігорівна → 1
        Шевченко Олександр Сергійович → 1
        Петренка Івана Володимировича → 0
        Коваленко Марії Ігорівни → 0
        Шевченка Олександра Сергійовича → 0
        Олександра Шевченка → 0
        Івн Петренко → 0
        ivan petrenko → 0
        __ → 0
        ПІБ → 0

        Тепер скажи, чи є наступне ім’я у називному відмінку:
        {diary.Assignment.StudentName}
        
        2. Твоє наступне завдання — визначати, чи подане ім’я у родовому відмінку української мови. 
        Відповідай лише 1 (так) або 0 (ні). Якщо ім’я некоректне або містить помилки, також відповідай 0.

        Приклади:
        Петренка Івана Володимировича → 1  
        Коваленко Марії Ігорівни → 1  
        Шевченка Олександра Сергійовича → 1  
        Петренко Іван Володимирович → 0  
        Коваленко Зоєю Ігорівною → 0  
        Коваленко Маріє Сергіївно  → 0  

        Тепер скажи, чи є наступне ім’я у родовому відмінку:
        {diary.TitlePage.StudentName}

        3. Останнє завдання - перевірити, чи цей текст відповідає на питання ЩО ЗРОБИТИ (Розробити..., опрацювати..., засвоїти..., вивчити...):
        {diary.Task.Content}

        Відповідай лише 1 (так) або 0 (ні).

        Формат відповіді: 
        <відповідь на перше завдання>
        <відповідь на друге завдання>
        <відповідь на третє завдання>";
            var response = await Negroid.Model.GenerateContent(prompt);
            var text = response.Text;
            var responses = text.Split('\n');
            return [
                new CheckResult(responses[0].Contains("1"), "Направлення на практику", "ПІБ не в називному відмінку", nameof(PractiseDiaryTitlePageHasCorrectCase_And_PractiseDiaryAssignmentHasCorrectCase_And_PractiseDiaryTaskHasInfinitiveAsync)),
                new CheckResult(responses[1].Contains("1"), "Титульна сторінка", "ПІБ не в родовому відмінку", nameof(PractiseDiaryTitlePageHasCorrectCase_And_PractiseDiaryAssignmentHasCorrectCase_And_PractiseDiaryTaskHasInfinitiveAsync)),
                new CheckResult(responses[2].Contains("1"), "Завдання на практику", "Завдання не відповідає на питання \"Що зробити?\"", nameof(PractiseDiaryTitlePageHasCorrectCase_And_PractiseDiaryAssignmentHasCorrectCase_And_PractiseDiaryTaskHasInfinitiveAsync)),
            ];
        }
        catch(Exception ex)
        {
            Console.WriteLine($"[Eror] {ex.Message}");
            return [new CheckResult(false, "Проблема перевірки", "Під час перевірки відмінків та завдання на практику виникла помилка.", nameof(PractiseDiaryTitlePageHasCorrectCase_And_PractiseDiaryAssignmentHasCorrectCase_And_PractiseDiaryTaskHasInfinitiveAsync))];
        }
    }
    public static async Task<string> test1(string name1, string name2, string text1)
    {
        var Negroid = GeminiAssistant.Instance;
        string prompt = $@"
    Ти — лінгвістичний асистент і в тебе є кілька завдань. 
    1. Твоє завдання — визначати, чи подане ім’я у називному відмінку української мови. 
    Відповідай лише 1 (так) або 0 (ні). Якщо ім’я некоректне або містить помилки, також відповідай 0.

    Приклади:
    Петренко Іван Володимирович → 1
    Коваленко Марія Ігорівна → 1
    Шевченко Олександр Сергійович → 1
    Петренка Івана Володимировича → 0
    Коваленко Марії Ігорівни → 0
    Шевченка Олександра Сергійовича → 0
    Олександра Шевченка → 0
    Івн Петренко → 0
    ivan petrenko → 0
    __ → 0
    ПІБ → 0

    Тепер скажи, чи є наступне ім’я у називному відмінку:
    {name1}
    
    2. Твоє наступне завдання — визначати, чи подане ім’я у родовому відмінку української мови. 
    Відповідай лише 1 (так) або 0 (ні). Якщо ім’я некоректне або містить помилки, також відповідай 0.

    Приклади:
    Петренка Івана Володимировича → 1  
    Коваленко Марії Ігорівни → 1  
    Шевченка Олександра Сергійовича → 1  
    Петренко Іван Володимирович → 0  
    Коваленко Зоєю Ігорівною → 0  
    Коваленко Маріє Сергіївно  → 0  

    Тепер скажи, чи є наступне ім’я у родовому відмінку:
    {name2}

    3. Останнє завдання - перевірити, чи цей текст відповідає на питання ЩО ЗРОБИТИ (Розробити..., опрацювати..., засвоїти..., вивчити...):
    {text1}

    Відповідай лише 1 (так) або 0 (ні).

    Формат відповіді: 
    <відповідь на перше завдання>
    <відповідь на друге завдання>
    <відповідь на третє завдання>";
        var response = await Negroid.Model.GenerateContent(prompt);
        var text = response.Text;
        return text;
    }

    public static async Task<CheckResult> PractiseReportHasConclusions(PractiseReport report)
    {
        try
        {

            var Negroid = GeminiAssistant.Instance;
            string prompt = $@"
    Ти — викладач на факультеті комп'ютерних наук та кібернетики.  
    Твоє завдання — перевіряти студентські звіти про виробничу практику.  

    📌 **Звіт має містити висновки.**  
    Ось **7 зразків** тексту, які є **висновками**:  

    1️⃣ Під час проходження виробничої практики було закріплено теоретичні знання з програмування та розширено практичні навички роботи з сучасними технологіями розробки програмного забезпечення, зокрема у сфері веб-розробки та баз даних.  

    2️⃣ Я навчився ефективно працювати в команді розробників, використовуючи системи контролю версій Git та методології Agile, що дозволило краще зрозуміти процеси розробки програмного забезпечення в реальних умовах.  

    3️⃣ Під час практики було здобуто цінний досвід у вирішенні реальних технічних завдань, включаючи аналіз вимог, проектування архітектури програмного забезпечення та його тестування, що значно поглибило моє розуміння повного циклу розробки.  

    4️⃣ Я зрозумів важливість дотримання корпоративних стандартів кодування та документації, навчився писати чистий, підтримуваний код та ефективно використовувати інструменти для автоматизації процесів розробки.  

    5️⃣ За період практики було розширено професійний світогляд через участь у технічних обговореннях, code review та вивчення нових технологій, що допомогло краще зрозуміти сучасні тенденції в галузі комп'ютерних наук та визначити напрямки подальшого професійного розвитку.  

    6️⃣ Практика сприяла розвитку навичок системного аналізу, розробки ефективних алгоритмів та їх інтеграції у реальні програмні рішення. Отриманий досвід дозволить впроваджувати передові методики розробки, оптимізувати робочі процеси та підвищувати якість програмного забезпечення.

    7️⃣Виконана робота дала змогу вдосконалити навички об’єктно-орієнтованого програмування, застосування патернів проєктування та роботи з API нейромереж. Набуті знання дозволять ефективніше вирішувати завдання автоматизації аналізу текстових даних та впроваджувати сучасні технології в реальні проєкти.  

    ---  

    📌 **Тепер проаналізуй цей звіт:**  
    ```  
    {report.Content}  
    ```  

    ❓ **Чи містить цей текст висновки?**  
    Відповідай **лише** `1` (так) або `0` (ні).
";

            var response = await Negroid.Model.GenerateContent(prompt);
            var text = response.Text?.Trim();
            return new CheckResult(text?.Contains("1") ?? false, "Висновки", "Звіт не містить висновків", nameof(PractiseReportHasConclusions));
        }
        catch(Exception ex)
        {
            Console.WriteLine($"[Eror] {ex.Message}");
            return new CheckResult(false, "Проблема перевірки", "Під час перевірки висновків виникла помилка.", nameof(PractiseReportHasConclusions));
        }
    }
}