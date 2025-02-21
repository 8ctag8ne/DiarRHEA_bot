using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SplittingScript;
using SplittingScript.PractiseDiaryKNU;

namespace PractiseDiaryKNU
{
    public class PractiseDiary : IPractiseDocument
    {
        private string filepath;
        public TitlePage TitlePage;
        public Assignment Assignment;
        public Evaluation Evaluation;
        public Schedule Schedule;
        public FacultyConclusion FacultyConclusion;
        public Rules Rules;  
        public MainProvisions MainProvisions;
        public Task Task;
        public List<OpenXmlElement> RulesList = new List<OpenXmlElement>();
        public List<OpenXmlElement> TitlePageList = new List<OpenXmlElement>();
        public List<OpenXmlElement> AssignmentList = new List<OpenXmlElement>();
        public List<OpenXmlElement> MainProvisionsList = new List<OpenXmlElement>();
        public List<OpenXmlElement> FacultyConclusionList = new List<OpenXmlElement>();
        public List<OpenXmlElement> TaskList = new List<OpenXmlElement>();
        public List<OpenXmlElement> ScheduleList = new List<OpenXmlElement>();
        public List<OpenXmlElement> EvaluationList = new List<OpenXmlElement>();

        private readonly List<(string SectionName, FieldInfo Field)> Sections;

        public PractiseDiary(string inputFilePath)
        {
            Sections = new List<(string, FieldInfo)>
            {
                ("Правила ведення й оформлення щоденника", GetType().GetField("RulesList")),
                ("Київський національний університет імені Тараса Шевченка", GetType().GetField("TitlePageList")),
                ("НАПРАВЛЕННЯ НА ПРАКТИКУ", GetType().GetField("AssignmentList")),
                ("Основні положення практики", GetType().GetField("MainProvisionsList")),
                ("Висновок керівника практики від факультету про роботу студента", GetType().GetField("FacultyConclusionList")),
                ("Завдання на практику", GetType().GetField("TaskList")),
                ("Календарний графік проходження практики", GetType().GetField("ScheduleList")),
                ("Характеристика й оцінка роботи студента на практиці", GetType().GetField("EvaluationList"))
            };
            this.filepath = inputFilePath;
            using (WordprocessingDocument doc = WordprocessingDocument.Open(filepath, false))
            {
                Body body = doc.MainDocumentPart.Document.Body;
                List<OpenXmlElement> elements = body.Elements<OpenXmlElement>().ToList();
                List<OpenXmlElement> currentSectionContent = new List<OpenXmlElement>();
                int sectionIndex = 7;

                foreach (var element in elements)
                {
                    string text = element.InnerText.Trim();

                    for(int index = 0; index<Sections.Count; index++)
                    {
                        if (text.ToLower().Replace(" ", "").Contains(Sections[index].SectionName.ToLower().Replace(" ", "")) && !text.EndsWith(",") && !text.EndsWith("!"))
                        {
                            // Console.WriteLine(text);
                            currentSectionContent = currentSectionContent.Where(e => !string.IsNullOrWhiteSpace(e.InnerText) || WordHelper.ContainsNonTextContent(e)).ToList();
                            Sections[sectionIndex].Field.SetValue(this, currentSectionContent);
                            currentSectionContent = new List<OpenXmlElement>();
                            sectionIndex = index;
                            break;
                        }
                    }
                    
                    currentSectionContent.Add(element.CloneNode(true));
                }
                
                if (currentSectionContent.Count > 0)
                {
                    currentSectionContent = currentSectionContent.Where(e => !string.IsNullOrWhiteSpace(e.InnerText) || WordHelper.ContainsNonTextContent(e)).ToList();
                    Sections[sectionIndex].Field.SetValue(this, currentSectionContent);
                    currentSectionContent = new List<OpenXmlElement>();
                }

                TitlePage = new TitlePage(TitlePageList);
                Assignment = new Assignment(AssignmentList);
                Evaluation = new Evaluation(EvaluationList);
                Schedule = new Schedule(ScheduleList);
                FacultyConclusion = new FacultyConclusion(FacultyConclusionList);
                Rules = new Rules(RulesList);
                MainProvisions = new MainProvisions(MainProvisionsList);
                Task = new Task(TaskList);
            }
        }
        public void CreateDocx()
        {
            string id = Guid.NewGuid().ToString();
            string folder = "Test\\"+DateTime.Now.ToString().Replace(" ", "_").Replace(":", "_").Replace(".", "_");
            using (WordprocessingDocument doc = WordprocessingDocument.Open(filepath, false))
            {
                MainDocumentPart originalMainPart = doc.MainDocumentPart;
                Directory.CreateDirectory(folder);
                WordHelper.CreateWordDocument(folder+"/Rules.docx", this.RulesList, originalMainPart);
                WordHelper.CreateWordDocument(folder+"/TitlePage.docx", this.TitlePageList, originalMainPart);
                WordHelper.CreateWordDocument(folder+"/Assignment.docx", this.AssignmentList, originalMainPart);
                WordHelper.CreateWordDocument(folder+"/MainProvisions.docx", this.MainProvisionsList, originalMainPart);
                WordHelper.CreateWordDocument(folder+"/FacultyConclusion.docx", this.FacultyConclusionList, originalMainPart);
                WordHelper.CreateWordDocument(folder+"/Task.docx", this.TaskList, originalMainPart);
                WordHelper.CreateWordDocument(folder+"/Schedule.docx", this.ScheduleList, originalMainPart);
                WordHelper.CreateWordDocument(folder+"/Evaluation.docx", this.EvaluationList, originalMainPart);
                Console.WriteLine("docx files created");
            }
        }

        public string PrintParsedContent()
        {
            return $"{TitlePage}\n{Assignment}\n{Evaluation}\n{Schedule}\n{FacultyConclusion}\n{Rules}\n{MainProvisions}\n{Task}";
        }
    }
}