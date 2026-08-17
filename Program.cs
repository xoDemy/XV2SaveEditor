namespace XV2SaveEditor
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            if (args.Contains("--release-self-test", StringComparer.OrdinalIgnoreCase))
            {
                Environment.ExitCode = RunReleaseSelfTest();
                return;
            }
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (_, args) => ReportUnhandled(args.Exception);
            AppDomain.CurrentDomain.UnhandledException += (_, args) => ReportUnhandled(args.ExceptionObject as Exception ?? new Exception("Unknown fatal error"));
            TaskScheduler.UnobservedTaskException += (_, args) => { ReportUnhandled(args.Exception); args.SetObserved(); };
            Application.Run(new Form1());
        }

        private static int RunReleaseSelfTest()
        {
            try
            {
                string baseDirectory = AppContext.BaseDirectory;
                string converterDirectory = Path.Combine(baseDirectory, "Tools", "PlatformConverters");
                if (!File.Exists(Path.Combine(baseDirectory, "AesCtrLibrary.dll"))) return 11;
                if (!Directory.Exists(converterDirectory) || Directory.GetFiles(converterDirectory, "*.exe").Length < 2) return 12;
                string[] resourceNames = typeof(Program).Assembly.GetManifestResourceNames();
                if (!resourceNames.Any(name => name.Contains("Data.Inventory", StringComparison.Ordinal))) return 13;
                if (!resourceNames.Any(name => name.Contains("Data.Skills", StringComparison.Ordinal))) return 14;
                if (!resourceNames.Any(name => name.Contains("Data.Progress", StringComparison.Ordinal))) return 15;
                if (!resourceNames.Contains("XV2SaveEditor.AppIcon.ico", StringComparer.Ordinal)) return 21;
                using Form1 form = new();
                form.CreateControl();
                if (form.Icon == null) return 22;
                using PreSaveValidationDialog validation = new(new[]
                {
                    new PreSaveIssue("Warning", "Self-test", "A long non-repairable finding used to verify the responsive safety dialog layout.", null, null)
                });
                validation.CreateControl();
                validation.PerformLayout();
                Button? proceed = validation.Controls.Find("btnContinueSaving", true).OfType<Button>().FirstOrDefault();
                Button? repair = validation.Controls.Find("btnFixSafeIssues", true).OfType<Button>().FirstOrDefault();
                TextBox? details = validation.Controls.Find("txtIssueDetails", true).OfType<TextBox>().FirstOrDefault();
                if (proceed?.Parent == null || proceed.Right > proceed.Parent.ClientSize.Width || proceed.Bottom > proceed.Parent.ClientSize.Height || !proceed.Enabled) return 16;
                if (repair == null || repair.Enabled) return 17;
                if (details == null || !details.ReadOnly || !details.Multiline) return 18;
                XV2Preset emptyPreset = new() { Top = 1, Bottom = 2, Gloves = 3, Shoes = 4, Accessory = 5, SuperSoul = 6, QQBang = 7, SuperSkill1 = 8, TopColor1 = 42 };
                PresetWriter.ApplyVerifiedEmptyLoadout(emptyPreset);
                if (emptyPreset.Top != -1 || emptyPreset.SuperSoul != -1 || emptyPreset.SuperSkill1 != -1 || emptyPreset.AwokenSkill != -1 || emptyPreset.QQBang != PresetWriter.VerifiedEmptyQQBang || emptyPreset.TopColor1 != 42) return 19;
                return 0;
            }
            catch
            {
                return 20;
            }
        }

        private static void ReportUnhandled(Exception exception)
        {
            string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "XV2SaveEditor", "Crash Reports");
            string logPath = Path.Combine(logDirectory, $"crash-{DateTime.Now:yyyyMMdd}.log");
            try { Directory.CreateDirectory(logDirectory); File.AppendAllText(logPath, $"[{DateTime.Now:O}]\nVersion: {Application.ProductVersion}\nOS: {Environment.OSVersion}\n{exception}\n\n"); } catch { }
            MessageBox.Show(
                $"The editor recovered from an unexpected error:\n\n{exception.Message}\n\nA diagnostic report was written to:\n{logPath}",
                "XV2 Save Editor - Recovered Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
