using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Ulko
{
    public static class ExcelToJson
    {
        [MenuItem("Assets/Import Excel Data")]
        public static void ManualConvert()
        {
            Convert(false);
        }

        [InitializeOnEnterPlayMode]
        public static void AutoConvert()
        {
            Convert(true);
        }

        public static void Convert(bool hidden)
        {
            var executablePath = Path.Combine(Application.dataPath, @"..\ExcelExporter.exe");
            var inputPath = Path.Combine(Application.dataPath, "Excel");
            var outputPath = Path.Combine(Application.dataPath, @"Game\Design\FromExcel");

            ProcessStartInfo processInfo = new ProcessStartInfo();

            processInfo.UseShellExecute = true;
            if (hidden) processInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processInfo.FileName = executablePath;
            processInfo.Arguments = inputPath +" "+ outputPath;

            var process = Process.Start(processInfo);
            process.WaitForExit();

            AssetDatabase.Refresh();
        }
    }
}
