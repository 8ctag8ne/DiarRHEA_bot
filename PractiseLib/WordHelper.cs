using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;

namespace SplittingScript
{
    public static class WordHelper
    {
        public static bool IsBoldText(OpenXmlElement element)
        {
            if (element is DocumentFormat.OpenXml.Wordprocessing.Paragraph paragraph)
            {
                foreach (var run in paragraph.Elements<Run>())
                {
                    if (run.RunProperties?.Bold != null && run.RunProperties.Bold.Val != null && run.RunProperties.Bold.Val != false)
                        return true;
                }
                
                if (paragraph.ParagraphProperties?.ParagraphStyleId != null)
                {
                    string? style = paragraph.ParagraphProperties.ParagraphStyleId.Val?.Value;
                    if (style != null && (style.Equals("Title", StringComparison.OrdinalIgnoreCase) || style.Equals("Subtitle", StringComparison.OrdinalIgnoreCase)))
                        return true;
                }
            }
            
            if (element is Run runElement && runElement.RunProperties?.Bold != null && runElement.RunProperties.Bold.Val != false)
            {
                return true;
            }

            return false;
        }
        public static bool ContainsNonTextContent(OpenXmlElement element)
        {
            return element.Descendants<Drawing>().Any() || // Зображення
                element.Descendants<DocumentFormat.OpenXml.Drawing.Picture>().Any() ||    // Малюнки (w:pict)
                element.Descendants<OleObject>().Any(); // Вбудовані об'єкти (OLE)
        }

        public static void CreateWordDocument(string filePath, List<OpenXmlElement> content, MainDocumentPart originalMainPart)
        {
            using (WordprocessingDocument newDoc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart newMainPart = newDoc.AddMainDocumentPart();
                newMainPart.Document = new Document();
                Body body = new Body();

                body.Append(content);
                newMainPart.Document.Append(body);
                newMainPart.Document.Save();

                // Copy images
                Dictionary<string, string> imageRelMap = CopyImages(originalMainPart, newMainPart);
                // Copy objects (OLE, charts, etc.)
                Dictionary<string, string> objectRelMap = CopyObjects(originalMainPart, newMainPart);
                // Copy drawings
                Dictionary<string, string> drawingsRelMap = CopyDrawings(originalMainPart, newMainPart);

                // Update all relationships
                UpdateDrawingRelationships(newMainPart, imageRelMap, objectRelMap, drawingsRelMap);

                // Save changes
                newMainPart.Document.Save();
            }
        }

        private static Dictionary<string, string> CopyImages(MainDocumentPart originalMainPart, MainDocumentPart newMainPart)
        {
            Dictionary<string, string> imageRelMap = new Dictionary<string, string>();

            foreach (var imagePart in originalMainPart.ImageParts)
            {
                string oldRelId = originalMainPart.GetIdOfPart(imagePart);
                ImagePart newImagePart = newMainPart.AddImagePart(imagePart.ContentType);

                using (Stream oldStream = imagePart.GetStream())
                using (Stream newStream = newImagePart.GetStream(FileMode.Create))
                {
                    oldStream.CopyTo(newStream);
                }

                imageRelMap[oldRelId] = newMainPart.GetIdOfPart(newImagePart);
            }

            return imageRelMap;
        }

        private static Dictionary<string, string> CopyObjects(MainDocumentPart originalMainPart, MainDocumentPart newMainPart)
        {
            Dictionary<string, string> objectRelMap = new Dictionary<string, string>();

            // Copy embedded objects (OLE, Ink, etc.)
            foreach (var embeddedPart in originalMainPart.Parts.Where(p => p.OpenXmlPart is EmbeddedPackagePart))
            {
                EmbeddedPackagePart newEmbeddedPart = newMainPart.AddNewPart<EmbeddedPackagePart>(embeddedPart.OpenXmlPart.ContentType);
                using (Stream oldStream = embeddedPart.OpenXmlPart.GetStream())
                using (Stream newStream = newEmbeddedPart.GetStream(FileMode.Create))
                {
                    oldStream.CopyTo(newStream);
                }

                objectRelMap[originalMainPart.GetIdOfPart(embeddedPart.OpenXmlPart)] = newMainPart.GetIdOfPart(newEmbeddedPart);
            }

            // Copy charts (ChartPart)
            foreach (var chartPart in originalMainPart.Parts.Where(p => p.OpenXmlPart is ChartPart))
            {
                ChartPart newChartPart = newMainPart.AddNewPart<ChartPart>(chartPart.OpenXmlPart.ContentType);
                using (Stream oldStream = chartPart.OpenXmlPart.GetStream())
                using (Stream newStream = newChartPart.GetStream(FileMode.Create))
                {
                    oldStream.CopyTo(newStream);
                }

                objectRelMap[originalMainPart.GetIdOfPart(chartPart.OpenXmlPart)] = newMainPart.GetIdOfPart(newChartPart);
            }

            return objectRelMap;
        }

