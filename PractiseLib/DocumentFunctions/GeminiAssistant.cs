using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office.PowerPoint.Y2021.M06.Main;
using Mscc.GenerativeAI;

namespace SplittingScript.DocumentFunctions
{
    public class GeminiAssistant
    {
        private static GeminiAssistant? _instance;
        private static readonly object _lock = new object();
        
        public GoogleAI GoogleAI { get; }
        public GenerativeModel Model { get; }

        private GeminiAssistant()
        {
            GoogleAI = new GoogleAI(apiKey: Environment.GetEnvironmentVariable("GOOGLE_API_KEY"));
            Model = GoogleAI.GenerativeModel();
        }

        public static GeminiAssistant Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new GeminiAssistant();
                    }
                }
                return _instance;
            }
        }
    }
}