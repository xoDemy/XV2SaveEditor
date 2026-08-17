using System.Diagnostics;

namespace XV2LivePrototype;

internal sealed class MainForm : Form
{
    private static readonly Color Window = Color.FromArgb(8, 11, 20);
    private static readonly Color Surface = Color.FromArgb(17, 21, 36);
    private static readonly Color Raised = Color.FromArgb(25, 30, 49);
    private static readonly Color TextColor = Color.FromArgb(242, 245, 255);
    private static readonly Color Muted = Color.FromArgb(147, 157, 185);
    private static readonly Color Purple = Color.FromArgb(143, 93, 255);
    private static readonly Color Cyan = Color.FromArgb(54, 214, 231);

    private readonly Label status = new();
    private readonly Label processInfo = new();
    private readonly Label moduleInfo = new();
    private readonly Label safetyInfo = new();
    private readonly Button attach = new();
    private readonly Button detach = new();
    private readonly Button refresh = new();
    private readonly ListView processes = new();
    private readonly NumericUpDown scanValue = new();
    private readonly Button firstScan = new();
    private readonly Button nextScan = new();
    private readonly Button exportResults = new();
    private readonly ListView scanResults = new();
    private readonly Label scanStatus = new();
    private readonly TextBox watchAddress = new();
    private readonly Label watchValue = new();
    private readonly Button watchCandidate = new();
    private readonly Button findPointers = new();
    private nint watchedAddress;
    private List<nint> candidates = new();
    private List<PointerCandidate> pointerCandidates = new();
    private readonly System.Windows.Forms.Timer monitor = new() { Interval = 1000 };
    private GameProcessSession? session;

    public MainForm()
    {
        Text = "XV2 Live Editor Prototype — Read Only";
        ClientSize = new Size(920, 610);
        MinimumSize = new Size(820, 560);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Window;
        ForeColor = TextColor;
        Font = new Font("Segoe UI", 9.5F);

        Panel header = new() { Dock = DockStyle.Top, Height = 94, BackColor = Surface };
        header.Controls.Add(new Label { Text = "XV2 LIVE LINK", AutoSize = true, Location = new Point(24, 17), Font = new Font("Segoe UI Semibold", 18F), ForeColor = TextColor });
        header.Controls.Add(new Label { Text = "READ-ONLY PROCESS RESEARCH PROTOTYPE", AutoSize = true, Location = new Point(27, 56), Font = new Font("Segoe UI Semibold", 8F), ForeColor = Cyan });
        status.Text = "DETACHED"; status.AutoSize = false; status.TextAlign = ContentAlignment.MiddleCenter; status.Location = new Point(720, 27); status.Size = new Size(170, 34); status.Anchor = AnchorStyles.Top | AnchorStyles.Right; status.BackColor = Raised; status.ForeColor = Muted;
        header.Controls.Add(status);

        Panel controls = new() { Dock = DockStyle.Top, Height = 70, Padding = new Padding(18) };
        refresh.Text = "Scan for game"; refresh.Location = new Point(18, 18); refresh.Size = new Size(145, 34);
        attach.Text = "Attach read-only"; attach.Location = new Point(175, 18); attach.Size = new Size(155, 34);
        detach.Text = "Detach"; detach.Location = new Point(342, 18); detach.Size = new Size(120, 34);
        controls.Controls.AddRange(new Control[] { refresh, attach, detach });

        processes.Dock = DockStyle.Left; processes.Width = 340; processes.View = View.Details; processes.FullRowSelect = true; processes.HideSelection = false; processes.BackColor = Surface; processes.ForeColor = TextColor;
        processes.Columns.Add("Process", 145); processes.Columns.Add("PID", 75); processes.Columns.Add("EAC", 130);
        TabControl workspace = new() { Dock = DockStyle.Fill };
        TabPage connectionPage = new("Connection") { BackColor = Surface };
        TabPage scannerPage = new("Value Scanner") { BackColor = Surface };
        Panel details = new() { Dock = DockStyle.Fill, Padding = new Padding(24), BackColor = Surface };
        processInfo.Location = new Point(24, 28); processInfo.Size = new Size(460, 90); processInfo.Text = "No game process selected.";
        moduleInfo.Location = new Point(24, 135); moduleInfo.Size = new Size(460, 150); moduleInfo.ForeColor = Muted;
        safetyInfo.Location = new Point(24, 320); safetyInfo.Size = new Size(460, 95); safetyInfo.ForeColor = Cyan; safetyInfo.Text = "Safety mode\nOnly PROCESS_QUERY_LIMITED_INFORMATION and PROCESS_VM_READ are requested. This prototype contains no write-memory API.";
        details.Controls.AddRange(new Control[] { processInfo, moduleInfo, safetyInfo });
        connectionPage.Controls.Add(details);
        ConfigureScanner(scannerPage);
        workspace.TabPages.Add(connectionPage); workspace.TabPages.Add(scannerPage);

        Controls.Add(workspace); Controls.Add(processes); Controls.Add(controls); Controls.Add(header);
        StyleButton(refresh, false); StyleButton(attach, true); StyleButton(detach, false);
        refresh.Click += (_, _) => ScanProcesses(); attach.Click += (_, _) => AttachSelected(); detach.Click += (_, _) => Detach();
        processes.SelectedIndexChanged += (_, _) => UpdateSelection();
        monitor.Tick += (_, _) => MonitorSession();
        FormClosed += (_, _) => Detach();
        ScanProcesses(); monitor.Start();
    }

