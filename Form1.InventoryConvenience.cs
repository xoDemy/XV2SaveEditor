using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace XV2SaveEditor
{
    public partial class Form1
    {
        private readonly HashSet<string> favouriteInventoryItems = new();
        private readonly Queue<string> recentInventoryEdits = new();
        private Panel inventoryConveniencePanel = null!;

        private void ConfigureInventoryConvenienceTools()
        {
            inventoryConveniencePanel = new Panel { Location = new System.Drawing.Point(14, 394), Size = new System.Drawing.Size(610, 38), Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom };
            Button favourite = new Button { Text = "★ Favourite", Location = new System.Drawing.Point(0, 4), Size = new System.Drawing.Size(115, 30) };
            Button showFavourites = new Button { Text = "Show favourites", Location = new System.Drawing.Point(125, 4), Size = new System.Drawing.Size(135, 30) };
            Button visibleQuantity = new Button { Text = "Set visible quantity", Location = new System.Drawing.Point(270, 4), Size = new System.Drawing.Size(160, 30) };
            Button recent = new Button { Text = "Recent edits", Location = new System.Drawing.Point(440, 4), Size = new System.Drawing.Size(120, 30) };
            favourite.Click += (_, _) => ToggleInventoryFavourite();
            showFavourites.Click += (_, _) => ShowInventoryFavourites();
            visibleQuantity.Click += (_, _) => SetVisibleInventoryQuantity();
            recent.Click += (_, _) => ShowRecentInventoryEdits();
            inventoryConveniencePanel.Controls.AddRange(new Control[] { favourite, showFavourites, visibleQuantity, recent });
            grpInventoryBrowser.Controls.Add(inventoryConveniencePanel);
            inventoryConveniencePanel.BringToFront();
        }

        private string InventoryKey(InventoryDisplayItem item) => $"{cmbInventoryCategory.SelectedIndex}:{item.ID}:{item.Type}";

        private void ToggleInventoryFavourite()
        {
            if (lstInventoryItems.SelectedItem is not InventoryDisplayItem item) return;
            string key = InventoryKey(item);
            if (!favouriteInventoryItems.Add(key)) favouriteInventoryItems.Remove(key);
            MessageBox.Show(favouriteInventoryItems.Contains(key) ? $"Added {item.Name} to favourites." : $"Removed {item.Name} from favourites.", "Inventory Favourites", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowInventoryFavourites()
        {
            if (favouriteInventoryItems.Count == 0) { MessageBox.Show("No inventory favourites have been added in this session.", "Inventory Favourites", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            string text = string.Join("\n", currentInventoryDisplayItems.Where(item => favouriteInventoryItems.Contains(InventoryKey(item))).Select(item => $"• {item.DisplayName}"));
            MessageBox.Show(text.Length == 0 ? "No favourites are present in the current category/filter." : text, "Inventory Favourites", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SetVisibleInventoryQuantity()
        {
            if (currentSave == null || currentInventoryDisplayItems.Count == 0) return;
            byte quantity = (byte)nudInventoryQuantity.Value;
            if (MessageBox.Show($"Set {currentInventoryDisplayItems.Count} visible owned item(s) to quantity {quantity}?", "Set Visible Quantity", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            int offset = GetSelectedInventorySectionOffset();
            foreach (InventoryDisplayItem display in currentInventoryDisplayItems) InventoryWriter.WriteQuantity(currentSave.DecryptedData, offset, display.Item, quantity);
            RecordRecentInventoryEdit($"Set {currentInventoryDisplayItems.Count} visible items to {quantity}");
            MarkUnsaved(); RefreshInventoryList();
        }

        private void RecordRecentInventoryEdit(string description)
        {
            recentInventoryEdits.Enqueue($"{DateTime.Now:HH:mm:ss}  {description}");
            while (recentInventoryEdits.Count > 20) recentInventoryEdits.Dequeue();
        }

        private void ShowRecentInventoryEdits() => MessageBox.Show(recentInventoryEdits.Count == 0 ? "No inventory bulk edits in this session." : string.Join("\n", recentInventoryEdits.Reverse()), "Recent Inventory Edits", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