        private static Dictionary<string, string> CopyDrawings(MainDocumentPart originalMainPart, MainDocumentPart newMainPart)
        {
            Dictionary<string, string> drawingsRelMap = new Dictionary<string, string>();

            // Copy DrawingsPart
            foreach (var drawingsPart in originalMainPart.Parts.Where(p => p.OpenXmlPart is DrawingsPart))
            {
                DrawingsPart newDrawingsPart = newMainPart.AddNewPart<DrawingsPart>(drawingsPart.OpenXmlPart.ContentType);
                using (Stream oldStream = drawingsPart.OpenXmlPart.GetStream())
                using (Stream newStream = newDrawingsPart.GetStream(FileMode.Create))
                {
                    oldStream.CopyTo(newStream);
                }

                drawingsRelMap[originalMainPart.GetIdOfPart(drawingsPart.OpenXmlPart)] = newMainPart.GetIdOfPart(newDrawingsPart);
            }

            return drawingsRelMap;
        }

        private static void UpdateDrawingRelationships(MainDocumentPart newMainPart, Dictionary<string, string> imageRelMap, Dictionary<string, string> objectRelMap, Dictionary<string, string> drawingsRelMap)
        {
            // Update image references
            foreach (var blip in newMainPart.Document.Body.Descendants<Blip>())
            {
                if (blip.Embed != null && imageRelMap.ContainsKey(blip.Embed.Value))
                {
                    blip.Embed.Value = imageRelMap[blip.Embed.Value];
                }
            }

            // Update object references (OLE, charts, etc.)
            foreach (var graphicData in newMainPart.Document.Body.Descendants<DocumentFormat.OpenXml.Drawing.GraphicData>())
            {
                var element = graphicData.GetFirstChild<OpenXmlElement>();
                if (element != null)
                {
                    try
                    {
                        var idAttr = element.GetAttribute("id", ""); // Check for "id" attribute
                        if (!string.IsNullOrEmpty(idAttr.Value) && objectRelMap.ContainsKey(idAttr.Value))
                        {
                            element.SetAttribute(new OpenXmlAttribute("id", "", objectRelMap[idAttr.Value]));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            // Update drawing references
            foreach (var drawing in newMainPart.Document.Body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Drawing>())
            {
                var inline = drawing.GetFirstChild<DocumentFormat.OpenXml.Drawing.Wordprocessing.Inline>();
                if (inline != null)
                {
                    var graphic = inline.GetFirstChild<DocumentFormat.OpenXml.Drawing.Graphic>();
                    if (graphic != null)
                    {
                        var graphicData = graphic.GetFirstChild<DocumentFormat.OpenXml.Drawing.GraphicData>();
                        if (graphicData != null)
                        {
                            var element = graphicData.GetFirstChild<OpenXmlElement>();
                            if (element != null)
                            {
                                try
                                {
                                    var idAttr = element.GetAttribute("id", ""); // Check for "id" attribute
                                    if (!string.IsNullOrEmpty(idAttr.Value) && drawingsRelMap.ContainsKey(idAttr.Value))
                                    {
                                        element.SetAttribute(new OpenXmlAttribute("id", "", drawingsRelMap[idAttr.Value]));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static bool ContainsSignature(OpenXmlElement element)
        {
            return element.Descendants<DocumentFormat.OpenXml.Wordprocessing.Drawing>().Any() ||
                element.Descendants<DocumentFormat.OpenXml.Wordprocessing.Picture>().Any() ||
                element.Descendants<DocumentFormat.OpenXml.Vml.Shape>().Any() ||
                element.Descendants<DocumentFormat.OpenXml.Vml.ImageData>().Any();
        }

        public static string RemoveImagesAndGetText(OpenXmlElement element)
        {
            var clonedElement = element.CloneNode(true);
            foreach (var drawing in clonedElement.Descendants<DocumentFormat.OpenXml.Wordprocessing.Drawing>().ToList())
            {
                drawing.Remove();
            }
            foreach (var picture in clonedElement.Descendants<DocumentFormat.OpenXml.Wordprocessing.Picture>().ToList())
            {
                picture.Remove();
            }
            return clonedElement.InnerText;
        }
    }
}