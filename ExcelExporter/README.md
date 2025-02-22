# ExcelExporter
Simple C# Excel to JSON exporter<br>

Exports .xlsx files from an input folder (arg0) into an output folder in JSON format (arg1)<br><br>

Format looks like this:<br>
In Excel:<br>
>       key_1,   key_2,   key_3,               *ignore_me
>       value_1, value_2, sheet:my_sheet_name, ignored_value
>       ...

First row contains keys.<br>
Subsequent rows contain values.<br>

Adding "sheet:" in front of a value refers to a worksheet name. It will fetch the content of the worksheet and append it as an array. It works recursively, so nested sheets can also contain nested sheets.<br>
Adding \* in front of a key will prevent it from being exported (useful for adding descriptions required only in the Excel document)<br>

Output:<br>
>        [
>            {
>                "key_1": value_1,
>                "key_2": value_2,
>                "key_3":
>                [
>                    "key_1": value_1,
>                    "key_2": value_2
>                ]
>            },
>            ...
>        ]

In order to export all sheets from a document, type export_all_sheets in the first cell of the first sheet and on the second row, type the relative path (relative to outputPath) of the folder you would like your sheets to be exported into.
If you want to ignore a sheet, type skip_export in the first cell of the sheet you want to ignore.

Usage from within the Unity Editor:<br>

        public static void Convert(bool hidden)
        {
            var executablePath = Path.Combine(Application.dataPath, @"..\ExcelExporter.exe");
            var inputPath = Path.Combine(Application.dataPath, "Excel");
            var outputPath = Path.Combine(Application.dataPath, "Resources");

            ProcessStartInfo processInfo = new ProcessStartInfo();

            processInfo.UseShellExecute = true;
            if (hidden) processInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processInfo.FileName = executablePath;
            processInfo.Arguments = inputPath +" "+ outputPath;

            var process = Process.Start(processInfo);
            process.WaitForExit();
        }
