namespace XV2SaveEditor;

public sealed class CacTargetPicker : Form
{
    private readonly CheckedListBox slots = new();
    public List<int> SelectedSlots { get; private set; } = new();

    public CacTargetPicker(IEnumerable<XV2Character> characters, IEnumerable<int>? initial = null, string purpose = "bulk operation")
    {
        Text = "Choose CaC Targets"; StartPosition = FormStartPosition.CenterParent; ClientSize = new System.Drawing.Size(500, 430);
        FormBorderStyle = FormBorderStyle.FixedDialog; MinimizeBox = false; MaximizeBox = false;
        HashSet<int> selected = (initial ?? Enumerable.Empty<int>()).ToHashSet();
        Label info = new() { Text = $"Choose which occupied CaCs receive this {purpose}.", Location = new System.Drawing.Point(16, 16), AutoSize = true };
        slots.SetBounds(16, 48, 468, 286); slots.CheckOnClick = true;
        foreach (XV2Character character in characters.Where(x => !x.IsEmpty))
        {
            slots.Items.Add(new Target(character.Slot - 1, $"Slot {character.Slot}  —  {character.Name} · {character.RaceName} · Lv. {character.Level}"), selected.Contains(character.Slot - 1));
        }
        Button all = new() { Text = "All", Location = new System.Drawing.Point(16, 350), Size = new System.Drawing.Size(72, 32) };
        Button none = new() { Text = "None", Location = new System.Drawing.Point(96, 350), Size = new System.Drawing.Size(72, 32) };
        Button okay = new() { Text = "Use selected", Location = new System.Drawing.Point(292, 382), Size = new System.Drawing.Size(110, 32), DialogResult = DialogResult.OK };
        Button cancel = new() { Text = "Cancel", Location = new System.Drawing.Point(410, 382), Size = new System.Drawing.Size(74, 32), DialogResult = DialogResult.Cancel };
        all.Click += (_, _) => { for (int i = 0; i < slots.Items.Count; i++) slots.SetItemChecked(i, true); };
        none.Click += (_, _) => { for (int i = 0; i < slots.Items.Count; i++) slots.SetItemChecked(i, false); };
        okay.Click += (_, _) => { SelectedSlots = slots.CheckedItems.Cast<Target>().Select(x => x.Slot).ToList(); if (SelectedSlots.Count == 0) { DialogResult = DialogResult.None; MessageBox.Show("Choose at least one CaC.", "CaC Targets", MessageBoxButtons.OK, MessageBoxIcon.Information); } };
        Controls.AddRange(new Control[] { info, slots, all, none, okay, cancel }); AcceptButton = okay; CancelButton = cancel; ModernTheme.Apply(this);
    }

    private sealed record Target(int Slot, string Label) { public override string ToString() => Label; }
}