    private void ConfigureScanner(TabPage page)
    {
        page.AutoScroll = true;
        page.Controls.Add(new Label { Text = "Known Int32 value", Location = new Point(20, 20), AutoSize = true });
        scanValue.Location = new Point(20, 46); scanValue.Size = new Size(190, 26); scanValue.Minimum = int.MinValue; scanValue.Maximum = int.MaxValue; scanValue.ThousandsSeparator = true;
        firstScan.Text = "First scan"; firstScan.Location = new Point(225, 43); firstScan.Size = new Size(120, 32);
        nextScan.Text = "Refine scan"; nextScan.Location = new Point(355, 43); nextScan.Size = new Size(120, 32);
        exportResults.Text = "Export"; exportResults.Location = new Point(485, 43); exportResults.Size = new Size(100, 32);
        scanStatus.Text = "Attach to the game before scanning."; scanStatus.Location = new Point(20, 86); scanStatus.AutoSize = true; scanStatus.ForeColor = Muted;
        scanResults.Location = new Point(20, 118); scanResults.Size = new Size(515, 230); scanResults.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; scanResults.View = View.Details; scanResults.FullRowSelect = true; scanResults.BackColor = Window; scanResults.ForeColor = TextColor;
        scanResults.Columns.Add("Address", 190); scanResults.Columns.Add("Module relative", 210); scanResults.Columns.Add("Value", 100);
        firstScan.Click += async (_, _) => await FirstScanAsync(); nextScan.Click += async (_, _) => await RefineScanAsync(); exportResults.Click += (_, _) => ExportScanResults();
        StyleButton(firstScan, true); StyleButton(nextScan, false); StyleButton(exportResults, false);
        watchAddress.Location = new Point(20, 365); watchAddress.Size = new Size(215, 26); watchAddress.PlaceholderText = "Address, e.g. 0x25C7ACA80F4";
        watchCandidate.Text = "Watch address"; watchCandidate.Location = new Point(245, 362); watchCandidate.Size = new Size(125, 32);
        findPointers.Text = "Find next pointer level"; findPointers.Location = new Point(20, 405); findPointers.Size = new Size(300, 36);
        watchValue.Text = "No address being watched."; watchValue.Location = new Point(20, 452); watchValue.Size = new Size(520, 48); watchValue.ForeColor = Cyan; watchValue.Font = new Font("Segoe UI Semibold", 10F);
        watchCandidate.Click += (_, _) => StartWatchingAddress();
        findPointers.Click += async (_, _) => await FindPointersAsync();
        scanResults.DoubleClick += (_, _) => { if (scanResults.SelectedItems.Count > 0) { watchAddress.Text = scanResults.SelectedItems[0].Text; StartWatchingAddress(); } };
        StyleButton(watchCandidate, false); StyleButton(findPointers, false);
        page.Controls.AddRange(new Control[] { scanValue, firstScan, nextScan, exportResults, scanStatus, scanResults, watchAddress, watchCandidate, findPointers, watchValue });
        void LayoutScanner()
        {
            int watcherTop = Math.Max(365, page.ClientSize.Height - 145);
            scanResults.Size = new Size(Math.Max(420, page.ClientSize.Width - 40), Math.Max(180, watcherTop - 132));
            watchAddress.Location = new Point(20, watcherTop);
            watchCandidate.Location = new Point(245, watcherTop - 3);
            findPointers.Location = new Point(20, watcherTop + 43);
            watchValue.Location = new Point(335, watcherTop + 43);
            watchValue.Size = new Size(Math.Max(250, page.ClientSize.Width - 355), 48);
            watchAddress.BringToFront(); watchCandidate.BringToFront(); findPointers.BringToFront(); watchValue.BringToFront();
        }
        page.Resize += (_, _) => LayoutScanner();
        LayoutScanner();
        UpdateScannerButtons();
    }

