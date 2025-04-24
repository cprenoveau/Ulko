using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelExporter
{
    class Program
    {
        public static void Main(string[] args)
        {
            string inputPath = Directory.GetCurrentDirectory();
            string outputPath = Directory.GetCurrentDirectory();

            if (args.Length > 0)
                inputPath = args[0];

            if (args.Length > 1)
                outputPath = args[1];

            var inputDir = new DirectoryInfo(inputPath);

            Excel.Application excelApp = new Excel.Application();

            FileInfo[] files = inputDir.GetFiles("*.xlsx");
            foreach (var file in files)
            {
                if (file.Name[0] == '~')
                    continue;

                string excelFile = Path.GetFileNameWithoutExtension(file.Name);

                var sizeInBytes = file.Length;
                var historyFilename = Path.Combine(inputPath, excelFile + "_export.txt");
                if(File.Exists(historyFilename))
                {
                    string content = File.ReadAllText(historyFilename);
                    if (content == sizeInBytes.ToString())
                        continue;
                }

                File.WriteAllText(historyFilename, sizeInBytes.ToString());

                ExportExcelFile(excelApp, file.FullName, excelFile, outputPath);
            }

            excelApp.Quit();
        }

        private static void ExportExcelFile(Excel.Application excelApp, string fullFileName, string fileName, string outputPath)
        {
            Excel.Workbook book = null;

            try
            {
                Console.WriteLine("Exporting " + fullFileName + " to " + outputPath);

                book = excelApp.Workbooks.Open(fullFileName);
                Excel.Worksheet sheet = book.Worksheets.get_Item(1);

                var (isTrue, folder, soloTokens) = ExportAll(sheet);
                if(isTrue)
                {
                    if (!string.IsNullOrEmpty(folder))
                    {
                        outputPath = Path.Combine(outputPath, folder);
                    }

                    if (!Directory.Exists(outputPath))
                        Directory.CreateDirectory(outputPath);

                    for (int i = 2; i <= book.Worksheets.Count; ++i)
                    {
                        if (!SkipExport(book.Worksheets[i]))
                        {
                            if (soloTokens.Length > 0)
                            {
                                foreach(string token in soloTokens)
                                {
                                    File.WriteAllText(Path.Combine(outputPath, fileName + "_" + book.Worksheets[i].Name + "_" + token + ".json"), ReadSheet(book.Worksheets[i], book, token, soloTokens).ToString());
                                }
                            }
                            else
                            {
                                File.WriteAllText(Path.Combine(outputPath, fileName + "_" + book.Worksheets[i].Name + ".json"), ReadSheet(book.Worksheets[i], book).ToString());
                            }
                        }
                    }
                }
                else
                {
                    if (!Directory.Exists(outputPath))
                        Directory.CreateDirectory(outputPath);

                    File.WriteAllText(Path.Combine(outputPath, fileName + ".json"), ReadSheet(sheet, book).ToString());
                }
            }
            catch (Exception e)
            {
                Console.Write("ExportExcelFile: Exception " + e.Message);
            }

            if (book != null)
                book.Close(false);
        }

        private static bool SkipExport(Excel.Worksheet sheet)
        {
            string key = sheet.Cells[1, 1].Value;
            return key == "*skip_export";
        }

        private static (bool isTrue, string folder, string[] soloTokens) ExportAll(Excel.Worksheet sheet)
        {
            string key = sheet.Cells[1, 1].Value;
            string soloTokensStr = sheet.Cells[3, 1].Value;
            return (key == "export_all_sheets", sheet.Cells[2,1].Value, !string.IsNullOrEmpty(soloTokensStr) ? soloTokensStr.Split(',') : new string[0]);
        }

        private static JToken ReadSheet(Excel.Worksheet sheet, Excel.Workbook book, string soloToken = null, string[] allSoloTokens = null)
        {
            JArray jArray = new JArray();
            Excel.Range range = sheet.UsedRange;

            for (int r = 2; r <= range.Rows.Count; ++r)
            {
                if (range.Cells[r, 1].Value == null && range.Cells[r, 2].Value == null)
                    break;

                JObject dict = new JObject();

                for (int c = 1; c <= range.Columns.Count; ++c)
                {
                    string label = (range.Cells[1, c] as Excel.Range).Value;
                    var value = (range.Cells[r, c] as Excel.Range).Value;

                    if (string.IsNullOrEmpty(label))
                        break;

                    if (label[0] == '*' || (soloToken != null && label != soloToken && allSoloTokens.Contains(label)))
                        continue;

                    if (value is string)
                    {
                        var vTokens = value.Split(':');
                        if (vTokens.Length > 1 && vTokens[0] == "sheet")
                        {
                            Excel.Worksheet subSheet = book.Worksheets[vTokens[1]];
                            dict.Add(label, ReadSheet(subSheet, book));
                        }
                        else
                        {
                            dict.Add(label, value);
                        }
                    }
                    else
                    {
                        dict.Add(label, value);
                    }
                }

                jArray.Add(dict);
            }

            return jArray;
        }
    }
}
