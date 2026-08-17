using Microsoft.VisualBasic.FileIO;

namespace XV2SaveEditor;

public sealed class CacLibraryBrowser : Form
{
    private readonly TextBox search = new();
    private readonly ComboBox race = new();
    private readonly ListView list = new();
    private readonly Label counts = new();
    private List<CacLibrary.Entry> entries = new();

    public string? SelectedPath { get; private set; }

    public CacLibraryBrowser()
    {
        Text = "CaC Library";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new System.Drawing.Size(900, 600);
        MinimumSize = new System.Drawing.Size(760, 500);

        Panel filters = new() { Dock = DockStyle.Top, Height = 74, Padding = new Padding(14) };
        search.SetBounds(14, 30, 300, 27); search.PlaceholderText = "Search name or filename";
        race.SetBounds(330, 30, 190, 27); race.DropDownStyle = ComboBoxStyle.DropDownList;
        race.Items.Add("All races"); race.Items.AddRange(CacLibrary.Races.Cast<object>().ToArray()); race.SelectedIndex = 0;
        counts.SetBounds(540, 34, 330, 24);
        filters.Controls.Add(new Label { Text = "SEARCH AND FILTER", Location = new System.Drawing.Point(14, 8), AutoSize = true, ForeColor = ModernTheme.Cyan });
        filters.Controls.AddRange(new Control[] { search, race, counts });

        list.Dock = DockStyle.Fill; list.View = View.Details; list.FullRowSelect = true; list.MultiSelect = false; list.HideSelection = false;
        list.Columns.Add("Name", 250); list.Columns.Add("Race", 180); list.Columns.Add("Level", 80); list.Columns.Add("Modified", 155); list.Columns.Add("File", 210);
        list.ColumnClick += (_, e) => SortColumn(e.Column);
        list.DoubleClick += (_, _) => ImportSelected();

        Panel actions = new() { Dock = DockStyle.Bottom, Height = 66, Padding = new Padding(14) };
        Button import = MakeButton("Import to selected slot", 14, (_, _) => ImportSelected(), 180);
        Button rename = MakeButton("Rename", 208, (_, _) => RenameSelected());
        Button delete = MakeButton("Delete", 342, (_, _) => DeleteSelected());
        Button folder = MakeButton("Open folder", 476, (_, _) => CacLibrary.OpenFolder(CacLibrary.RootPath));
        Button close = MakeButton("Close", 766, (_, _) => Close()); close.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        actions.Controls.AddRange(new Control[] { import, rename, delete, folder, close });

        Controls.Add(list); Controls.Add(actions); Controls.Add(filters);
        search.TextChanged += (_, _) => RefreshList(); race.SelectedIndexChanged += (_, _) => RefreshList();
        ModernTheme.Apply(this);
        Reload();
    }

    private static Button MakeButton(string text, int x, EventHandler click, int width = 120)
    {
        Button button = new() { Text = text, Location = new System.Drawing.Point(x, 15), Size = new System.Drawing.Size(width, 34) };
        button.Click += click; return button;
    }

    private void Reload() { entries = CacLibrary.ReadEntries(); RefreshList(); }

    private void RefreshList()
    {
        if (list == null) return;
        string query = search.Text.Trim(); int selectedRace = race.SelectedIndex - 1;
        IEnumerable<CacLibrary.Entry> filtered = entries.Where(entry =>
            (selectedRace < 0 || entry.Race == selectedRace) &&
            (query.Length == 0 || entry.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || Path.GetFileName(entry.Path).Contains(query, StringComparison.OrdinalIgnoreCase)));
        list.BeginUpdate(); list.Items.Clear();
        foreach (CacLibrary.Entry entry in filtered.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase))
        {
            ListViewItem item = new(entry.Name) { Tag = entry };
            item.SubItems.Add(entry.RaceName); item.SubItems.Add(entry.Level.ToString()); item.SubItems.Add(entry.Modified.ToString("g")); item.SubItems.Add(Path.GetFileName(entry.Path));
            list.Items.Add(item);
        }
        list.EndUpdate(); counts.Text = $"{list.Items.Count} visible / {entries.Count} stored";
    }

    private CacLibrary.Entry? SelectedEntry() => list.SelectedItems.Count == 1 ? list.SelectedItems[0].Tag as CacLibrary.Entry : null;

    private void ImportSelected()
    {
        CacLibrary.Entry? entry = SelectedEntry(); if (entry == null) return;
        SelectedPath = entry.Path; DialogResult = DialogResult.OK; Close();
    }

    private void RenameSelected()
    {
        CacLibrary.Entry? entry = SelectedEntry(); if (entry == null) return;
        string? name = AskName(entry.Name); if (string.IsNullOrWhiteSpace(name)) return;
        string safe = string.Concat(name.Trim().Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch));
        string target = Path.Combine(Path.GetDirectoryName(entry.Path)!, safe + ".excac");
        if (File.Exists(target)) { MessageBox.Show("A library file with that name already exists.", "CaC Library", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        File.Move(entry.Path, target); Reload();
    }

    private void DeleteSelected()
    {
        CacLibrary.Entry? entry = SelectedEntry(); if (entry == null) return;
        if (MessageBox.Show($"Move {entry.Name} to the Recycle Bin?", "CaC Library", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        FileSystem.DeleteFile(entry.Path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin); Reload();
    }

    private string? AskName(string current)
    {
        using Form prompt = new() { Text = "Rename Library CaC", StartPosition = FormStartPosition.CenterParent, ClientSize = new System.Drawing.Size(420, 130), FormBorderStyle = FormBorderStyle.FixedDialog, MinimizeBox = false, MaximizeBox = false };
        TextBox value = new() { Text = current, Location = new System.Drawing.Point(14, 35), Size = new System.Drawing.Size(392, 27) };
        Button okay = new() { Text = "Rename", Location = new System.Drawing.Point(238, 82), Size = new System.Drawing.Size(80, 32), DialogResult = DialogResult.OK };
        Button cancel = new() { Text = "Cancel", Location = new System.Drawing.Point(326, 82), Size = new System.Drawing.Size(80, 32), DialogResult = DialogResult.Cancel };
        prompt.Controls.Add(new Label { Text = "Library filename", Location = new System.Drawing.Point(14, 12), AutoSize = true }); prompt.Controls.AddRange(new Control[] { value, okay, cancel });
        prompt.AcceptButton = okay; prompt.CancelButton = cancel; ModernTheme.Apply(prompt); value.SelectAll();
        return prompt.ShowDialog(this) == DialogResult.OK ? value.Text : null;
    }

    private void SortColumn(int column)
    {
        List<CacLibrary.Entry> visible = list.Items.Cast<ListViewItem>().Select(item => (CacLibrary.Entry)item.Tag!).ToList();
        visible = column switch { 1 => visible.OrderBy(x => x.RaceName).ThenBy(x => x.Name).ToList(), 2 => visible.OrderByDescending(x => x.Level).ThenBy(x => x.Name).ToList(), 3 => visible.OrderByDescending(x => x.Modified).ToList(), 4 => visible.OrderBy(x => Path.GetFileName(x.Path)).ToList(), _ => visible.OrderBy(x => x.Name).ToList() };
        list.BeginUpdate(); list.Items.Clear();
        foreach (var entry in visible) { ListViewItem item = new(entry.Name) { Tag = entry }; item.SubItems.Add(entry.RaceName); item.SubItems.Add(entry.Level.ToString()); item.SubItems.Add(entry.Modified.ToString("g")); item.SubItems.Add(Path.GetFileName(entry.Path)); list.Items.Add(item); }
        list.EndUpdate();
    }
}