    private async Task FirstScanAsync()
    {
        if (session == null) return;
        pointerCandidates.Clear();
        SetScanning(true, "Scanning readable game memory…");
        try { candidates = (await Task.Run(() => session.ScanInt32((int)scanValue.Value))).ToList(); ShowScanResults(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Scan Failed", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { SetScanning(false, $"{candidates.Count:N0} candidate address(es). Change the value in-game, enter the new value, then refine."); }
    }

    private async Task RefineScanAsync()
    {
        if (session == null || candidates.Count == 0) return;
        pointerCandidates.Clear();
        SetScanning(true, "Refining previous candidates…");
        try { candidates = (await Task.Run(() => session.RefineInt32(candidates, (int)scanValue.Value))).ToList(); ShowScanResults(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Scan Failed", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { SetScanning(false, $"{candidates.Count:N0} candidate address(es) remain."); }
    }

    private void ShowScanResults()
    {
        scanResults.BeginUpdate(); scanResults.Items.Clear();
        if (session != null) foreach (nint address in candidates.Take(5000))
        {
            long relative = address - session.ModuleBase;
            ListViewItem item = new($"0x{address:X}"); item.SubItems.Add(relative >= 0 && relative < session.ModuleSize ? $"DBXV2.exe+0x{relative:X}" : "Dynamic / heap"); item.SubItems.Add(scanValue.Value.ToString("N0")); scanResults.Items.Add(item);
        }
        scanResults.EndUpdate();
    }

    private void ShowPointerResults()
    {
        scanResults.BeginUpdate(); scanResults.Items.Clear();
        if (session == null) { scanResults.EndUpdate(); return; }
        foreach (PointerCandidate candidate in pointerCandidates.Take(5000))
        {
            ListViewItem item = new($"0x{candidate.Address:X}");
            item.SubItems.Add(candidate.Address >= session.ModuleBase && candidate.Address < session.ModuleBase + session.ModuleSize
                ? $"DBXV2.exe+0x{candidate.Address - session.ModuleBase:X}" : "Dynamic / heap");
            item.SubItems.Add($"L{candidate.Level} +0x{candidate.Offset:X}");
            scanResults.Items.Add(item);
        }
        scanResults.EndUpdate();
    }

    private void ExportScanResults()
    {
        if (session == null || (candidates.Count == 0 && pointerCandidates.Count == 0)) return;
        using SaveFileDialog dialog = new() { Filter = "Text files (*.txt)|*.txt", FileName = "XV2_scan_candidates.txt" };
        if (dialog.ShowDialog() != DialogResult.OK) return;
        if (pointerCandidates.Count > 0)
            File.WriteAllLines(dialog.FileName, pointerCandidates.Select(candidate => $"L{candidate.Level}\t0x{candidate.Address:X}\t+0x{candidate.Offset:X}\t-> 0x{candidate.Target:X}\t{(candidate.Address - session.ModuleBase >= 0 && candidate.Address - session.ModuleBase < session.ModuleSize ? $"DBXV2.exe+0x{candidate.Address - session.ModuleBase:X}" : "dynamic")}"));
        else
            File.WriteAllLines(dialog.FileName, candidates.Select(address => $"0x{address:X}\t{(address - session.ModuleBase >= 0 && address - session.ModuleBase < session.ModuleSize ? $"DBXV2.exe+0x{address - session.ModuleBase:X}" : "dynamic")}"));
    }

    private void StartWatchingAddress()
    {
        string value = watchAddress.Text.Trim();
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) value = value[2..];
        if (!long.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out long parsed))
        {
            MessageBox.Show("Enter a valid hexadecimal memory address.", "Live Watch", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        watchedAddress = (nint)parsed;
        RefreshWatchedValue();
        UpdateScannerButtons();
    }

    private void RefreshWatchedValue()
    {
        if (session == null || watchedAddress == 0) { watchValue.Text = "No address being watched."; return; }
        try
        {
            int value = BitConverter.ToInt32(session.ReadBytes(watchedAddress, 4));
            watchValue.Text = $"Live Int32: {value:N0}   •   Address: 0x{watchedAddress:X}";
            watchValue.ForeColor = value == (int)scanValue.Value ? Cyan : TextColor;
        }
        catch { watchValue.Text = "Address is no longer readable — it may have moved."; watchValue.ForeColor = Color.FromArgb(255, 120, 135); }
    }

    private async Task FindPointersAsync()
    {
        if (session == null || watchedAddress == 0) return;
        SetScanning(true, $"Searching for pointers to 0x{watchedAddress:X}…");
        try
        {
            pointerCandidates = (await Task.Run(() => session.ScanPointerPaths(watchedAddress, maximumOffset: 0x400, maximumLevels: 1, maximumResults: 5000))).ToList();
            candidates.Clear();
            ShowPointerResults();
            scanStatus.Text = $"{pointerCandidates.Count:N0} next-level candidate(s), using offsets up to 0x400. Double-click one to continue upward.";
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Pointer Scan Failed", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { UseWaitCursor = false; UpdateScannerButtons(); }
    }

    private void SetScanning(bool scanning, string message) { scanStatus.Text = message; firstScan.Enabled = !scanning && session != null; nextScan.Enabled = !scanning && session != null && candidates.Count > 0; exportResults.Enabled = !scanning && (candidates.Count > 0 || pointerCandidates.Count > 0); findPointers.Enabled = !scanning && session != null && watchedAddress != 0; UseWaitCursor = scanning; }
    private void UpdateScannerButtons() { firstScan.Enabled = session != null; nextScan.Enabled = session != null && candidates.Count > 0; exportResults.Enabled = candidates.Count > 0 || pointerCandidates.Count > 0; findPointers.Enabled = session != null && watchedAddress != 0; }

    private void ScanProcesses()
    {
        int selectedPid = processes.SelectedItems.Count > 0 && processes.SelectedItems[0].Tag is int selected ? selected : -1;
        processes.Items.Clear();
        bool eac = IsEacRunning();
        foreach (Process process in Process.GetProcesses().Where(p => p.ProcessName.Equals("DBXV2", StringComparison.OrdinalIgnoreCase)))
        {
            ListViewItem item = new(process.ProcessName + ".exe") { Tag = process.Id };
            item.SubItems.Add(process.Id.ToString()); item.SubItems.Add(eac ? "Detected — blocked" : "Not detected");
            if (eac) item.ForeColor = Color.FromArgb(255, 120, 135);
            processes.Items.Add(item);
            if (process.Id == selectedPid) item.Selected = true;
            process.Dispose();
        }
        if (processes.Items.Count > 0 && processes.SelectedItems.Count == 0) processes.Items[0].Selected = true;
        UpdateButtons();
    }

    private void UpdateSelection()
    {
        if (processes.SelectedItems.Count == 0) { processInfo.Text = "No game process selected."; return; }
        if (processes.SelectedItems[0].Tag is not int pid) return;
        processInfo.Text = $"Dragon Ball Xenoverse 2 detected\nProcess ID: {pid}\nEAC status: {(IsEacRunning() ? "Detected — attachment disabled" : "Not detected")}";
        UpdateButtons();
    }

    private void AttachSelected()
    {
        if (IsEacRunning()) { MessageBox.Show("Easy Anti-Cheat appears to be active. The prototype will not attach.", "Attachment Blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (processes.SelectedItems.Count == 0) return;
        try
        {
            Detach();
            if (processes.SelectedItems[0].Tag is not int pid) return;
            session = GameProcessSession.Attach(Process.GetProcessById(pid));
            status.Text = "READ-ONLY ATTACHED"; status.ForeColor = Cyan;
            moduleInfo.Text = $"Executable\n{session.ExecutablePath}\n\nVersion: {session.Version}\nModule base: 0x{session.ModuleBase.ToInt64():X}\nModule size: {session.ModuleSize:N0} bytes";
            candidates.Clear(); ShowScanResults(); scanStatus.Text = "Enter a known value such as Zeni, TP Medals, level, or XP.";
        }
        catch (Exception ex) { Detach(); MessageBox.Show(ex.Message, "Attach Failed", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        UpdateButtons();
    }

    private void MonitorSession() { if (session != null && !session.IsAttached) Detach(); else if (session == null) ScanProcesses(); else RefreshWatchedValue(); }
    private void Detach() { session?.Dispose(); session = null; candidates.Clear(); watchedAddress = 0; status.Text = "DETACHED"; status.ForeColor = Muted; moduleInfo.Text = ""; scanStatus.Text = "Attach to the game before scanning."; watchValue.Text = "No address being watched."; ShowScanResults(); UpdateButtons(); }
    private void UpdateButtons() { bool eac = IsEacRunning(); attach.Enabled = session == null && processes.SelectedItems.Count > 0 && !eac; detach.Enabled = session != null; refresh.Enabled = session == null; UpdateScannerButtons(); }
    private static bool IsEacRunning() => Process.GetProcesses().Any(p => { try { return p.ProcessName.Contains("EasyAntiCheat", StringComparison.OrdinalIgnoreCase) || p.ProcessName.Equals("start_protected_game", StringComparison.OrdinalIgnoreCase); } finally { p.Dispose(); } });
    private static void StyleButton(Button button, bool primary) { button.FlatStyle = FlatStyle.Flat; button.FlatAppearance.BorderSize = primary ? 0 : 1; button.FlatAppearance.BorderColor = Color.FromArgb(48, 55, 80); button.BackColor = primary ? Purple : Raised; button.ForeColor = TextColor; button.Cursor = Cursors.Hand; }
}
