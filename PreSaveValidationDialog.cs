namespace XV2SaveEditor;

public sealed class PreSaveValidationDialog : Form
{
    private readonly IReadOnlyList<PreSaveIssue> issues;
    private readonly TextBox details;
    public bool SafeRepairsApplied { get; private set; }

    public PreSaveValidationDialog(IReadOnlyList<PreSaveIssue> findings)
    {
        issues = findings;
        int errors = issues.Count(issue => issue.Severity == "Error");
        int warnings = issues.Count - errors;
        int repairable = issues.Count(issue => issue.Repair != null);

        Text = "Pre-save Safety Check";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new System.Drawing.Size(940, 610);
        MinimumSize = new System.Drawing.Size(780, 520);

        Label summary = new()
        {
            Dock = DockStyle.Top,
            Height = 68,
            Padding = new Padding(14, 14, 14, 8),
            Text = errors == 0
                ? $"{warnings} warning(s) found. You may continue saving after reviewing them. {repairable} safe automatic repair(s) available."
                : $"{errors} blocking error(s) and {warnings} warning(s) found. Blocking errors must be corrected before saving."
        };

        Panel actions = new() { Dock = DockStyle.Bottom, Height = 72, Padding = new Padding(14) };
        Button repair = new()
        {
            Name = "btnFixSafeIssues",
            Text = repairable == 0 ? "No safe fixes available" : $"Fix {repairable} safe issue(s)",
            Dock = DockStyle.Left,
            Width = 180,
            Enabled = repairable > 0
        };
        FlowLayoutPanel decisionButtons = new()
        {
            Dock = DockStyle.Right,
            Width = 300,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 0, 0, 0)
        };
        Button proceed = new()
        {
            Name = "btnContinueSaving",
            Text = "Continue saving",
            Size = new System.Drawing.Size(150, 40),
            DialogResult = DialogResult.OK,
            Enabled = errors == 0,
            Margin = new Padding(0, 0, 10, 0)
        };
        Button cancel = new()
        {
            Name = "btnCancelSaving",
            Text = "Cancel",
            Size = new System.Drawing.Size(125, 40),
            DialogResult = DialogResult.Cancel,
            Margin = new Padding(0)
        };
        decisionButtons.Controls.AddRange(new Control[] { proceed, cancel });
        actions.Controls.Add(decisionButtons);
        actions.Controls.Add(repair);

        Panel detailPanel = new() { Dock = DockStyle.Bottom, Height = 125, Padding = new Padding(14, 8, 14, 12) };
        Label detailTitle = new() { Text = "SELECTED FINDING", Dock = DockStyle.Top, Height = 24, ForeColor = ModernTheme.Cyan };
        details = new TextBox
        {
            Name = "txtIssueDetails",
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Text = "Select a finding to see its complete explanation."
        };
        detailPanel.Controls.Add(details);
        detailPanel.Controls.Add(detailTitle);

        ListView list = new()
        {
            Name = "lstPreSaveIssues",
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            HideSelection = false,
            ShowItemToolTips = true,
            MultiSelect = false
        };
        list.Columns.Add("Severity", 90);
        list.Columns.Add("Area", 145);
        list.Columns.Add("Finding", 465);
        list.Columns.Add("Safe repair", 210);
        foreach (PreSaveIssue issue in issues)
        {
            ListViewItem row = new(issue.Severity) { Tag = issue, ToolTipText = issue.Message };
            row.SubItems.Add(issue.Area);
            row.SubItems.Add(issue.Message);
            row.SubItems.Add(issue.SafeRepair ?? "Not available — review manually");
            if (issue.Severity == "Error") row.ForeColor = System.Drawing.Color.OrangeRed;
            list.Items.Add(row);
        }
        list.SelectedIndexChanged += (_, _) => ShowSelectedIssue(list);
        list.DoubleClick += (_, _) =>
        {
            if (list.SelectedItems.Count == 1 && list.SelectedItems[0].Tag is PreSaveIssue issue)
                MessageBox.Show(Describe(issue), "Safety Finding", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };
        if (list.Items.Count > 0) list.Items[0].Selected = true;

        ToolTip tips = new() { ShowAlways = true };
        tips.SetToolTip(repair, repairable == 0
            ? "None of the current findings has a verified automatic repair. Select a row to review it, then continue or cancel."
            : "Applies only the repairs explicitly marked as safe and verified.");

        repair.Click += (_, _) =>
        {
            foreach (PreSaveIssue issue in issues.Where(issue => issue.Repair != null)) issue.Repair!();
            SafeRepairsApplied = true;
            DialogResult = DialogResult.Retry;
            Close();
        };

        Controls.Add(list);
        Controls.Add(detailPanel);
        Controls.Add(actions);
        Controls.Add(summary);
        AcceptButton = proceed;
        CancelButton = cancel;
        ModernTheme.Apply(this);
        ModernTheme.StyleButton(proceed, true);
    }

    private void ShowSelectedIssue(ListView list)
    {
        details.Text = list.SelectedItems.Count == 1 && list.SelectedItems[0].Tag is PreSaveIssue issue
            ? Describe(issue)
            : "Select a finding to see its complete explanation.";
    }

    private static string Describe(PreSaveIssue issue) =>
        $"{issue.Severity} — {issue.Area}{Environment.NewLine}{Environment.NewLine}{issue.Message}{Environment.NewLine}{Environment.NewLine}" +
        (issue.Repair == null
            ? "Automatic repair: Not available. The editor will not guess how to change this value; review it manually or continue if it is intentional."
            : $"Verified automatic repair: {issue.SafeRepair}");
}
