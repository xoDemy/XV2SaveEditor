using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace XV2SaveEditor
{
    public partial class Form1
    {
        private bool dashboardLayoutsConfigured;

        private void ConfigureResponsiveDashboardLayouts()
        {
            if (dashboardLayoutsConfigured) return;
            dashboardLayoutsConfigured = true;

            tabCaCCustomisation.Padding = new Padding(12);
            tabCaCPresets.Padding = new Padding(12);
            LayoutCacCustomisationPage();
            LayoutPresetPage();
            LayoutInventoryPage();
            LayoutQQBangPage();

            tabCaCCustomisation.Resize += (_, _) => LayoutCacCustomisationPage();
            tabCaCPresets.Resize += (_, _) => LayoutPresetPage();
            tabInventory.Resize += (_, _) => { LayoutInventoryPage(); LayoutQQBangPage(); };
        }

        private void LayoutCacCustomisationPage()
        {
            int margin = 14;
            int gap = 16;
            int availableWidth = Math.Max(900, tabCaCCustomisation.ClientSize.Width - margin * 2);
            int availableHeight = Math.Max(560, tabCaCCustomisation.ClientSize.Height - margin * 2);
            int statsWidth = Math.Clamp((int)(availableWidth * 0.38), 400, 470);

            grpCharacter.Location = new Point(margin, margin);
            grpCharacter.Size = new Size(statsWidth, availableHeight);
            grpCharacter.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            grpCharacter.Text = "  CHARACTER & ATTRIBUTES  ";

            grpAppearance.Location = new Point(margin + statsWidth + gap, margin);
            grpAppearance.Size = new Size(availableWidth - statsWidth - gap, availableHeight);
            grpAppearance.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpAppearance.Text = "  APPEARANCE WORKSPACE  ";

            grpSpecialAppearance.Location = new Point(Math.Max(292, grpAppearance.ClientSize.Width - 320), 224);
            grpSpecialAppearance.Size = new Size(300, Math.Min(325, grpAppearance.ClientSize.Height - 240));
            grpSpecialAppearance.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            grpSpecialAppearance.Text = "  SPECIAL TOOLS  ";
            selectiveAppearanceTransfer.Location = new Point(grpSpecialAppearance.Left, grpSpecialAppearance.Top - 42);
            selectiveAppearanceTransfer.Size = new Size(grpSpecialAppearance.Width, 32);
            selectiveAppearanceTransfer.BringToFront();
        }

        private void LayoutPresetPage()
        {
            int margin = 14;
            int gap = 14;
            int width = Math.Max(960, tabCaCPresets.ClientSize.Width - margin * 2);
            int height = Math.Max(590, tabCaCPresets.ClientSize.Height - margin * 2);
            int selectionHeight = 76;
            int toolsHeight = 82;
            int bodyTop = margin + selectionHeight + gap;
            int bodyHeight = height - selectionHeight - toolsHeight - gap * 2;
            int equipmentWidth = Math.Clamp((int)(width * 0.31), 330, 370);
            int skillsWidth = Math.Clamp((int)(width * 0.35), 365, 410);

            grpPresetSelection.Location = new Point(margin, margin);
            grpPresetSelection.Size = new Size(width, selectionHeight);
            grpPresetSelection.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpPresetSelection.Text = "  ACTIVE LOADOUT  ";

            grpPresetEquipment.Location = new Point(margin, bodyTop);
            grpPresetEquipment.Size = new Size(equipmentWidth, bodyHeight);
            grpPresetEquipment.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            grpPresetEquipment.Text = "  EQUIPMENT  ";

            grpPresetSkills.Location = new Point(margin + equipmentWidth + gap, bodyTop);
            grpPresetSkills.Size = new Size(skillsWidth, bodyHeight);
            grpPresetSkills.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            grpPresetSkills.Text = "  SKILL LOADOUT  ";

            int colorsLeft = margin + equipmentWidth + gap + skillsWidth + gap;
            grpPresetColors.Location = new Point(colorsLeft, bodyTop);
            grpPresetColors.Size = new Size(width - (colorsLeft - margin), bodyHeight);
            grpPresetColors.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpPresetColors.Text = "  CLOTHING COLOURS  ";

            grpPresetTools.Location = new Point(margin, margin + height - toolsHeight);
            grpPresetTools.Size = new Size(width, toolsHeight);
            grpPresetTools.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpPresetTools.Text = "  LOADOUT ACTIONS  ";
            LayoutPresetActionButtons();
        }

        private void LayoutPresetActionButtons()
        {
            Button[] buttons = { btnCopyPreset, btnPastePreset, btnCopyOutfit, btnPasteOutfit, btnCopySkills, btnPasteSkills, btnResetPreset, btnStorePresetLibrary, btnImportPresetLibrary, btnExportCac, btnImportCac };
            int gap = 8;
            int usable = Math.Max(900, grpPresetTools.ClientSize.Width - 28);
            int buttonWidth = Math.Max(92, (usable - gap * (buttons.Length - 1)) / buttons.Length);
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Location = new Point(14 + i * (buttonWidth + gap), 29);
                buttons[i].Size = new Size(buttonWidth, 36);
                buttons[i].Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            }
        }

        private void LayoutInventoryPage()
        {
            TabControl? tabs = tabInventory.Controls.OfType<TabControl>().FirstOrDefault();
            if (tabs == null || tabs.TabPages.Count == 0) return;
            TabPage page = tabs.TabPages[0];
            int margin = 12;
            int gap = 14;
            int width = Math.Max(940, page.ClientSize.Width - margin * 2);
            int height = Math.Max(520, page.ClientSize.Height - margin * 2);
            int browserWidth = Math.Clamp((int)(width * 0.64), 610, 760);

            grpInventoryBrowser.Location = new Point(margin, margin);
            grpInventoryBrowser.Size = new Size(browserWidth, height);
            grpInventoryBrowser.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            grpInventoryBrowser.Text = "  INVENTORY BROWSER  ";
            grpInventoryDetails.Location = new Point(margin + browserWidth + gap, margin);
            grpInventoryDetails.Size = new Size(width - browserWidth - gap, height);
            grpInventoryDetails.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpInventoryDetails.Text = "  SELECTED ITEM & BULK ACTIONS  ";

            int actionY = grpInventoryBrowser.ClientSize.Height - 62;
            inventoryConveniencePanel.Location = new Point(14, actionY - 43);
            inventoryConveniencePanel.Size = new Size(grpInventoryBrowser.ClientSize.Width - 28, 38);
            inventoryConveniencePanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            inventoryConveniencePanel.BringToFront();
            lstInventoryItems.Width = grpInventoryBrowser.ClientSize.Width - 40;
            lstInventoryItems.Height = Math.Max(220, inventoryConveniencePanel.Top - lstInventoryItems.Top - 8);
            lstInventoryItems.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Button[] inventoryActions = { btnPartnerKeys, btnInfiniteDragonBalls };
            int actionWidth = Math.Max(150, (grpInventoryBrowser.ClientSize.Width - 60) / 3);
            for (int i = 0; i < inventoryActions.Length; i++)
            {
                inventoryActions[i].Location = new Point(14 + i * (actionWidth + 8), actionY);
                inventoryActions[i].Size = new Size(actionWidth, 36);
                inventoryActions[i].Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            }
        }

        private void LayoutQQBangPage()
        {
            if (grpQQBangEditor == null || grpQQBangEditor.Controls.Count < 2) return;
            Panel? left = grpQQBangEditor.Controls.OfType<Panel>().FirstOrDefault(panel => panel.Dock == DockStyle.Left);
            Panel? right = grpQQBangEditor.Controls.OfType<Panel>().FirstOrDefault(panel => panel.Dock == DockStyle.Fill);
            if (left == null || right == null) return;
            left.Width = Math.Clamp((int)(grpQQBangEditor.ClientSize.Width * 0.52), 500, 650);
            lstQQBangs.Location = new Point(5, 86);
            lstQQBangs.Size = new Size(left.ClientSize.Width - 20, Math.Max(310, left.ClientSize.Height - 105));
            lstQQBangs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtQQBangSearch.Width = left.ClientSize.Width - 125;
            Control? clear = left.Controls.OfType<Button>().FirstOrDefault(button => button.Text == "Clear");
            if (clear != null)
            {
                clear.Left = left.ClientSize.Width - 108;
                clear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            }
        }
    }
}
