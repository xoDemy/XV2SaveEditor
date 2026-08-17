using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace XV2SaveEditor
{
    public partial class Form1 : Form
    {
        private SaveFile? currentSave;


        private readonly Xv2NameDatabase
            nameDatabase =
                new Xv2NameDatabase();


        private readonly Dictionary<
            ComboBox,
            NamedValueKind>
            namedComboKinds =
                new Dictionary<
                    ComboBox,
                    NamedValueKind>();


        private readonly Dictionary<
            ComboBox,
            NamedSaveValue?>
            lastValidNamedSelections =
                new Dictionary<
                    ComboBox,
                    NamedSaveValue?>();


        // =========================================================
        // INVENTORY DISPLAY CACHE
        // =========================================================

        private readonly List<InventoryDisplayItem>
            currentInventoryDisplayItems =
                new List<InventoryDisplayItem>();

        private readonly List<XV2QQBang> currentQQBangs = new List<XV2QQBang>();
        private readonly Dictionary<int, XV2QQBang> originalQQBangs = new Dictionary<int, XV2QQBang>();
        private XV2QQBang? copiedQQBang;
        private bool isLoadingQQBangControls;
        private GroupBox grpQQBangEditor = null!;
        private ListBox lstQQBangs = null!;
        private TextBox txtQQBangSearch = null!;
        private Label lblQQBangCount = null!;
        private Label lblQQBangMetadata = null!;
        private NumericUpDown[] nudQQBangStats = null!;
        private NumericUpDown nudQQBangQuantity = null!;
        private Button btnQQBangApply = null!;
        private Button btnQQBangCopy = null!;
        private Button btnQQBangPaste = null!;
        private Button btnQQBangRevert = null!;


        // =========================================================
        // ORIGINAL PRESET SNAPSHOTS
        // =========================================================

        private readonly Dictionary<
            (int Slot, int Preset),
            XV2Preset>
            originalPresetSnapshots =
                new Dictionary<
                    (int Slot, int Preset),
                    XV2Preset>();


        private bool nameDatabaseLoaded = false;

        private bool isLoadingControls = false;
        private bool isLoadingPresetControls = false;
        private bool isSynchronizingCharacters = false;
        private bool isSearchingNamedCombo = false;

        private bool hasUnsavedChanges = false;

        private readonly List<XV2SkillOwnership> skillOwnership = new List<XV2SkillOwnership>();
        private byte[]? skillOwnershipSnapshot;
        private TextBox txtSkillSearch = null!;
        private ComboBox cmbSkillView = null!;
        private CheckedListBox lstSkillOwnership = null!;
        private TabControl tabSkillCategories = null!;
        private readonly Dictionary<NamedValueKind, CheckedListBox> skillCategoryLists = new Dictionary<NamedValueKind, CheckedListBox>();
        private Label lblSkillCounts = null!;
        private Button btnUnlockSelected = null!;
        private Button btnUnlockVisible = null!;
        private Button btnUnlockAllSkills = null!;
        private Button btnRevertSkills = null!;
        private bool isLoadingSkills;
        private readonly List<XV2QuestProgress> questProgress = new List<XV2QuestProgress>();
        private byte[]? questProgressSnapshot;
        private ComboBox cmbQuestCharacter = null!;
        private ComboBox cmbQuestCategory = null!;
        private ComboBox cmbQuestView = null!;
        private TextBox txtQuestSearch = null!;
        private ListBox lstQuestProgress = null!;
        private Label lblQuestCounts = null!;
        private NumericUpDown nudQuestScore = null!;
        private ComboBox cmbQuestRank = null!;
        private TabPage questProgressPage = null!;
        private readonly ProgressReferenceDatabase progressReferences = new ProgressReferenceDatabase();
        private List<TokipediaProgressEntry> tokipediaProgress = new List<TokipediaProgressEntry>();
        private ComboBox cmbProgressCharacter = null!;
        private ListBox lstTokipedia = null!;
        private CheckedListBox lstTokipediaPaths = null!;
        private bool isLoadingTokipediaPaths;
        private TextBox txtSystemFlagSearch = null!;
        private ListView lstSystemFlags = null!;
        private readonly List<Button> navigationButtons = new List<Button>();
        private Label lblModernTitle = null!;
        private Label lblModernSubtitle = null!;
        private Button btnImportCac = null!;
        private Button btnExportCac = null!;
        private Button btnEmptyAllPresets = null!;
        private Button btnPartnerKeys = null!;
        private Button btnInfiniteDragonBalls = null!;
        private bool enforceInfiniteDragonBalls;
        private byte[]? progressionSnapshot;
        private ComboBox cmbMentorCharacter = null!;
        private TextBox txtMentorSearch = null!;
        private ListBox lstMentorGauges = null!;
        private Label lblMentorCounts = null!;
        private NumericUpDown nudMentorFriendship = null!;
        private NumericUpDown nudMentorDual = null!;
        private ComboBox cmbCollectionCategory = null!;
        private ComboBox cmbCollectionView = null!;
        private TextBox txtCollectionSearch = null!;
        private CheckedListBox lstCollectionUnlocks = null!;
        private Label lblCollectionCounts = null!;
        private bool isLoadingProgression;
        private ComboBox cmbPartnerCharacter = null!, cmbPartnerStatType = null!;
        private TextBox txtPartnerSearch = null!;
        private ListBox lstPartnerStats = null!;
        private Label lblPartnerCounts = null!;
        private TabPage tabMentorCustomisation = null!;

        private int rememberedPresetIndex = 0;


        private XV2Character? copiedStats = null;
        private XV2Appearance? copiedAppearance = null;
        private XV2Character? loadedCharacter = null;
        private int loadedCharacterSlotIndex = -1;


        // =========================================================
        // PRESET CLIPBOARDS
        // =========================================================

        private XV2Preset? copiedPreset = null;

        private XV2Preset? copiedOutfit = null;

        private XV2Preset? copiedSkills = null;


        private string copiedPresetSource = "";
        private string copiedOutfitSource = "";
        private string copiedSkillsSource = "";


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public Form1()
        {
            InitializeComponent();
            ApplyApplicationIcon();


            // Dark mode retired for performance.
            btnDarkMode.Visible =
                false;


            nudZeni.ValueChanged +=
                AnyValueChanged;

            nudTPMedals.ValueChanged +=
                AnyValueChanged;


            nudCharacterLevel.Maximum =
                LevelExperience.MaximumLevel;


            BuildNamedComboMap();

            AttachPresetChangeHandlers();

            ConfigureSearchableNamedCombos();


            KeyPreview =
                true;

            KeyDown +=
                Form1_KeyDown;

            FormClosing += Form1_FormClosing;


            TryLoadNameDatabase();

            progressReferences.Load();

            PopulateAllNamedComboLists();

            ConfigureInventoryEditor();

            ConfigureQQBangEditor();

            ConfigureSkillUnlockEditor();

            ConfigurePlayDataEditor();

            ConfigureCharacterTransferButtons();

            ConfigureCacManagementHub();

            ConfigureInvisibilityTools();

            ConfigurePresetLibrary();

            ConfigureDiagnosticsHub();

            ConfigureChangeHistory();

            ConfigureBackupRecovery();

            ConfigureAdvancedTransferTools();

            ConfigureInventoryConvenienceTools();

            ConfigureSaveComparison();

            ConfigureMaxOutWizard();

            ConfigurePlatformExport();

            ConfigureSaveDashboard();

            ConfigureDragAndDrop();

            ConfigureModernInterface();

            Shown += Form1ReleaseShown;


            SetEditorEnabled(
                false
            );


            ResetPresetClipboardState();

            UpdateWindowTitle();
        }


        // =========================================================
        // NAMED COMBO MAP
        // =========================================================

        private void BuildNamedComboMap()
        {
            namedComboKinds.Clear();


            namedComboKinds[
                cmbPresetTop
            ] = NamedValueKind.Top;


            namedComboKinds[
                cmbPresetBottom
            ] = NamedValueKind.Bottom;


            namedComboKinds[
                cmbPresetGloves
            ] = NamedValueKind.Gloves;


            namedComboKinds[
                cmbPresetShoes
            ] = NamedValueKind.Shoes;


            namedComboKinds[
                cmbPresetAccessory
            ] = NamedValueKind.Accessory;


            namedComboKinds[
                cmbPresetSuperSoul
            ] = NamedValueKind.SuperSoul;


            namedComboKinds[
                cmbSuperSkill1
            ] = NamedValueKind.SuperSkill;


            namedComboKinds[
                cmbSuperSkill2
            ] = NamedValueKind.SuperSkill;


            namedComboKinds[
                cmbSuperSkill3
            ] = NamedValueKind.SuperSkill;


            namedComboKinds[
                cmbSuperSkill4
            ] = NamedValueKind.SuperSkill;


            namedComboKinds[
                cmbUltimateSkill1
            ] = NamedValueKind.UltimateSkill;


            namedComboKinds[
                cmbUltimateSkill2
            ] = NamedValueKind.UltimateSkill;


            namedComboKinds[
                cmbEvasiveSkill
            ] = NamedValueKind.EvasiveSkill;


            namedComboKinds[
                cmbAwokenSkill
            ] = NamedValueKind.AwokenSkill;
        }


        private IEnumerable<ComboBox> GetNamedComboBoxes()
        {
            return namedComboKinds.Keys;
        }


        // =========================================================
        // KEYBOARD SHORTCUTS
        // =========================================================

        private void Form1_KeyDown(
            object? sender,
            KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z)
            {
                e.SuppressKeyPress = true;
                UndoHistory();
                return;
            }

            if (e.Control && e.KeyCode == Keys.Y)
            {
                e.SuppressKeyPress = true;
                RedoHistory();
                return;
            }

            if (
                e.Control &&
                e.KeyCode == Keys.O)
            {
                e.SuppressKeyPress =
                    true;


                btnOpenSave_Click(
                    btnOpenSave,
                    EventArgs.Empty
                );


                return;
            }


            if (
                e.Control &&
                e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress =
                    true;


                if (btnSave.Enabled)
                {
                    btnSave_Click(
                        btnSave,
                        EventArgs.Empty
                    );
                }
            }
        }


        // =========================================================
        // NAME DATABASE
        // =========================================================

        private void TryLoadNameDatabase()
        {
            try
            {
                nameDatabase.LoadFromEmbeddedInventoryCatalogs();

                string gameDataPath =
                    Path.Combine(
                        AppContext.BaseDirectory,
                        "GameData"
                    );


                if (
                    !Directory.Exists(
                        gameDataPath
                    ))
                {
                    nameDatabaseLoaded =
                        true;


                    lblNameDatabaseStatus.Text =
                        "Names: Inventory catalogues loaded";


                    return;
                }


                lblNameDatabaseStatus.Text =
                    "Names: Loading...";


                nameDatabase.LoadFromGameData(
                    gameDataPath,
                    clearExisting: false
                );


                nameDatabaseLoaded =
                    true;


                lblNameDatabaseStatus.Text =
                    "Names: Loaded ✓";
            }
            catch (Exception ex)
            {
                nameDatabaseLoaded =
                    false;


                lblNameDatabaseStatus.Text =
                    "Names: Could not load";


                MessageBox.Show(
                    "The GameData folder was found, but the " +
                    "name database could not be loaded.\n\n" +

                    "Error:\n" +
                    ex.Message,

                    "XV2 Name Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }


        // =========================================================
        // POPULATE NAMED LISTS ONCE
        // =========================================================

        private void PopulateAllNamedComboLists()
        {
            bool previousState =
                isLoadingPresetControls;


            isLoadingPresetControls =
                true;


            try
            {
                foreach (
                    KeyValuePair<
                        ComboBox,
                        NamedValueKind>
                    pair
                    in namedComboKinds)
                {
                    ComboBox combo =
                        pair.Key;


                    NamedValueKind kind =
                        pair.Value;


                    combo.BeginUpdate();


                    try
                    {
                        combo.Items.Clear();


                        if (nameDatabaseLoaded)
                        {
                            foreach (
                                NamedSaveValue value
                                in nameDatabase.GetValues(
                                    kind
                                ))
                            {
                                combo.Items.Add(
                                    value
                                );
                            }
                        }


                        combo.SelectedIndex =
                            -1;


                        lastValidNamedSelections[
                            combo
                        ] = null;
                    }
                    finally
                    {
                        combo.EndUpdate();
                    }
                }
            }
            finally
            {
                isLoadingPresetControls =
                    previousState;
            }
        }


        // =========================================================
        // SEARCHABLE COMBOS
        // =========================================================

        private void ConfigureSearchableNamedCombos()
        {
            foreach (
                ComboBox combo
                in GetNamedComboBoxes())
            {
                combo.DropDownStyle =
                    ComboBoxStyle.DropDown;


                combo.AutoCompleteMode =
                    AutoCompleteMode.SuggestAppend;


                combo.AutoCompleteSource =
                    AutoCompleteSource.ListItems;


                combo.MaxDropDownItems =
                    15;


                combo.IntegralHeight =
                    true;


                combo.KeyDown +=
                    SearchableCombo_KeyDown;


                combo.Validating +=
                    SearchableCombo_Validating;


                combo.DropDown +=
                    SearchableCombo_DropDown;


                lastValidNamedSelections[
                    combo
                ] = null;
            }
        }


        private void SearchableCombo_DropDown(
            object? sender,
            EventArgs e)
        {
            if (
                sender is not ComboBox combo)
            {
                return;
            }


            if (
                combo.DropDownWidth <
                500)
            {
                combo.DropDownWidth =
                    500;
            }
        }


        private void SearchableCombo_KeyDown(
            object? sender,
            KeyEventArgs e)
        {
            if (
                sender is not ComboBox combo ||
                e.KeyCode != Keys.Enter)
            {
                return;
            }


            e.SuppressKeyPress =
                true;


            string searchText =
                combo.Text.Trim();


            if (
                string.IsNullOrWhiteSpace(
                    searchText
                ))
            {
                return;
            }


            NamedSaveValue? result =
                FindNamedComboMatch(
                    combo,
                    searchText
                );


            if (result == null)
            {
                System.Media
                    .SystemSounds
                    .Beep
                    .Play();

                return;
            }


            isSearchingNamedCombo =
                true;


            combo.SelectedItem =
                result;


            combo.DroppedDown =
                false;


            lastValidNamedSelections[
                combo
            ] = result;


            isSearchingNamedCombo =
                false;


            StorePresetControlsInSelectedPreset();

            MarkUnsaved();
        }


        private void SearchableCombo_Validating(
            object? sender,
            System.ComponentModel.CancelEventArgs e)
        {
            if (
                sender is not ComboBox combo)
            {
                return;
            }


            if (
                isLoadingPresetControls ||
                isSearchingNamedCombo)
            {
                return;
            }


            if (
                combo.SelectedItem
                is NamedSaveValue validSelection)
            {
                lastValidNamedSelections[
                    combo
                ] = validSelection;


                return;
            }


            NamedSaveValue? typedMatch =
                FindNamedComboMatch(
                    combo,
                    combo.Text
                );


            if (typedMatch != null)
            {
                isSearchingNamedCombo =
                    true;


                combo.SelectedItem =
                    typedMatch;


                lastValidNamedSelections[
                    combo
                ] = typedMatch;


                isSearchingNamedCombo =
                    false;


                StorePresetControlsInSelectedPreset();

                MarkUnsaved();


                return;
            }


            if (
                lastValidNamedSelections
                    .TryGetValue(
                        combo,
                        out NamedSaveValue? previous
                    ) &&
                previous != null)
            {
                isSearchingNamedCombo =
                    true;


                combo.SelectedItem =
                    previous;


                isSearchingNamedCombo =
                    false;
            }
        }


        private static NamedSaveValue? FindNamedComboMatch(
            ComboBox combo,
            string searchText)
        {
            if (
                string.IsNullOrWhiteSpace(
                    searchText
                ))
            {
                return null;
            }


            string query =
                searchText.Trim();


            IEnumerable<NamedSaveValue> entries =
                combo.Items
                    .OfType<NamedSaveValue>();


            if (
                int.TryParse(
                    query,
                    out int requestedId
                ))
            {
                NamedSaveValue? idMatch =
                    entries.FirstOrDefault(
                        x =>
                            x.SaveId ==
                            requestedId
                    );


                if (idMatch != null)
                {
                    return idMatch;
                }
            }


            string digitsOnly =
                new string(
                    query
                        .Where(
                            char.IsDigit
                        )
                        .ToArray()
                );


            if (
                digitsOnly.Length > 0 &&
                query.IndexOf(
                    "id",
                    StringComparison.OrdinalIgnoreCase
                ) >= 0 &&
                int.TryParse(
                    digitsOnly,
                    out int embeddedId
                ))
            {
                NamedSaveValue? embeddedMatch =
                    entries.FirstOrDefault(
                        x =>
                            x.SaveId ==
                            embeddedId
                    );


                if (embeddedMatch != null)
                {
                    return embeddedMatch;
                }
            }


            NamedSaveValue? exactName =
                entries.FirstOrDefault(
                    x =>
                        string.Equals(
                            x.Name,
                            query,
                            StringComparison.OrdinalIgnoreCase
                        ) ||
                        string.Equals(
                            x.DisplayName,
                            query,
                            StringComparison.OrdinalIgnoreCase
                        )
                );


            if (exactName != null)
            {
                return exactName;
            }


            NamedSaveValue? startsWith =
                entries.FirstOrDefault(
                    x =>
                        x.Name.StartsWith(
                            query,
                            StringComparison.OrdinalIgnoreCase
                        )
                );


            if (startsWith != null)
            {
                return startsWith;
            }


            return entries.FirstOrDefault(
                x =>
                    x.Name.IndexOf(
                        query,
                        StringComparison.OrdinalIgnoreCase
                    ) >= 0
            );
        }


        // =========================================================
        // PRESET CHANGE EVENTS
        // =========================================================

        private void AttachPresetChangeHandlers()
        {
            foreach (
                ComboBox field
                in GetNamedComboBoxes())
            {
                field.SelectedIndexChanged +=
                    PresetNamedValueChanged;
            }


            NumericUpDown[] numericFields =
            {
                nudPresetQQBang,
                nudBlastSkill,

                nudTopColor1,
                nudTopColor2,
                nudTopColor3,
                nudTopColor4,

                nudBottomColor1,
                nudBottomColor2,
                nudBottomColor3,
                nudBottomColor4,

                nudGlovesColor1,
                nudGlovesColor2,
                nudGlovesColor3,
                nudGlovesColor4,

                nudShoesColor1,
                nudShoesColor2,
                nudShoesColor3,
                nudShoesColor4
            };


            foreach (
                NumericUpDown field
                in numericFields)
            {
                field.ValueChanged +=
                    PresetValueChanged;
            }
        }


        // =========================================================
        // INVENTORY EDITOR
        // =========================================================

        private bool isLoadingInventoryControls =
            false;


        private void ConfigureInventoryEditor()
        {
            isLoadingInventoryControls =
                true;


            try
            {
                btnPartnerKeys = new Button { Text = "Customization Keys 1–20", Location = new System.Drawing.Point(20, 440), Size = new System.Drawing.Size(195, 34) };
                btnInfiniteDragonBalls = new Button { Text = "Infinite Dragon Balls", Location = new System.Drawing.Point(225, 440), Size = new System.Drawing.Size(175, 34) };
                btnPartnerKeys.Click += (_, _) => GivePartnerKeys();
                btnInfiniteDragonBalls.Click += (_, _) => GiveInfiniteDragonBalls();
                grpInventoryBrowser.Controls.AddRange(new Control[] { btnPartnerKeys, btnInfiniteDragonBalls });
                // Keep the owned-items list above the convenience action row.
                // The favourites buttons begin at Y=405.
                lstInventoryItems.Height = 295;
                if (
                    cmbInventoryCategory.Items.Count > 0)
                {
                    cmbInventoryCategory.SelectedIndex =
                        0;
                }


                txtInventorySearch.Text =
                    "";


                chkInventoryBulkFiltered.Checked =
                    false;


                lblInventoryCount.Text =
                    "Items: 0";


                nudInventoryQuantity.Minimum =
                    0;


                nudInventoryQuantity.Maximum =
                    255;


                nudInventoryQuantity.Value =
                    0;


                ClearInventoryDetails();

                UpdateInventoryButtonState();
            }
            finally
            {
                isLoadingInventoryControls =
                    false;
            }
        }


        // =========================================================
        // INVENTORY CATEGORY CHANGED
        // =========================================================

        private void cmbInventoryCategory_SelectedIndexChanged(
            object? sender,
            EventArgs e)
        {
            if (isLoadingInventoryControls)
            {
                return;
            }


            RefreshInventoryList();
        }


        // =========================================================
        // INVENTORY SEARCH
        // =========================================================

        private void txtInventorySearch_TextChanged(
            object? sender,
            EventArgs e)
        {
            if (isLoadingInventoryControls)
            {
                return;
            }


            RefreshInventoryList();
        }


        private void btnInventoryClearSearch_Click(
            object? sender,
            EventArgs e)
        {
            txtInventorySearch.Clear();
            txtInventorySearch.Focus();
        }


        // =========================================================
        // GET CURRENT INVENTORY CATEGORY
        // =========================================================

        private List<XV2InventoryItem>
            GetSelectedInventoryCategory()
        {
            if (currentSave == null)
            {
                return new List<XV2InventoryItem>();
            }


            XV2Inventory inventory =
                currentSave.Inventory;


            return cmbInventoryCategory.SelectedIndex switch
            {
                0 =>
                    inventory.Tops,

                1 =>
                    inventory.Bottoms,

                2 =>
                    inventory.Gloves,

                3 =>
                    inventory.Shoes,

                4 =>
                    inventory.Accessories,

                5 =>
                    inventory.SuperSouls,

                6 =>
                    inventory.MixItems,

                7 =>
                    inventory.ImportantItems,

                8 =>
                    inventory.Capsules,

                _ =>
                    new List<XV2InventoryItem>()
            };
        }


        // =========================================================
        // GET INVENTORY SECTION OFFSET
        // =========================================================

        private int GetSelectedInventorySectionOffset()
        {
            return cmbInventoryCategory.SelectedIndex switch
            {
                0 =>
                    InventoryReader.TopsOffset,

                1 =>
                    InventoryReader.BottomsOffset,

                2 =>
                    InventoryReader.GlovesOffset,

                3 =>
                    InventoryReader.ShoesOffset,

                4 =>
                    InventoryReader.AccessoriesOffset,

                5 =>
                    InventoryReader.SuperSoulsOffset,

                6 =>
                    InventoryReader.MixItemsOffset,

                7 =>
                    InventoryReader.ImportantItemsOffset,

                8 =>
                    InventoryReader.CapsulesOffset,

                _ =>
                    throw new InvalidOperationException(
                        "No valid inventory category is selected."
                    )
            };
        }


        // =========================================================
        // GET INVENTORY NAME TYPE
        // =========================================================

        private NamedValueKind?
            GetSelectedInventoryNameKind()
        {
            return cmbInventoryCategory.SelectedIndex switch
            {
                0 =>
                    NamedValueKind.Top,

                1 =>
                    NamedValueKind.Bottom,

                2 =>
                    NamedValueKind.Gloves,

                3 =>
                    NamedValueKind.Shoes,

                4 =>
                    NamedValueKind.Accessory,

                5 =>
                    NamedValueKind.SuperSoul,

                6 =>
                    NamedValueKind.MixItem,

                7 =>
                    NamedValueKind.ImportantItem,

                8 =>
                    NamedValueKind.Capsule,

                _ =>
                    null
            };
        }


        // =========================================================
        // REFRESH INVENTORY LIST
        // =========================================================

        private void RefreshInventoryList()
        {
            int previousId =
                -1;


            int previousSlot =
                -1;


            if (
                lstInventoryItems.SelectedItem
                is InventoryDisplayItem previousSelection)
            {
                previousId =
                    previousSelection.ID;


                previousSlot =
                    previousSelection.SlotIndex;
            }


            lstInventoryItems.BeginUpdate();


            try
            {
                lstInventoryItems.Items.Clear();

                currentInventoryDisplayItems.Clear();


                if (currentSave == null)
                {
                    lblInventoryCount.Text =
                        "Items: 0";


                    ClearInventoryDetails();

                    UpdateInventoryButtonState();

                    return;
                }


                List<XV2InventoryItem> source =
                    GetSelectedInventoryCategory();


                NamedValueKind? nameKind =
                    GetSelectedInventoryNameKind();


                string search =
                    txtInventorySearch.Text
                        .Trim();


                InventoryDisplayItem? selectionToRestore =
                    null;


                foreach (
                    XV2InventoryItem item
                    in source)
                {
                    string name =
                        ResolveInventoryItemName(
                            item,
                            nameKind
                        );


                    InventoryDisplayItem display =
                        new InventoryDisplayItem
                        {
                            Item =
                                item,

                            Name =
                                name
                        };


                    if (
                        !InventorySearchMatches(
                            display,
                            search
                        ))
                    {
                        continue;
                    }


                    currentInventoryDisplayItems.Add(
                        display
                    );


                    lstInventoryItems.Items.Add(
                        display
                    );


                    if (
                        item.ID == previousId &&
                        item.SlotIndex == previousSlot)
                    {
                        selectionToRestore =
                            display;
                    }
                }


                lblInventoryCount.Text =
                    currentInventoryDisplayItems.Count == source.Count
                        ? $"Owned items: {source.Count}"
                        : $"Showing: {currentInventoryDisplayItems.Count} of {source.Count} owned";


                if (selectionToRestore != null)
                {
                    lstInventoryItems.SelectedItem =
                        selectionToRestore;
                }
                else if (
                    lstInventoryItems.Items.Count > 0)
                {
                    lstInventoryItems.SelectedIndex =
                        0;
                }
                else
                {
                    ClearInventoryDetails();
                }


                UpdateInventoryButtonState();
            }
            finally
            {
                lstInventoryItems.EndUpdate();
            }
        }


        // =========================================================
        // INVENTORY NAME RESOLUTION
        // =========================================================

        private string ResolveInventoryItemName(
            XV2InventoryItem item,
            NamedValueKind? kind)
        {
            if (
                kind.HasValue &&
                nameDatabaseLoaded)
            {
                if (
                    nameDatabase.TryGetValue(
                        kind.Value,
                        item.ID,
                        out NamedSaveValue? namedValue
                    ) &&
                    namedValue != null)
                {
                    return namedValue.Name;
                }


                return
                    "Unknown / Modded";
            }


            return
                "Raw Item";
        }


        // =========================================================
        // INVENTORY SEARCH MATCH
        // =========================================================

        private static bool InventorySearchMatches(
            InventoryDisplayItem display,
            string search)
        {
            if (
                string.IsNullOrWhiteSpace(
                    search
                ))
            {
                return true;
            }


            if (
                display.Name.IndexOf(
                    search,
                    StringComparison.OrdinalIgnoreCase
                ) >= 0)
            {
                return true;
            }


            if (
                display.ID
                    .ToString()
                    .Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase
                    ))
            {
                return true;
            }


            if (
                display.Quantity
                    .ToString()
                    .Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase
                    ))
            {
                return true;
            }


            if (
                display.SlotIndex
                    .ToString()
                    .Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase
                    ))
            {
                return true;
            }


            return false;
        }


        // =========================================================
        // SELECT INVENTORY ITEM
        // =========================================================

        private void lstInventoryItems_SelectedIndexChanged(
            object? sender,
            EventArgs e)
        {
            if (
                lstInventoryItems.SelectedItem
                is not InventoryDisplayItem selected)
            {
                ClearInventoryDetails();

                UpdateInventoryButtonState();

                return;
            }


            isLoadingInventoryControls =
                true;


            try
            {
                lblInventoryNameValue.Text =
                    selected.Name;


                lblInventoryIdValue.Text =
                    selected.ID
                        .ToString();


                lblInventorySlotValue.Text =
                    selected.SlotIndex
                        .ToString();


                lblInventoryTypeValue.Text =
                    $"{selected.Type} (0x{selected.Type:X2})";


                lblInventoryQuantityValue.Text =
                    selected.Quantity
                        .ToString();


                lblInventoryI06Value.Text =
                    $"{selected.Item.I_06} (0x{selected.Item.I_06:X2})";


                lblInventoryI07Value.Text =
                    $"{selected.Item.I_07} (0x{selected.Item.I_07:X2})";


                grpInventoryDetails.Text =
                    $"Selected Item - {cmbInventoryCategory.Text}";


                SetNumericValue(
                    nudInventoryQuantity,
                    selected.Quantity
                );
            }
            finally
            {
                isLoadingInventoryControls =
                    false;
            }


            UpdateInventoryButtonState();
        }


        // =========================================================
        // APPLY INVENTORY QUANTITY
        // =========================================================

        private void ApplyInventoryQuantity(
            byte quantity)
        {
            if (
                currentSave == null ||
                lstInventoryItems.SelectedItem
                    is not InventoryDisplayItem selected)
            {
                return;
            }


            try
            {
                int sectionOffset =
                    GetSelectedInventorySectionOffset();


                InventoryWriter.WriteQuantity(
                    currentSave.DecryptedData,
                    sectionOffset,
                    selected.Item,
                    quantity
                );


                MarkUnsaved();


                RefreshInventoryList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not change the inventory quantity.\n\n" +
                    ex.Message,
                    "XV2 Inventory Editor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }


        // =========================================================
        // APPLY BUTTON
        // =========================================================

        private void btnInventoryApplyQuantity_Click(
            object? sender,
            EventArgs e)
        {
            if (
                currentSave == null ||
                lstInventoryItems.SelectedItem
                    is not InventoryDisplayItem)
            {
                return;
            }


            ApplyInventoryQuantity(
                (byte)nudInventoryQuantity.Value
            );
        }


        // =========================================================
        // QUICK QUANTITY BUTTONS
        // =========================================================

        private void btnInventorySet1_Click(
            object? sender,
            EventArgs e)
        {
            nudInventoryQuantity.Value =
                1;


            ApplyInventoryQuantity(
                1
            );
        }


        private void btnInventorySet99_Click(
            object? sender,
            EventArgs e)
        {
            nudInventoryQuantity.Value =
                99;


            ApplyInventoryQuantity(
                99
            );
        }


        private void btnInventorySet50_Click(
            object? sender,
            EventArgs e)
        {
            nudInventoryQuantity.Value =
                50;


            ApplyInventoryQuantity(
                50
            );
        }


        private void btnInventorySet125_Click(
            object? sender,
            EventArgs e)
        {
            nudInventoryQuantity.Value =
                125;


            ApplyInventoryQuantity(
                125
            );
        }


        // =========================================================
        // MAX ALL IN CATEGORY
        // =========================================================

        private void btnInventoryMaxCategory_Click(
            object? sender,
            EventArgs e)
        {
            if (currentSave == null)
            {
                return;
            }


            List<XV2InventoryItem> items =
                chkInventoryBulkFiltered.Checked
                    ? currentInventoryDisplayItems
                        .Select(x => x.Item)
                        .ToList()
                    : GetSelectedInventoryCategory();


            if (items.Count == 0)
            {
                return;
            }


            DialogResult result =
                MessageBox.Show(
                    $"Set {items.Count} existing items in " +
                    $"{cmbInventoryCategory.Text} to quantity 125?\n\n" +

                    (chkInventoryBulkFiltered.Checked
                        ? "Only items currently matching the search/filter will be changed. "
                        : "All owned items in this category will be changed. ") +
                    "Only existing inventory entries will be changed. " +
                    "Empty inventory slots will not be touched.",

                    "Max Inventory Category",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );


            if (
                result !=
                DialogResult.Yes)
            {
                return;
            }


            try
            {
                int sectionOffset =
                    GetSelectedInventorySectionOffset();


                InventoryWriter.SetAllQuantities(
                    currentSave.DecryptedData,
                    sectionOffset,
                    items,
                    125
                );


                MarkUnsaved();


                RefreshInventoryList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not update the inventory category.\n\n" +
                    ex.Message,
                    "XV2 Inventory Editor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }


        private void chkInventoryBulkFiltered_CheckedChanged(
            object? sender,
            EventArgs e)
        {
            UpdateInventoryButtonState();
        }


        private void btnInventoryGiveMissing_Click(
            object? sender,
            EventArgs e)
        {
            if (currentSave != null && cmbInventoryCategory.SelectedIndex is >= 0 and <= 3)
            {
                DialogResult choice = MessageBox.Show(
                    "Give every catalogued Top, Bottom, Glove, and Shoe and set all clothing quantities?\n\nYes = 125 each\nNo = 99 each\nCancel = make no changes",
                    "Give All Clothes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (choice == DialogResult.Cancel) return;
                byte quantity = choice == DialogResult.Yes ? (byte)125 : (byte)99;
                NamedValueKind[] kinds = { NamedValueKind.Top, NamedValueKind.Bottom, NamedValueKind.Gloves, NamedValueKind.Shoes };
                int[] offsets = { InventoryReader.TopsOffset, InventoryReader.BottomsOffset, InventoryReader.GlovesOffset, InventoryReader.ShoesOffset };
                int added = 0;
                for (int type = 0; type < kinds.Length; type++)
                {
                    List<int> ids = nameDatabase.GetValues(kinds[type]).Select(x => x.SaveId).Distinct().ToList();
                    added += InventoryWriter.AddMissingClothing(currentSave.DecryptedData, offsets[type], (byte)type, ids, quantity);
                    XV2Inventory refreshed = InventoryReader.Read(currentSave.DecryptedData);
                    IEnumerable<XV2InventoryItem> items = type switch
                    {
                        0 => refreshed.Tops, 1 => refreshed.Bottoms, 2 => refreshed.Gloves, _ => refreshed.Shoes
                    };
                    InventoryWriter.SetAllQuantities(currentSave.DecryptedData, offsets[type], items, quantity);
                }
                MarkUnsaved(); RefreshInventoryList();
                MessageBox.Show($"All four clothing categories now have quantity {quantity}.\n\nAdded {added} previously missing entries.",
                    "Give All Clothes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (currentSave != null && cmbInventoryCategory.SelectedIndex is >= 4 and <= 8 && GetSelectedInventoryNameKind() is NamedValueKind kind)
            {
                DialogResult choice = MessageBox.Show(
                    $"Give every catalogued {cmbInventoryCategory.Text} entry and set all quantities?\n\nYes = 125 each\nNo = 99 each\nCancel = make no changes",
                    "Give All Items", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (choice == DialogResult.Cancel) return;
                byte quantity = choice == DialogResult.Yes ? (byte)125 : (byte)99;
                List<int> ids = nameDatabase.GetValues(kind).Select(x => x.SaveId).Distinct().ToList();
                int added = InventoryWriter.AddMissingItems(currentSave.DecryptedData, GetSelectedInventorySectionOffset(),
                    (byte)cmbInventoryCategory.SelectedIndex, ids, quantity);
                foreach (XV2InventoryItem item in GetSelectedInventoryCategory())
                    InventoryWriter.WriteQuantity(currentSave.DecryptedData, GetSelectedInventorySectionOffset(), item, quantity);
                if (cmbInventoryCategory.SelectedIndex == 7)
                {
                    PartnerKeyAccess.GiveAll(currentSave.DecryptedData);
                    List<int> occupied = currentSave.Characters.Where(character => !character.IsEmpty).Select(character => character.Slot - 1).ToList();
                    PartnerCustomizationInitializer.Initialize(currentSave.DecryptedData, occupied);
                    PartnerCustomizationInitializer.UnlockAllOptions(currentSave.DecryptedData);
                }
                MarkUnsaved(); RefreshInventoryList();
                MessageBox.Show($"All {cmbInventoryCategory.Text} now have quantity {quantity}.\n\nAdded {added} missing entries.",
                    "Give All Items", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MessageBox.Show(
                "Give All Missing Items is intentionally locked.\n\n" +
                "The current project verifies the 8-byte layout and quantity byte " +
                "for existing entries, but it does not yet verify the required Type, " +
                "I_06, and I_07 values for newly inserted entries in this category.\n\n" +
                "No save data was changed. This action can be enabled later once an " +
                "authoritative insertion format is implemented and tested.",
                "Safe Inventory Guard",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }


        // =========================================================
        // INVENTORY BUTTON STATE
        // =========================================================

        private void GivePartnerKeys()
        {
            if (currentSave == null) return;
            if (MessageBox.Show("Add Customization Unlock Keys 1–20 and set the verified partner-key activation fields?",
                "Give Customization Keys", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            int added = PartnerKeyAccess.GiveAll(currentSave.DecryptedData);
            List<int> occupied = currentSave.Characters.Where(character => !character.IsEmpty).Select(character => character.Slot - 1).ToList();
            int initialized = PartnerCustomizationInitializer.Initialize(currentSave.DecryptedData, occupied);
            PartnerCustomizationInitializer.UnlockAllOptions(currentSave.DecryptedData);
            MarkUnsaved(); RefreshInventoryList();
            MessageBox.Show($"Customization Keys 1–20 are present and activated.\n\nAdded {added} missing key items.\nInitialized {initialized} mentor customization records.",
                "Give Customization Keys", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void GiveInfiniteDragonBalls()
        {
            if (currentSave == null) return;
            enforceInfiniteDragonBalls = true;
            int added = SetAndVerifyInfiniteDragonBalls();
            MarkUnsaved(); RefreshInventoryList();
            MessageBox.Show($"All seven Dragon Balls now have quantity 125.\n\nAdded {added} missing Dragon Ball entries.",
                "Infinite Dragon Balls", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private int SetAndVerifyInfiniteDragonBalls()
        {
            if (currentSave == null) return 0;
            int added = InventoryWriter.SetDragonBalls(currentSave.DecryptedData, 125);
            List<XV2InventoryItem> result = InventoryReader.Read(currentSave.DecryptedData).ImportantItems
                .Where(x => x.SlotIndex < 7).ToList();
            if (result.Count != 7 || result.Any(x => x.ID != x.SlotIndex + 1 || x.Type != 0x06 || x.Quantity != 125))
                throw new InvalidOperationException("Dragon Ball quantity verification failed.");
            return added;
        }

        private void GiveWhisLevelFoods()
        {
            if (currentSave == null) return;
            // Full-database Mix Item IDs: Parfait, Tempura, Sushi, Octopus Balls.
            int[] ids = { 76, 77, 78, 79 };
            int added = InventoryWriter.AddMissingItems(currentSave.DecryptedData,
                InventoryReader.MixItemsOffset, 6, ids, 125);
            foreach (XV2InventoryItem item in InventoryReader.Read(currentSave.DecryptedData).MixItems.Where(x => ids.Contains(x.ID)))
                InventoryWriter.WriteQuantity(currentSave.DecryptedData, InventoryReader.MixItemsOffset, item, 125);
            MarkUnsaved(); RefreshInventoryList();
            MessageBox.Show($"Parfait, Tempura, Sushi, and Octopus Balls now have quantity 125.\n\nAdded {added} missing food entries. Give the requested food to Whis at each cap.",
                "Whis Level Foods", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void UpdateInventoryButtonState()
        {
            bool hasSave =
                currentSave != null;


            bool hasSelectedItem =
                hasSave &&
                lstInventoryItems.SelectedItem
                is InventoryDisplayItem;


            btnInventoryApplyQuantity.Enabled =
                hasSelectedItem;


            btnInventorySet1.Enabled =
                hasSelectedItem;


            btnInventorySet50.Enabled =
                hasSelectedItem;


            btnInventorySet99.Enabled =
                hasSelectedItem;


            btnInventorySet125.Enabled =
                hasSelectedItem;


            nudInventoryQuantity.Enabled =
                hasSelectedItem;


            btnInventoryMaxCategory.Enabled =
                hasSave &&
                (chkInventoryBulkFiltered.Checked
                    ? currentInventoryDisplayItems.Count
                    : GetSelectedInventoryCategory().Count) > 0;


            btnInventoryGiveMissing.Enabled =
                hasSave;

            btnInventoryGiveMissing.Text = cmbInventoryCategory.SelectedIndex is >= 0 and <= 3
                ? "Give All Clothes (99 / 125)"
                : "Give All (99 / 125)";
        }


        // =========================================================
        // CLEAR INVENTORY DETAILS
        // =========================================================

        private void ClearInventoryDetails()
        {
            isLoadingInventoryControls =
                true;


            try
            {
                lblInventoryNameValue.Text =
                    "-";


                lblInventoryIdValue.Text =
                    "-";


                lblInventorySlotValue.Text =
                    "-";


                lblInventoryTypeValue.Text =
                    "-";


                lblInventoryQuantityValue.Text =
                    "-";


                lblInventoryI06Value.Text =
                    "-";


                lblInventoryI07Value.Text =
                    "-";


                grpInventoryDetails.Text =
                    "Selected Item";


                nudInventoryQuantity.Value =
                    0;
            }
            finally
            {
                isLoadingInventoryControls =
                    false;
            }
        }

        private void btnOpenSave_Click(
            object? sender,
            EventArgs e)
        {
            if (hasUnsavedChanges && MessageBox.Show(
                "Opening another save will discard all unsaved changes. Continue?",
                "Unsaved changes", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            using OpenFileDialog dialog =
                new OpenFileDialog();


            dialog.Title =
                "Open Dragon Ball Xenoverse 2 Save";


            dialog.Filter =
                "Xenoverse 2 Saves (*.sav;*.dat;*.bin)|*.sav;*.dat;*.bin|" +
                "All Files (*.*)|*.*";


            if (
                dialog.ShowDialog()
                != DialogResult.OK)
            {
                return;
            }


            try
            {
                currentSave =
                    new SaveFile(
                        dialog.FileName
                    );
                loadedCharacter = null;
                loadedCharacterSlotIndex = -1;
                enforceInfiniteDragonBalls = false;

                RecordOpenedSave(dialog.FileName);

                // Every per-save snapshot and character selector must be reset.
                // Otherwise controls can retain XV2Character objects from the
                // previously loaded save and read the wrong CaC slot.
                questProgressSnapshot = null;
                progressionSnapshot = null;
                skillOwnershipSnapshot = null;
                playDataSnapshot = null;
                if (cmbQuestCharacter != null) cmbQuestCharacter.Items.Clear();
                if (cmbProgressCharacter != null) cmbProgressCharacter.Items.Clear();
                if (cmbMentorCharacter != null) cmbMentorCharacter.Items.Clear();
                if (cmbPartnerCharacter != null) cmbPartnerCharacter.Items.Clear();


                string backupPath =
                    currentSave.CreateBackup();


                isLoadingControls =
                    true;

                isLoadingPresetControls =
                    true;

                isSynchronizingCharacters =
                    true;


                rememberedPresetIndex =
                    0;


                nudZeni.Value =
                    Math.Min(currentSave.Zeni, 999999999u);


                nudTPMedals.Value =
                    Math.Min(currentSave.TPMedals, 999999999u);


                cmbCharacters.BeginUpdate();

                cmbPresetCharacters.BeginUpdate();


                try
                {
                    cmbCharacters.Items.Clear();

                    cmbPresetCharacters.Items.Clear();


                    foreach (
                        XV2Character character
                        in currentSave.Characters)
                    {
                        cmbCharacters.Items.Add(
                            character
                        );


                        cmbPresetCharacters.Items.Add(
                            character
                        );
                    }


                    if (
                        cmbCharacters.Items.Count > 0)
                    {
                        cmbCharacters.SelectedIndex =
                            0;


                        cmbPresetCharacters.SelectedIndex =
                            0;
                    }
                }
                finally
                {
                    cmbCharacters.EndUpdate();

                    cmbPresetCharacters.EndUpdate();
                }


                CaptureOriginalPresetSnapshots();

                ResetPresetClipboardState();


                isSynchronizingCharacters =
                    false;

                isLoadingControls =
                    false;

                isLoadingPresetControls =
                    false;


                copiedStats =
                    null;

                copiedAppearance =
                    null;


                btnCopyStats.Text =
                    "Copy Stats";

                btnCopyAppearance.Text =
                    "Copy Appearance";


                hasUnsavedChanges =
                    false;


                SetEditorEnabled(
                    true
                );


                LoadSelectedCharacter();

                LoadPresetList();

                UpdatePresetToolButtons();

                RefreshInventoryList();

                SnapshotQQBangs();

                LoadSkillOwnership();

                RefreshCacManagementHub();

                LoadQuestProgress();

                RefreshPlayDataEditor();

                ResetAndRunDiagnostics();

                ResetChangeHistory();

                RefreshBackupRecovery();


                UpdateWindowTitle();


                MessageBox.Show(
                    "Save loaded successfully!\n\n" +
                    "Backup created:\n" +
                    $"{Path.GetFileName(backupPath)}",
                    "XV2 Save Editor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                isLoadingControls =
                    false;

                isLoadingPresetControls =
                    false;

                isSynchronizingCharacters =
                    false;


                MessageBox.Show(
                    $"Could not open the save file:\n\n{ex.Message}",
                    "XV2 Save Editor - Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }


        // =========================================================
        // ORIGINAL PRESET SNAPSHOTS
        // =========================================================

        private void CaptureOriginalPresetSnapshots()
        {
            originalPresetSnapshots.Clear();


            if (currentSave == null)
            {
                return;
            }


            foreach (
                XV2Character character
                in currentSave.Characters)
            {
                foreach (
                    XV2Preset preset
                    in character.Presets)
                {
                    originalPresetSnapshots[
                        (
                            character.Slot,
                            preset.Index
                        )
                    ] =
                        ClonePreset(
                            preset
                        );
                }
            }
        }


        // =========================================================
        // SAVE
        // =========================================================

        private void btnSave_Click(
            object? sender,
            EventArgs e)
        {
            if (currentSave == null)
            {
                MessageBox.Show(
                    "Please open a Xenoverse 2 save first.",
                    "XV2 Save Editor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }


            try
            {
                StorePresetControlsInSelectedPreset();

                int changedSkills = CountChangedSkillOwnership();
                int changedQuests = CountChangedQuestProgress();
                int changedProgression = CountChangedProgressionBytes();
                int changedPlayData = CountChangedPlayDataFields();
                string summary = "Save the current edits?\n\n" +
                    $"Verified skill ownership changes: {changedSkills}\n" +
                    $"Verified quest progress records changed: {changedQuests}\n" +
                    $"Mentor/collection progression bytes changed: {changedProgression}\n" +
                    $"Verified Play Data fields changed: {changedPlayData}\n" +
                    "Other edited sections: CaC, presets, inventory, and QQ Bangs are included when changed.";
                if (MessageBox.Show(summary, "Review changes before save", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) != DialogResult.OK)
                    return;


                currentSave.Zeni =
                    (uint)nudZeni.Value;


                currentSave.TPMedals =
                    (uint)nudTPMedals.Value;


                StoreLoadedCharacterControls();


                foreach (
                    object item
                    in cmbPresetCharacters.Items)
                {
                    if (
                        item is not XV2Character
                        presetCharacter)
                    {
                        continue;
                    }


                    PresetWriter.WritePresets(
                        currentSave.DecryptedData,

                        presetCharacter.Slot - 1,

                        presetCharacter.Presets
                    );
                }

                // LazyBones' verified account-wide and per-CaC level-cap flags.
                LevelCapFlagValidator.Apply(currentSave.DecryptedData);
                if (enforceInfiniteDragonBalls) SetAndVerifyInfiniteDragonBalls();

                while (true)
                {
                    List<PreSaveIssue> findings = PreSaveValidator.Inspect(currentSave);
                    if (findings.Count == 0) break;
                    using PreSaveValidationDialog validation = new(findings);
                    DialogResult validationResult = validation.ShowDialog(this);
                    if (validationResult == DialogResult.Retry)
                    {
                        MarkUnsaved();
                        RefreshLoadedSave(cmbCharacters.SelectedIndex < 0 ? 0 : cmbCharacters.SelectedIndex, preserveDirty: true);
                        continue;
                    }
                    if (validationResult != DialogResult.OK) return;
                    break;
                }


                using SaveFileDialog dialog =
                    new SaveFileDialog();


                dialog.Title =
                    "Save Edited Dragon Ball Xenoverse 2 Save";


                dialog.Filter =
                    "Xenoverse 2 Saves (*.sav;*.dat;*.bin)|*.sav;*.dat;*.bin|" +
                    "All Files (*.*)|*.*";


                dialog.FileName =
                    Path.GetFileName(currentSave.FilePath);

                dialog.InitialDirectory =
                    Path.GetDirectoryName(currentSave.FilePath);


                if (
                    dialog.ShowDialog()
                    != DialogResult.OK)
                {
                    return;
                }


                currentSave.SaveAs(
                    dialog.FileName
                );


                hasUnsavedChanges =
                    false;

                skillOwnershipSnapshot = (byte[])currentSave.DecryptedData.Clone();
                questProgressSnapshot = (byte[])currentSave.DecryptedData.Clone();


                UpdateWindowTitle();


                MessageBox.Show(
                    "Edited save created successfully!\n\n" + dialog.FileName,
                    "XV2 Save Editor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not save:\n\n{ex.Message}",
                    "XV2 Save Editor - Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }


        // =========================================================
        // CHARACTER SELECTION
        // =========================================================

        private void cmbCharacters_SelectedIndexChanged(
            object? sender,
            EventArgs e)
        {
            if (isSynchronizingCharacters)
            {
                return;
            }


            StoreLoadedCharacterControls();
            StorePresetControlsInSelectedPreset();


            isSynchronizingCharacters =
                true;


            try
            {
                if (
                    cmbCharacters.SelectedIndex >= 0 &&
                    cmbCharacters.SelectedIndex <
                    cmbPresetCharacters.Items.Count)
                {
                    cmbPresetCharacters.SelectedIndex =
                        cmbCharacters.SelectedIndex;
                }
            }
            finally
            {
                isSynchronizingCharacters =
                    false;
            }


            LoadSelectedCharacter();

            LoadPresetList();
        }


        private void cmbPresetCharacters_SelectedIndexChanged(
            object? sender,
            EventArgs e)
        {
            if (isSynchronizingCharacters)
            {
                return;
            }


            StoreLoadedCharacterControls();
            StorePresetControlsInSelectedPreset();


            isSynchronizingCharacters =
                true;


            try
            {
                if (
                    cmbPresetCharacters.SelectedIndex >= 0 &&
                    cmbPresetCharacters.SelectedIndex <
                    cmbCharacters.Items.Count)
                {
                    cmbCharacters.SelectedIndex =
                        cmbPresetCharacters.SelectedIndex;
                }
            }
            finally
            {
                isSynchronizingCharacters =
                    false;
            }


            LoadSelectedCharacter();

            LoadPresetList();
        }


        // =========================================================
        // LOAD CHARACTER
        // =========================================================

        private void LoadSelectedCharacter()
        {
            if (
                cmbCharacters.SelectedItem
                is not XV2Character character)
            {
                loadedCharacter = null;
                loadedCharacterSlotIndex = -1;
                return;
            }

            loadedCharacter = character;
            loadedCharacterSlotIndex = character.Slot - 1;


            isLoadingControls =
                true;


            try
            {
                if (character.IsEmpty)
                {
                    ClearCharacterControls();

                    ClearAppearanceControls();

                    return;
                }


                txtCharacterName.Text =
                    character.Name;


                if (
                    character.Race >= 0 &&
                    character.Race <= 7)
                {
                    cmbRace.SelectedIndex =
                        character.Race;
                }
                else
                {
                    cmbRace.SelectedIndex =
                        -1;
                }


                SetNumericValue(
                    nudCharacterLevel,
                    character.Level
                );


                SetNumericValue(
                    nudCharacterXP,
                    character.Experience
                );


                SetNumericValue(
                    nudAttributePoints,
                    character.AttributePoints
                );


                SetNumericValue(
                    nudHealth,
                    character.Health
                );


                SetNumericValue(
                    nudKi,
                    character.Ki
                );


                SetNumericValue(
                    nudStamina,
                    character.Stamina
                );


                SetNumericValue(
                    nudBasicAttack,
                    character.BasicAttack
                );


                SetNumericValue(
                    nudStrikeSupers,
                    character.StrikeSupers
                );


                SetNumericValue(
                    nudKiBlastSupers,
                    character.KiBlastSupers
                );


                if (
                    character.Appearance != null)
                {
                    LoadAppearance(
                        character.Appearance
                    );
                }
                else
                {
                    ClearAppearanceControls();
                }
            }
            finally
            {
                isLoadingControls =
                    false;
            }
        }


        // =========================================================
        // PRESET LIST
        // =========================================================

        private void LoadPresetList()
        {
            bool previousLoadingState =
                isLoadingPresetControls;


            isLoadingPresetControls =
                true;


            cmbPresets.BeginUpdate();


            try
            {
                cmbPresets.Items.Clear();


                if (
                    cmbPresetCharacters.SelectedItem
                    is not XV2Character character)
                {
                    ClearPresetControls();

                    return;
                }


                if (
                    character.Presets == null ||
                    character.Presets.Count == 0)
                {
                    ClearPresetControls();

                    return;
                }


                foreach (
                    XV2Preset preset
                    in character.Presets)
                {
                    cmbPresets.Items.Add(
                        preset
                    );
                }


                int index =
                    rememberedPresetIndex;


                if (
                    index < 0 ||
                    index >=
                    cmbPresets.Items.Count)
                {
                    index =
                        0;
                }


                cmbPresets.SelectedIndex =
                    index;
            }
            finally
            {
                cmbPresets.EndUpdate();


                isLoadingPresetControls =
                    previousLoadingState;
            }


            LoadSelectedPreset();

            UpdatePresetToolButtons();
        }


        private void cmbPresets_SelectedIndexChanged(
            object? sender,
            EventArgs e)
        {
            if (
                cmbPresets.SelectedIndex >= 0)
            {
                rememberedPresetIndex =
                    cmbPresets.SelectedIndex;
            }


            if (
                isLoadingPresetControls)
            {
                return;
            }


            LoadSelectedPreset();

            UpdatePresetToolButtons();
        }


        // =========================================================
        // LOAD PRESET
        // =========================================================

        private void LoadSelectedPreset()
        {
            if (
                cmbPresets.SelectedItem
                is not XV2Preset preset)
            {
                ClearPresetControls();

                return;
            }


            isLoadingPresetControls =
                true;


            grpPresetEquipment.SuspendLayout();

            grpPresetSkills.SuspendLayout();

            grpPresetColors.SuspendLayout();


            try
            {
                SelectNamedValue(
                    cmbPresetTop,
                    NamedValueKind.Top,
                    preset.Top
                );


                SelectNamedValue(
                    cmbPresetBottom,
                    NamedValueKind.Bottom,
                    preset.Bottom
                );


                SelectNamedValue(
                    cmbPresetGloves,
                    NamedValueKind.Gloves,
                    preset.Gloves
                );


                SelectNamedValue(
                    cmbPresetShoes,
                    NamedValueKind.Shoes,
                    preset.Shoes
                );


                SelectNamedValue(
                    cmbPresetAccessory,
                    NamedValueKind.Accessory,
                    preset.Accessory
                );


                SelectNamedValue(
                    cmbPresetSuperSoul,
                    NamedValueKind.SuperSoul,
                    preset.SuperSoul
                );


                SetNumericValue(
                    nudPresetQQBang,
                    preset.QQBang
                );


                SelectNamedValue(
                    cmbSuperSkill1,
                    NamedValueKind.SuperSkill,
                    preset.SuperSkill1
                );


                SelectNamedValue(
                    cmbSuperSkill2,
                    NamedValueKind.SuperSkill,
                    preset.SuperSkill2
                );


                SelectNamedValue(
                    cmbSuperSkill3,
                    NamedValueKind.SuperSkill,
                    preset.SuperSkill3
                );


                SelectNamedValue(
                    cmbSuperSkill4,
                    NamedValueKind.SuperSkill,
                    preset.SuperSkill4
                );


                SelectNamedValue(
                    cmbUltimateSkill1,
                    NamedValueKind.UltimateSkill,
                    preset.UltimateSkill1
                );


                SelectNamedValue(
                    cmbUltimateSkill2,
                    NamedValueKind.UltimateSkill,
                    preset.UltimateSkill2
                );


                SelectNamedValue(
                    cmbEvasiveSkill,
                    NamedValueKind.EvasiveSkill,
                    preset.EvasiveSkill
                );


                SetNumericValue(
                    nudBlastSkill,
                    preset.BlastSkill
                );


                SelectNamedValue(
                    cmbAwokenSkill,
                    NamedValueKind.AwokenSkill,
                    preset.AwokenSkill
                );


                SetNumericValue(
                    nudTopColor1,
                    preset.TopColor1
                );

                SetNumericValue(
                    nudTopColor2,
                    preset.TopColor2
                );

                SetNumericValue(
                    nudTopColor3,
                    preset.TopColor3
                );

                SetNumericValue(
                    nudTopColor4,
                    preset.TopColor4
                );


                SetNumericValue(
                    nudBottomColor1,
                    preset.BottomColor1
                );

                SetNumericValue(
                    nudBottomColor2,
                    preset.BottomColor2
                );

                SetNumericValue(
                    nudBottomColor3,
                    preset.BottomColor3
                );

                SetNumericValue(
                    nudBottomColor4,
                    preset.BottomColor4
                );


                SetNumericValue(
                    nudGlovesColor1,
                    preset.GlovesColor1
                );

                SetNumericValue(
                    nudGlovesColor2,
                    preset.GlovesColor2
                );

                SetNumericValue(
                    nudGlovesColor3,
                    preset.GlovesColor3
                );

                SetNumericValue(
                    nudGlovesColor4,
                    preset.GlovesColor4
                );


                SetNumericValue(
                    nudShoesColor1,
                    preset.ShoesColor1
                );

                SetNumericValue(
                    nudShoesColor2,
                    preset.ShoesColor2
                );

                SetNumericValue(
                    nudShoesColor3,
                    preset.ShoesColor3
                );

                SetNumericValue(
                    nudShoesColor4,
                    preset.ShoesColor4
                );
            }
            finally
            {
                grpPresetEquipment.ResumeLayout(
                    false
                );


                grpPresetSkills.ResumeLayout(
                    false
                );


                grpPresetColors.ResumeLayout(
                    false
                );


                isLoadingPresetControls =
                    false;
            }
        }


        // =========================================================
        // SELECT NAMED VALUE
        // =========================================================

        private void SelectNamedValue(
            ComboBox combo,
            NamedValueKind kind,
            int saveId)
        {
            NamedSaveValue? selected =
                null;


            if (nameDatabaseLoaded)
            {
                nameDatabase.TryGetValue(
                    kind,
                    saveId,
                    out selected
                );
            }


            if (selected == null)
            {
                selected =
                    combo.Items
                        .OfType<NamedSaveValue>()
                        .FirstOrDefault(
                            x =>
                                x.SaveId ==
                                saveId
                        );
            }


            if (selected == null)
            {
                selected =
                    nameDatabaseLoaded

                        ? nameDatabase.GetOrUnknown(
                            kind,
                            saveId
                        )

                        : new NamedSaveValue
                        {
                            SaveId =
                                saveId,

                            DatabaseId =
                                saveId,

                            Name =
                                "Unknown / Raw",

                            Kind =
                                kind
                        };


                combo.Items.Insert(
                    0,
                    selected
                );
            }


            isSearchingNamedCombo =
                true;


            combo.SelectedItem =
                selected;


            lastValidNamedSelections[
                combo
            ] = selected;


            isSearchingNamedCombo =
                false;
        }


        private static int GetSelectedSaveId(
            ComboBox combo,
            int fallback)
        {
            if (
                combo.SelectedItem
                is NamedSaveValue value)
            {
                return value.SaveId;
            }


            return fallback;
        }


        // =========================================================
        // PRESET VALUE CHANGES
        // =========================================================

        private void PresetNamedValueChanged(
            object? sender,
            EventArgs e)
        {
            if (
                isLoadingPresetControls ||
                isSearchingNamedCombo ||
                currentSave == null)
            {
                return;
            }


            if (
                sender is ComboBox combo)
            {
                if (
                    combo.SelectedItem
                    is not NamedSaveValue selectedValue)
                {
                    return;
                }


                lastValidNamedSelections[
                    combo
                ] = selectedValue;
            }


            StorePresetControlsInSelectedPreset();

            MarkUnsaved();
        }


        private void PresetValueChanged(
            object? sender,
            EventArgs e)
        {
            if (
                isLoadingPresetControls ||
                currentSave == null)
            {
                return;
            }


            StorePresetControlsInSelectedPreset();

            MarkUnsaved();
        }


        // =========================================================
        // STORE PRESET
        // =========================================================

        private void StorePresetControlsInSelectedPreset()
        {
            if (
                isLoadingPresetControls)
            {
                return;
            }


            if (
                cmbPresets.SelectedItem
                is not XV2Preset preset)
            {
                return;
            }


            preset.Top =
                GetSelectedSaveId(
                    cmbPresetTop,
                    preset.Top
                );


            preset.Bottom =
                GetSelectedSaveId(
                    cmbPresetBottom,
                    preset.Bottom
                );


            preset.Gloves =
                GetSelectedSaveId(
                    cmbPresetGloves,
                    preset.Gloves
                );


            preset.Shoes =
                GetSelectedSaveId(
                    cmbPresetShoes,
                    preset.Shoes
                );


            preset.Accessory =
                GetSelectedSaveId(
                    cmbPresetAccessory,
                    preset.Accessory
                );


            preset.SuperSoul =
                GetSelectedSaveId(
                    cmbPresetSuperSoul,
                    preset.SuperSoul
                );


            preset.QQBang =
                (int)nudPresetQQBang.Value;


            preset.SuperSkill1 =
                GetSelectedSaveId(
                    cmbSuperSkill1,
                    preset.SuperSkill1
                );


            preset.SuperSkill2 =
                GetSelectedSaveId(
                    cmbSuperSkill2,
                    preset.SuperSkill2
                );


            preset.SuperSkill3 =
                GetSelectedSaveId(
                    cmbSuperSkill3,
                    preset.SuperSkill3
                );


            preset.SuperSkill4 =
                GetSelectedSaveId(
                    cmbSuperSkill4,
                    preset.SuperSkill4
                );


            preset.UltimateSkill1 =
                GetSelectedSaveId(
                    cmbUltimateSkill1,
                    preset.UltimateSkill1
                );


            preset.UltimateSkill2 =
                GetSelectedSaveId(
                    cmbUltimateSkill2,
                    preset.UltimateSkill2
                );


            preset.EvasiveSkill =
                GetSelectedSaveId(
                    cmbEvasiveSkill,
                    preset.EvasiveSkill
                );


            preset.BlastSkill =
                (int)nudBlastSkill.Value;


            preset.AwokenSkill =
                GetSelectedSaveId(
                    cmbAwokenSkill,
                    preset.AwokenSkill
                );


            preset.TopColor1 =
                (ushort)nudTopColor1.Value;

            preset.TopColor2 =
                (ushort)nudTopColor2.Value;

            preset.TopColor3 =
                (ushort)nudTopColor3.Value;

            preset.TopColor4 =
                (ushort)nudTopColor4.Value;


            preset.BottomColor1 =
                (ushort)nudBottomColor1.Value;

            preset.BottomColor2 =
                (ushort)nudBottomColor2.Value;

            preset.BottomColor3 =
                (ushort)nudBottomColor3.Value;

            preset.BottomColor4 =
                (ushort)nudBottomColor4.Value;


            preset.GlovesColor1 =
                (ushort)nudGlovesColor1.Value;

            preset.GlovesColor2 =
                (ushort)nudGlovesColor2.Value;

            preset.GlovesColor3 =
                (ushort)nudGlovesColor3.Value;

            preset.GlovesColor4 =
                (ushort)nudGlovesColor4.Value;


            preset.ShoesColor1 =
                (ushort)nudShoesColor1.Value;

            preset.ShoesColor2 =
                (ushort)nudShoesColor2.Value;

            preset.ShoesColor3 =
                (ushort)nudShoesColor3.Value;

            preset.ShoesColor4 =
                (ushort)nudShoesColor4.Value;
        }


        // =========================================================
        // PRESET TOOLS
        // =========================================================

        private XV2Preset? GetCurrentPreset()
        {
            StorePresetControlsInSelectedPreset();


            return cmbPresets.SelectedItem
                as XV2Preset;
        }


        private string GetPresetSourceName()
        {
            int characterSlot =
                0;


            if (
                cmbPresetCharacters.SelectedItem
                is XV2Character character)
            {
                characterSlot =
                    character.Slot;
            }


            int presetNumber =
                cmbPresets.SelectedIndex + 1;


            return
                $"C{characterSlot} P{presetNumber}";
        }


        private static XV2Preset ClonePreset(
            XV2Preset source)
        {
            XV2Preset result =
                new XV2Preset();


            CopyEntirePreset(
                source,
                result
            );


            result.Index =
                source.Index;


            return result;
        }


        private static void CopyEntirePreset(
            XV2Preset source,
            XV2Preset destination)
        {
            int destinationIndex =
                destination.Index;


            destination.Top =
                source.Top;

            destination.Bottom =
                source.Bottom;

            destination.Gloves =
                source.Gloves;

            destination.Shoes =
                source.Shoes;

            destination.Accessory =
                source.Accessory;

            destination.SuperSoul =
                source.SuperSoul;

            destination.QQBang =
                source.QQBang;


            CopyOutfitFields(
                source,
                destination
            );


            CopySkillFields(
                source,
                destination
            );


            destination.Index =
                destinationIndex;
        }


        private static void CopyOutfitFields(
            XV2Preset source,
            XV2Preset destination)
        {
            destination.Top =
                source.Top;

            destination.Bottom =
                source.Bottom;

            destination.Gloves =
                source.Gloves;

            destination.Shoes =
                source.Shoes;

            destination.Accessory =
                source.Accessory;


            destination.TopColor1 =
                source.TopColor1;

            destination.TopColor2 =
                source.TopColor2;

            destination.TopColor3 =
                source.TopColor3;

            destination.TopColor4 =
                source.TopColor4;


            destination.BottomColor1 =
                source.BottomColor1;

            destination.BottomColor2 =
                source.BottomColor2;

            destination.BottomColor3 =
                source.BottomColor3;

            destination.BottomColor4 =
                source.BottomColor4;


            destination.GlovesColor1 =
                source.GlovesColor1;

            destination.GlovesColor2 =
                source.GlovesColor2;

            destination.GlovesColor3 =
                source.GlovesColor3;

            destination.GlovesColor4 =
                source.GlovesColor4;


            destination.ShoesColor1 =
                source.ShoesColor1;

            destination.ShoesColor2 =
                source.ShoesColor2;

            destination.ShoesColor3 =
                source.ShoesColor3;

            destination.ShoesColor4 =
                source.ShoesColor4;
        }


        private static void CopySkillFields(
            XV2Preset source,
            XV2Preset destination)
        {
            destination.SuperSkill1 =
                source.SuperSkill1;

            destination.SuperSkill2 =
                source.SuperSkill2;

            destination.SuperSkill3 =
                source.SuperSkill3;

            destination.SuperSkill4 =
                source.SuperSkill4;


            destination.UltimateSkill1 =
                source.UltimateSkill1;

            destination.UltimateSkill2 =
                source.UltimateSkill2;


            destination.EvasiveSkill =
                source.EvasiveSkill;

            destination.BlastSkill =
                source.BlastSkill;

            destination.AwokenSkill =
                source.AwokenSkill;
        }


        private void btnCopyPreset_Click(
            object? sender,
            EventArgs e)
        {
            XV2Preset? preset =
                GetCurrentPreset();


            if (preset == null)
            {
                return;
            }


            copiedPreset =
                ClonePreset(
                    preset
                );


            copiedPresetSource =
                GetPresetSourceName();


            UpdatePresetToolButtons();
        }


        private void btnPastePreset_Click(
            object? sender,
            EventArgs e)
        {
            if (
                copiedPreset == null ||
                cmbPresets.SelectedItem
                is not XV2Preset destination)
            {
                return;
            }


            CopyEntirePreset(
                copiedPreset,
                destination
            );


            LoadSelectedPreset();

            MarkUnsaved();
        }


        private void btnCopyOutfit_Click(
            object? sender,
            EventArgs e)
        {
            XV2Preset? preset =
                GetCurrentPreset();


            if (preset == null)
            {
                return;
            }


            copiedOutfit =
                ClonePreset(
                    preset
                );


            copiedOutfitSource =
                GetPresetSourceName();


            UpdatePresetToolButtons();
        }


        private void btnPasteOutfit_Click(
            object? sender,
            EventArgs e)
        {
            if (
                copiedOutfit == null ||
                cmbPresets.SelectedItem
                is not XV2Preset destination)
            {
                return;
            }


            CopyOutfitFields(
                copiedOutfit,
                destination
            );


            LoadSelectedPreset();

            MarkUnsaved();
        }


        private void btnCopySkills_Click(
            object? sender,
            EventArgs e)
        {
            XV2Preset? preset =
                GetCurrentPreset();


            if (preset == null)
            {
                return;
            }


            copiedSkills =
                ClonePreset(
                    preset
                );


            copiedSkillsSource =
                GetPresetSourceName();


            UpdatePresetToolButtons();
        }


        private void btnPasteSkills_Click(
            object? sender,
            EventArgs e)
        {
            if (
                copiedSkills == null ||
                cmbPresets.SelectedItem
                is not XV2Preset destination)
            {
                return;
            }


            CopySkillFields(
                copiedSkills,
                destination
            );


            LoadSelectedPreset();

            MarkUnsaved();
        }


        private void btnResetPreset_Click(
            object? sender,
            EventArgs e)
        {
            if (
                cmbPresetCharacters.SelectedItem
                    is not XV2Character character ||
                cmbPresets.SelectedItem
                    is not XV2Preset destination)
            {
                return;
            }


            (
                int Slot,
                int Preset
            ) key =
                (
                    character.Slot,
                    destination.Index
                );


            if (
                !originalPresetSnapshots
                    .TryGetValue(
                        key,
                        out XV2Preset? original
                    ))
            {
                MessageBox.Show(
                    "The original version of this preset " +
                    "could not be found.",
                    "XV2 Save Editor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );


                return;
            }


            CopyEntirePreset(
                original,
                destination
            );


            LoadSelectedPreset();

            MarkUnsaved();
        }


        private void ResetPresetClipboardState()
        {
            copiedPreset =
                null;

            copiedOutfit =
                null;

            copiedSkills =
                null;


            copiedPresetSource =
                "";

            copiedOutfitSource =
                "";

            copiedSkillsSource =
                "";


            UpdatePresetToolButtons();
        }


        private void UpdatePresetToolButtons()
        {
            bool hasPreset =
                currentSave != null &&
                cmbPresets.SelectedItem
                is XV2Preset;


            btnCopyPreset.Enabled =
                hasPreset;

            btnCopyOutfit.Enabled =
                hasPreset;

            btnCopySkills.Enabled =
                hasPreset;

            btnResetPreset.Enabled =
                hasPreset;


            btnPastePreset.Enabled =
                hasPreset &&
                copiedPreset != null;


            btnPasteOutfit.Enabled =
                hasPreset &&
                copiedOutfit != null;


            btnPasteSkills.Enabled =
                hasPreset &&
                copiedSkills != null;


            btnPastePreset.Text =
                copiedPreset == null
                    ? "Paste Preset"
                    : $"Paste ({copiedPresetSource})";


            btnPasteOutfit.Text =
                copiedOutfit == null
                    ? "Paste Outfit"
                    : $"Paste {copiedOutfitSource}";


            btnPasteSkills.Text =
                copiedSkills == null
                    ? "Paste Skills"
                    : $"Paste {copiedSkillsSource}";
        }


        // =========================================================
        // CLEAR PRESET
        // =========================================================

        private void ClearPresetControls()
        {
            bool previousState =
                isLoadingPresetControls;


            isLoadingPresetControls =
                true;


            try
            {
                foreach (
                    ComboBox combo
                    in GetNamedComboBoxes())
                {
                    combo.SelectedIndex =
                        -1;


                    combo.Text =
                        "";


                    lastValidNamedSelections[
                        combo
                    ] = null;
                }


                NumericUpDown[] numericFields =
                {
                    nudPresetQQBang,
                    nudBlastSkill,

                    nudTopColor1,
                    nudTopColor2,
                    nudTopColor3,
                    nudTopColor4,

                    nudBottomColor1,
                    nudBottomColor2,
                    nudBottomColor3,
                    nudBottomColor4,

                    nudGlovesColor1,
                    nudGlovesColor2,
                    nudGlovesColor3,
                    nudGlovesColor4,

                    nudShoesColor1,
                    nudShoesColor2,
                    nudShoesColor3,
                    nudShoesColor4
                };


                foreach (
                    NumericUpDown field
                    in numericFields)
                {
                    SetNumericValue(
                        field,
                        0
                    );
                }
            }
            finally
            {
                isLoadingPresetControls =
                    previousState;
            }


            UpdatePresetToolButtons();
        }


        // =========================================================
        // LEVEL / XP
        // =========================================================

        private void nudCharacterLevel_ValueChanged(
            object? sender,
            EventArgs e)
        {
            if (
                isLoadingControls)
            {
                return;
            }


            MarkUnsaved();

            ApplyAutomaticLevelValues();
        }


        private void chkAutoXP_CheckedChanged(
            object? sender,
            EventArgs e)
        {
            UpdateAutoToggleVisuals();

            if (
                isLoadingControls)
            {
                return;
            }


            if (chkAutoXP.Checked)
            {
                ApplyAutomaticLevelValues();

                nudCharacterXP.Enabled =
                    false;
            }
            else
            {
                nudCharacterXP.Enabled =
                    true;
            }
        }


        private void chkAutoAttributePoints_CheckedChanged(
            object? sender,
            EventArgs e)
        {
            UpdateAutoToggleVisuals();

            if (
                isLoadingControls)
            {
                return;
            }


            if (
                chkAutoAttributePoints.Checked)
            {
                ApplyAutomaticLevelValues();

                nudAttributePoints.Enabled =
                    false;
            }
            else
            {
                nudAttributePoints.Enabled =
                    true;
            }
        }


        private void ApplyAutomaticLevelValues()
        {
            int level =
                (int)nudCharacterLevel.Value;


            if (
                !LevelExperience.IsValidLevel(
                    level
                ))
            {
                return;
            }


            isLoadingControls =
                true;


            try
            {
                if (chkAutoXP.Checked)
                {
                    SetNumericValue(
                        nudCharacterXP,

                        LevelExperience
                            .ExperienceForLevel(
                                level
                            )
                    );
                }


                if (
                    chkAutoAttributePoints.Checked)
                {
                    SetNumericValue(
                        nudAttributePoints,

                        LevelExperience
                            .AttributePointsForLevel(
                                level
                            )
                    );
                }
            }
            finally
            {
                isLoadingControls =
                    false;
            }


            MarkUnsaved();
        }


        // =========================================================
        // MAX BUTTONS
        // =========================================================

        private void btnMaxZeni_Click(
            object? sender,
            EventArgs e)
        {
            nudZeni.Value =
                nudZeni.Maximum;


            MarkUnsaved();
        }


        private void btnMaxTP_Click(
            object? sender,
            EventArgs e)
        {
            nudTPMedals.Value =
                nudTPMedals.Maximum;


            MarkUnsaved();
        }


        private void btnMaxStats_Click(
            object? sender,
            EventArgs e)
        {
            const decimal maxStat =
                200;


            nudHealth.Value =
                maxStat;

            nudKi.Value =
                maxStat;

            nudStamina.Value =
                maxStat;

            nudBasicAttack.Value =
                maxStat;

            nudStrikeSupers.Value =
                maxStat;

            nudKiBlastSupers.Value =
                maxStat;


            MarkUnsaved();
        }


        // =========================================================
        // RESET CHARACTER
        // =========================================================

        private void btnResetCharacter_Click(
            object? sender,
            EventArgs e)
        {
            LoadSelectedCharacter();


            hasUnsavedChanges =
                false;


            UpdateWindowTitle();
        }


        private void btnResetAppearance_Click(
            object? sender,
            EventArgs e)
        {
            if (
                loadedCharacter
                    is XV2Character character &&
                character.Appearance != null)
            {
                isLoadingControls =
                    true;


                try
                {
                    LoadAppearance(
                        character.Appearance
                    );
                }
                finally
                {
                    isLoadingControls =
                        false;
                }


                MarkUnsaved();
            }
        }


        // =========================================================
        // COPY STATS
        // =========================================================

        private void btnCopyStats_Click(
            object? sender,
            EventArgs e)
        {
            if (copiedStats == null)
            {
                copiedStats =
                    new XV2Character
                    {
                        Level =
                            (int)nudCharacterLevel.Value,

                        Experience =
                            (int)nudCharacterXP.Value,

                        AttributePoints =
                            (int)nudAttributePoints.Value,

                        Health =
                            (int)nudHealth.Value,

                        Ki =
                            (int)nudKi.Value,

                        Stamina =
                            (int)nudStamina.Value,

                        BasicAttack =
                            (int)nudBasicAttack.Value,

                        StrikeSupers =
                            (int)nudStrikeSupers.Value,

                        KiBlastSupers =
                            (int)nudKiBlastSupers.Value
                    };


                btnCopyStats.Text =
                    "Paste Stats";


                MessageBox.Show(
                    "Stats copied.\n\n" +
                    "Select another character and click Paste Stats.",
                    "XV2 Save Editor"
                );


                return;
            }


            isLoadingControls =
                true;


            try
            {
                SetNumericValue(
                    nudCharacterLevel,
                    copiedStats.Level
                );


                SetNumericValue(
                    nudCharacterXP,
                    copiedStats.Experience
                );


                SetNumericValue(
                    nudAttributePoints,
                    copiedStats.AttributePoints
                );


                SetNumericValue(
                    nudHealth,
                    copiedStats.Health
                );


                SetNumericValue(
                    nudKi,
                    copiedStats.Ki
                );


                SetNumericValue(
                    nudStamina,
                    copiedStats.Stamina
                );


                SetNumericValue(
                    nudBasicAttack,
                    copiedStats.BasicAttack
                );


                SetNumericValue(
                    nudStrikeSupers,
                    copiedStats.StrikeSupers
                );


                SetNumericValue(
                    nudKiBlastSupers,
                    copiedStats.KiBlastSupers
                );
            }
            finally
            {
                isLoadingControls =
                    false;
            }


            copiedStats =
                null;


            btnCopyStats.Text =
                "Copy Stats";


            MarkUnsaved();
        }


        // =========================================================
        // COPY APPEARANCE
        // =========================================================

        private void btnCopyAppearance_Click(
            object? sender,
            EventArgs e)
        {
            if (copiedAppearance == null)
            {
                copiedAppearance =
                    ReadAppearanceFromControls();


                btnCopyAppearance.Text =
                    "Paste Appearance";


                MessageBox.Show(
                    "Appearance copied.\n\n" +
                    "Select another character and click Paste Appearance.",
                    "XV2 Save Editor"
                );


                return;
            }


            isLoadingControls =
                true;


            try
            {
                LoadAppearance(
                    copiedAppearance
                );
            }
            finally
            {
                isLoadingControls =
                    false;
            }


            copiedAppearance =
                null;


            btnCopyAppearance.Text =
                "Copy Appearance";


            MarkUnsaved();
        }


        // =========================================================
        // SPECIAL APPEARANCE
        // =========================================================

        private void btnSSJ3Hair_Click(
            object? sender,
            EventArgs e)
        {
            nudHair.Value =
                299;


            MarkUnsaved();
        }


        private void btnMaleBlackEyes_Click(
            object? sender,
            EventArgs e)
        {
            nudEyes.Value =
                104;

            nudForehead.Value =
                50;


            MarkUnsaved();
        }


        private void btnFemaleBlackEyes_Click(
            object? sender,
            EventArgs e)
        {
            nudEyes.Value =
                167;

            nudForehead.Value =
                74;


            MarkUnsaved();
        }


        private void btnUltraInstinctEyes_Click(
            object? sender,
            EventArgs e)
        {
            nudEyes.Value =
                1201;


            MarkUnsaved();
        }


        private void btnTimeBreakerMask_Click(
            object? sender,
            EventArgs e)
        {
            nudForehead.Value =
                800;


            MarkUnsaved();
        }


        // =========================================================
        // DARK MODE - RETIRED
        // =========================================================

        private void btnDarkMode_Click(
            object? sender,
            EventArgs e)
        {
            // Kept because Designer still references this.
        }


        // =========================================================
        // UNSAVED STATE
        // =========================================================

        private void AnyValueChanged(
            object? sender,
            EventArgs e)
        {
            if (
                !isLoadingControls)
            {
                MarkUnsaved();
            }
        }


        private void MarkUnsaved()
        {
            if (
                currentSave == null)
            {
                return;
            }


            hasUnsavedChanges =
                true;


            UpdateWindowTitle();

            ScheduleHistoryCapture();

        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!hasUnsavedChanges) return;
            if (MessageBox.Show("Close and discard all unsaved changes?", "Unsaved changes",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                e.Cancel = true;
        }


        private void UpdateWindowTitle()
        {
            string filename =
                currentSave == null
                    ? "No Save Loaded"
                    : Path.GetFileName(
                        currentSave.FilePath
                    );


            string dirty =
                hasUnsavedChanges
                    ? " *"
                    : "";


            Text =
                "Dragon Ball Xenoverse 2 Save Editor - " +
                $"{filename}{dirty}";

            if (lblModernSubtitle != null)
                lblModernSubtitle.Text = currentSave == null
                    ? "No save loaded"
                    : $"{filename}  •  {PlatformSaveAdapter.DisplayName(currentSave.Platform)}" + (hasUnsavedChanges ? "  •  Unsaved changes" : "  •  All changes saved");
        }

        private void ConfigureModernInterface()
        {
            SuspendLayout();
            try
            {
                BackColor = ModernTheme.Window;
                ClientSize = new System.Drawing.Size(1400, 900);
                MinimumSize = new System.Drawing.Size(1240, 820);
                StartPosition = FormStartPosition.CenterScreen;

                grpGeneral.Location = new System.Drawing.Point(230, 88);
                grpGeneral.Size = new System.Drawing.Size(1150, 82);
                grpGeneral.Text = "  SAVE CONTROL  ";
                grpGeneral.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                lblZeniTitle.AutoSize = false;
                lblZeniTitle.Location = new System.Drawing.Point(565, 27);
                lblZeniTitle.Size = new System.Drawing.Size(65, 29);
                lblZeniTitle.TextAlign = ContentAlignment.MiddleRight;
                nudZeni.Location = new System.Drawing.Point(640, 30);
                nudZeni.Size = new System.Drawing.Size(130, 25);
                btnMaxZeni.Location = new System.Drawing.Point(780, 29);
                btnMaxZeni.Size = new System.Drawing.Size(52, 28);
                lblTPMedalsTitle.AutoSize = false;
                lblTPMedalsTitle.Location = new System.Drawing.Point(840, 27);
                lblTPMedalsTitle.Size = new System.Drawing.Size(82, 29);
                lblTPMedalsTitle.TextAlign = ContentAlignment.MiddleRight;
                nudTPMedals.Location = new System.Drawing.Point(932, 30);
                nudTPMedals.Size = new System.Drawing.Size(120, 25);
                btnMaxTP.Location = new System.Drawing.Point(1062, 29);
                btnMaxTP.Size = new System.Drawing.Size(48, 28);

                tabMain.Location = new System.Drawing.Point(230, 184);
                tabMain.Size = new System.Drawing.Size(1150, 696);
                tabMain.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                tabMain.Appearance = TabAppearance.FlatButtons;
                tabMain.SizeMode = TabSizeMode.Fixed;
                tabMain.ItemSize = new System.Drawing.Size(0, 1);
                tabMain.Multiline = true;

                Panel header = new Panel { Location = new System.Drawing.Point(230, 12), Size = new System.Drawing.Size(1150, 62), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, BackColor = ModernTheme.Window };
                Panel headerRail = new Panel { Location = new System.Drawing.Point(0, 8), Size = new System.Drawing.Size(4, 42), BackColor = ModernTheme.Cyan };
                lblModernTitle = new Label { Text = "CAC CUSTOMISATION", AutoSize = true, Location = new System.Drawing.Point(18, 3), Font = new System.Drawing.Font("Segoe UI Semibold", 17F), ForeColor = ModernTheme.Text };
                lblModernSubtitle = new Label { Text = "NO SAVE CONNECTED", AutoSize = true, Location = new System.Drawing.Point(20, 38), Font = new System.Drawing.Font("Segoe UI Semibold", 8F), ForeColor = ModernTheme.Muted };
                Label buildBadge = new Label { Text = "CAPSULE SYSTEM  /  VERIFIED", AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Location = new System.Drawing.Point(900, 13), Size = new System.Drawing.Size(235, 30), Anchor = AnchorStyles.Top | AnchorStyles.Right, Font = new System.Drawing.Font("Segoe UI Semibold", 8F), ForeColor = ModernTheme.Cyan, BackColor = ModernTheme.Surface };
                header.Controls.AddRange(new Control[] { headerRail, lblModernTitle, lblModernSubtitle, buildBadge });
                ConfigureEditorHelp(header);
                Controls.Add(header);

                Panel sidebar = new Panel { Location = new System.Drawing.Point(0, 0), Size = new System.Drawing.Size(210, 900), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left, BackColor = ModernTheme.Sidebar, Padding = new Padding(14) };
                Panel brandMark = new Panel { Location = new System.Drawing.Point(18, 18), Size = new System.Drawing.Size(7, 46), BackColor = ModernTheme.Purple };
                Label brand = new Label { Text = "CAPSULE", Location = new System.Drawing.Point(38, 16), AutoSize = true, Font = new System.Drawing.Font("Segoe UI Black", 17F), ForeColor = ModernTheme.Text };
                Label brandSub = new Label { Text = "XV2  /  CONTROL", Location = new System.Drawing.Point(40, 48), AutoSize = true, Font = new System.Drawing.Font("Segoe UI Semibold", 8F), ForeColor = ModernTheme.Cyan };
                Label navCaption = new Label { Text = "SYSTEM MODULES", Location = new System.Drawing.Point(18, 91), AutoSize = true, Font = new System.Drawing.Font("Segoe UI Semibold", 7.5F), ForeColor = ModernTheme.Muted };
                sidebar.Controls.AddRange(new Control[] { brandMark, brand, brandSub, navCaption });
                string[] names = { "01   CAC PROFILE", "02   CAC MANAGER", "03   CAC PRESETS", "04   INVENTORY / QQ", "05   PROGRESS", "06   PLAY DATA", "07   MENTOR LAB", "08   DIAGNOSTICS", "09   CHANGE HISTORY", "10   BACKUP RECOVERY", "11   SAVE COMPARISON", "12   MAX-OUT WIZARD", "13   DASHBOARD" };
                for (int i = 0; i < names.Length; i++)
                {
                    int index = i;
                    Button nav = new Button { Text = names[i], TextAlign = ContentAlignment.MiddleLeft, Location = new System.Drawing.Point(14, 118 + i * 54), Size = new System.Drawing.Size(182, 44), Padding = new Padding(14, 0, 0, 0), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F) };
                    nav.FlatAppearance.BorderSize = 0;
                    nav.Click += (_, _) => { tabMain.SelectedIndex = index; UpdateModernNavigation(); };
                    navigationButtons.Add(nav); sidebar.Controls.Add(nav);
                }
                Panel statusDot = new Panel { Location = new System.Drawing.Point(20, 838), Size = new System.Drawing.Size(7, 7), Anchor = AnchorStyles.Left | AnchorStyles.Bottom, BackColor = ModernTheme.Cyan };
                Label safety = new Label { Text = "SAFE WRITE MODE", AutoSize = true, Location = new System.Drawing.Point(36, 834), Anchor = AnchorStyles.Left | AnchorStyles.Bottom, ForeColor = ModernTheme.Muted, Font = new System.Drawing.Font("Segoe UI Semibold", 7.5F) };
                Label edition = new Label { Text = "MADE WITH LOVE BY: DEMYLICIOUSS\nWITH HELP FROM: GLISCORS", AutoSize = true, Location = new System.Drawing.Point(20, 856), Anchor = AnchorStyles.Left | AnchorStyles.Bottom, ForeColor = ModernTheme.Border, Font = new System.Drawing.Font("Segoe UI", 7F) };
                sidebar.Controls.AddRange(new Control[] { statusDot, safety, edition });
                Controls.Add(sidebar);
                sidebar.BringToFront(); header.BringToFront();

                AlignPresetSelection();
                ExpandPresetTools();
                RemoveBlastFromPresetEditor();
                AlignInventoryAndQQBangEditor();
                ConfigureResponsiveDashboardLayouts();
                LayoutSaveControlBar();

                ModernTheme.Apply(this);
                ConfigureAutoCalculationToggles();
                ModernTheme.StyleButton(btnSave, true);
                btnSave.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
                btnOpenSave.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
                tabMain.SelectedIndexChanged += (_, _) => UpdateModernNavigation();
                UpdateModernNavigation();
            }
            finally { ResumeLayout(true); }
        }

        private void AlignPresetSelection()
        {
            lblPresetCharacterTitle.AutoSize = false;
            lblPresetCharacterTitle.Location = new System.Drawing.Point(20, 25);
            lblPresetCharacterTitle.Size = new System.Drawing.Size(80, 30);
            lblPresetCharacterTitle.TextAlign = ContentAlignment.MiddleRight;
            cmbPresetCharacters.Location = new System.Drawing.Point(110, 28);
            cmbPresetCharacters.Size = new System.Drawing.Size(280, 25);

            lblPresetTitle.AutoSize = false;
            lblPresetTitle.Location = new System.Drawing.Point(410, 25);
            lblPresetTitle.Size = new System.Drawing.Size(65, 30);
            lblPresetTitle.TextAlign = ContentAlignment.MiddleRight;
            cmbPresets.Location = new System.Drawing.Point(485, 28);
            cmbPresets.Size = new System.Drawing.Size(180, 25);
            lblNameDatabaseStatus.Location = new System.Drawing.Point(710, 33);
        }

        private void ExpandPresetTools()
        {
            grpPresetTools.Location = new System.Drawing.Point(15, 535);
            grpPresetTools.Size = new System.Drawing.Size(1055, 112);
            Button[] buttons = { btnCopyPreset, btnPastePreset, btnCopyOutfit, btnPasteOutfit, btnCopySkills, btnPasteSkills, btnResetPreset, btnExportCac, btnImportCac, btnEmptyAllPresets };
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Location = new System.Drawing.Point(16 + (i % 5) * 205, 27 + (i / 5) * 39);
                buttons[i].Size = new System.Drawing.Size(194, 34);
            }
        }

        private void RemoveBlastFromPresetEditor()
        {
            // Blast is derived from the equipped Super Soul. Keep the hidden
            // control loaded so existing data round-trips unchanged.
            lblBlastSkillTitle.Visible = false;
            nudBlastSkill.Visible = false;
            lblAwokenSkillTitle.Location = new System.Drawing.Point(20, 338);
            cmbAwokenSkill.Location = new System.Drawing.Point(120, 335);
            grpPresetSkills.Size = new System.Drawing.Size(grpPresetSkills.Width, 380);
        }

        private void AlignInventoryAndQQBangEditor()
        {
            AlignFieldLabel(lblInventoryCategoryTitle, 15, 24, 75);
            cmbInventoryCategory.Location = new System.Drawing.Point(100, 28);
            AlignFieldLabel(lblInventorySearchTitle, 335, 24, 65);
            txtInventorySearch.Location = new System.Drawing.Point(410, 28);
            btnInventoryClearSearch.Location = new System.Drawing.Point(565, 27);
            chkInventoryBulkFiltered.Location = new System.Drawing.Point(410, 64);

            Label[] detailTitles =
            {
                lblInventoryNameTitle, lblInventoryIdTitle, lblInventorySlotTitle,
                lblInventoryTypeTitle, lblInventoryQuantityTitle,
                lblInventoryI06Title, lblInventoryI07Title
            };
            foreach (Label label in detailTitles)
                AlignFieldLabel(label, 15, label.Top - 7, 100);

            Control[] detailValues =
            {
                lblInventoryNameValue, lblInventoryIdValue, lblInventorySlotValue,
                lblInventoryTypeValue, lblInventoryQuantityValue,
                lblInventoryI06Value, lblInventoryI07Value
            };
            foreach (Control value in detailValues)
                value.Location = new System.Drawing.Point(125, value.Top - 2);

            if (nudQQBangStats != null)
            {
                foreach (NumericUpDown stat in nudQQBangStats)
                {
                    Label? label = stat.Parent?.Controls.OfType<Label>()
                        .FirstOrDefault(x => Math.Abs(x.Top - stat.Top) < 8 && x.Left < stat.Left);
                    if (label != null) AlignFieldLabel(label, 8, stat.Top - 3, 135);
                    stat.Location = new System.Drawing.Point(155, stat.Top);
                    stat.Size = new System.Drawing.Size(90, 25);
                }
                AlignFieldLabel(nudQQBangQuantity.Parent!.Controls.OfType<Label>().First(x => x.Text == "Quantity:"), 8, nudQQBangQuantity.Top - 3, 135);
                nudQQBangQuantity.Size = new System.Drawing.Size(90, 25);
            }
        }

        private static void AlignFieldLabel(Label label, int x, int y, int width)
        {
            label.AutoSize = false;
            label.Location = new System.Drawing.Point(x, y);
            label.Size = new System.Drawing.Size(width, 30);
            label.TextAlign = ContentAlignment.MiddleRight;
        }

        private void ConfigureAutoCalculationToggles()
        {
            ConfigureAutoToggle(chkAutoXP, new System.Drawing.Point(275, 139));
            ConfigureAutoToggle(chkAutoAttributePoints, new System.Drawing.Point(275, 214));
            UpdateAutoToggleVisuals();
        }

        private static void ConfigureAutoToggle(CheckBox toggle, System.Drawing.Point location)
        {
            toggle.Appearance = Appearance.Button;
            toggle.AutoSize = false;
            toggle.AutoCheck = true;
            toggle.TabStop = true;
            toggle.Location = location;
            toggle.Size = new System.Drawing.Size(115, 29);
            toggle.TextAlign = ContentAlignment.MiddleCenter;
            toggle.FlatStyle = FlatStyle.Flat;
            toggle.FlatAppearance.BorderSize = 1;
            toggle.FlatAppearance.BorderColor = ModernTheme.Border;
            toggle.Cursor = Cursors.Hand;
            toggle.BringToFront();
        }

        private void UpdateAutoToggleVisuals()
        {
            foreach (CheckBox toggle in new[] { chkAutoXP, chkAutoAttributePoints })
            {
                toggle.BackColor = toggle.Checked ? ModernTheme.Purple : ModernTheme.SurfaceRaised;
                toggle.ForeColor = Color.White;
                toggle.FlatAppearance.BorderColor = toggle.Checked ? ModernTheme.PurpleHover : ModernTheme.Border;
            }
        }

        private void UpdateModernNavigation()
        {
            string[] pageTitles = { "CAC CUSTOMISATION", "CAC MANAGEMENT HUB", "CAC PRESET LOADOUTS", "INVENTORY / QQ BANGS", "PROGRESS / UNLOCKS", "PLAY DATA", "MENTOR CUSTOMISATION", "SAVE DIAGNOSTICS", "CHANGE HISTORY", "BACKUP & RECOVERY", "SAVE COMPARISON", "VERIFIED MAX-OUT WIZARD", "SAVE DASHBOARD" };
            for (int i = 0; i < navigationButtons.Count; i++)
            {
                Button button = navigationButtons[i];
                bool selected = i == tabMain.SelectedIndex;
                button.BackColor = selected ? ModernTheme.SurfaceRaised : ModernTheme.Sidebar;
                button.ForeColor = selected ? ModernTheme.Cyan : ModernTheme.Muted;
                button.FlatAppearance.BorderSize = selected ? 1 : 0;
                button.FlatAppearance.BorderColor = selected ? ModernTheme.Purple : ModernTheme.Sidebar;
            }
            if (lblModernTitle != null && tabMain.SelectedIndex >= 0 && tabMain.SelectedIndex < pageTitles.Length)
                lblModernTitle.Text = pageTitles[tabMain.SelectedIndex];
        }


        // =========================================================
        // ENABLE / DISABLE
        // =========================================================

        private void SetEditorEnabled(
            bool enabled)
        {
            btnSave.Enabled =
                enabled;


            nudZeni.Enabled =
                enabled;

            nudTPMedals.Enabled =
                enabled;


            btnMaxZeni.Enabled =
                enabled;

            btnMaxTP.Enabled =
                enabled;


            grpCharacter.Enabled =
                enabled;

            chkAutoXP.Enabled = enabled;
            chkAutoAttributePoints.Enabled = enabled;

            grpAppearance.Enabled =
                enabled;


            grpPresetSelection.Enabled =
                enabled;

            grpPresetEquipment.Enabled =
                enabled;

            grpPresetSkills.Enabled =
                enabled;

            grpPresetColors.Enabled =
                enabled;

            grpPresetTools.Enabled =
                enabled;

            if (btnImportCac != null) btnImportCac.Enabled = enabled;
            if (btnExportCac != null) btnExportCac.Enabled = enabled;
            if (btnEmptyAllPresets != null) btnEmptyAllPresets.Enabled = enabled;


            grpInventoryBrowser.Enabled =
                enabled;

            grpInventoryDetails.Enabled =
                enabled;

            grpQQBangEditor.Enabled =
                enabled;


            UpdatePresetToolButtons();
        }

        // =========================================================
        // QQ BANG EDITOR
        // =========================================================

        private void ConfigureSkillUnlockEditor()
        {
            tabProgress.Controls.Clear();
            TabControl progressTabs = new TabControl { Dock = DockStyle.Fill };
            TabPage skillsPage = new TabPage("Skills / Awokens");
            questProgressPage = new TabPage("Quest Progress");
            Panel top = new Panel { Dock = DockStyle.Top, Height = 82, Padding = new Padding(10) };
            Panel bottom = new Panel { Dock = DockStyle.Bottom, Height = 58, Padding = new Padding(10) };
            txtSkillSearch = new TextBox { Location = new System.Drawing.Point(10, 31), Width = 420, PlaceholderText = "Search skill name" };
            cmbSkillView = new ComboBox { Location = new System.Drawing.Point(445, 31), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSkillView.Items.AddRange(new object[] { "All", "Owned", "Missing" });
            cmbSkillView.SelectedIndex = 0;
            lblSkillCounts = new Label { Location = new System.Drawing.Point(615, 34), AutoSize = true, Text = "No save loaded" };
            top.Controls.Add(new Label { Location = new System.Drawing.Point(10, 9), AutoSize = true, Text = "Verified skill ownership — checked means owned" });
            top.Controls.AddRange(new Control[] { txtSkillSearch, cmbSkillView, lblSkillCounts });
            btnUnlockSelected = MakeSkillButton("Unlock selected", 10, (_, _) => UnlockSelectedSkill());
            btnUnlockVisible = MakeSkillButton("Unlock visible", 160, (_, _) => UnlockVisibleSkills());
            btnUnlockAllSkills = MakeSkillButton("Unlock all verified", 310, (_, _) => UnlockAllVerifiedSkills());
            btnRevertSkills = MakeSkillButton("Revert to load snapshot", 480, (_, _) => RevertSkillOwnership());
            bottom.Controls.AddRange(new Control[] { btnUnlockSelected, btnUnlockVisible, btnUnlockAllSkills, btnRevertSkills });
            tabSkillCategories = new TabControl { Dock = DockStyle.Fill };
            foreach ((NamedValueKind category, string title) in new[]
            {
                (NamedValueKind.SuperSkill, "Supers"), (NamedValueKind.UltimateSkill, "Ultimates"),
                (NamedValueKind.EvasiveSkill, "Evasives"), (NamedValueKind.AwokenSkill, "Awokens")
            })
            {
                TabPage categoryPage = new TabPage(title);
                CheckedListBox list = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true, HorizontalScrollbar = true };
                list.ItemCheck += SkillOwnership_ItemCheck;
                categoryPage.Controls.Add(list); tabSkillCategories.TabPages.Add(categoryPage); skillCategoryLists[category] = list;
            }
            lstSkillOwnership = skillCategoryLists[NamedValueKind.SuperSkill];
            skillsPage.Controls.Add(tabSkillCategories);
            skillsPage.Controls.Add(bottom);
            skillsPage.Controls.Add(top);
            progressTabs.TabPages.Add(skillsPage);
            progressTabs.TabPages.Add(questProgressPage);
            progressTabs.TabPages.Add(BuildTokipediaPage());
            progressTabs.TabPages.Add(BuildMentorProgressPage());
            progressTabs.TabPages.Add(BuildCollectionsPage());
            progressTabs.TabPages.Add(BuildSystemFlagsPage());
            tabProgress.Controls.Add(progressTabs);
            tabMentorCustomisation = BuildPartnerStatsPage();
            int progressIndex = tabMain.TabPages.IndexOf(tabProgress);
            tabMain.TabPages.Insert(progressIndex + 1, tabMentorCustomisation);
            txtSkillSearch.TextChanged += (_, _) => RefreshSkillOwnershipList();
            cmbSkillView.SelectedIndexChanged += (_, _) => RefreshSkillOwnershipList();
            tabSkillCategories.SelectedIndexChanged += (_, _) => { lstSkillOwnership = ActiveSkillList(); RefreshSkillOwnershipList(); };
            SetSkillButtons(false);
            ConfigureQuestProgressEditor();
        }

        private TabPage BuildMentorProgressPage()
        {
            TabPage page = new TabPage("Mentors");
            Panel top = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(10) };
            cmbMentorCharacter = new ComboBox { Location = new System.Drawing.Point(10, 31), Width = 210, DropDownStyle = ComboBoxStyle.DropDownList };
            txtMentorSearch = new TextBox { Location = new System.Drawing.Point(235, 31), Width = 260, PlaceholderText = "Search mentor" };
            lblMentorCounts = new Label { Location = new System.Drawing.Point(515, 34), AutoSize = true, Text = "No save loaded" };
            top.Controls.Add(new Label { Location = new System.Drawing.Point(10, 8), AutoSize = true, Text = "Verified per-CaC mentor gauges — Advancement Tests remain unsupported" });
            top.Controls.AddRange(new Control[] { cmbMentorCharacter, txtMentorSearch, lblMentorCounts });
            lstMentorGauges = new ListBox { Dock = DockStyle.Fill, HorizontalScrollbar = true };
            Panel actions = new Panel { Dock = DockStyle.Bottom, Height = 105, Padding = new Padding(10) };
            nudMentorFriendship = new NumericUpDown { Location = new System.Drawing.Point(10, 15), Width = 90, Minimum = 0, Maximum = 100 };
            nudMentorDual = new NumericUpDown { Location = new System.Drawing.Point(115, 15), Width = 90, Minimum = 0, Maximum = 100 };
            Button apply = MakeQuestButton("Apply selected", 220, (_, _) => ApplySelectedMentor());
            Button maxVisible = MakeQuestButton("Max visible", 385, (_, _) => MaxVisibleMentors());
            Button revert = MakeQuestButton("Revert snapshot", 550, (_, _) => RevertProgression());
            actions.Controls.AddRange(new Control[] { nudMentorFriendship, nudMentorDual, apply, maxVisible, revert });
            actions.Controls.Add(new Label { Location = new System.Drawing.Point(10, 67), AutoSize = true, Text = "Friendship       Dual Ultimate       Existing mentor flags are preserved." });
            page.Controls.Add(lstMentorGauges); page.Controls.Add(actions); page.Controls.Add(top);
            cmbMentorCharacter.SelectedIndexChanged += (_, _) => RefreshMentors();
            txtMentorSearch.TextChanged += (_, _) => RefreshMentors();
            lstMentorGauges.SelectedIndexChanged += (_, _) => LoadSelectedMentorGauge();
            return page;
        }

        private TabPage BuildCollectionsPage()
        {
            TabPage page = new TabPage("Artwork / Mascots");
            Panel top = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(10) };
            cmbCollectionCategory = new ComboBox { Location = new System.Drawing.Point(10, 31), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCollectionCategory.Items.AddRange(new object[] { "Artwork", "Mascots" }); cmbCollectionCategory.SelectedIndex = 0;
            cmbCollectionView = new ComboBox { Location = new System.Drawing.Point(175, 31), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCollectionView.Items.AddRange(new object[] { "All", "Owned", "Missing" }); cmbCollectionView.SelectedIndex = 0;
            txtCollectionSearch = new TextBox { Location = new System.Drawing.Point(320, 31), Width = 230, PlaceholderText = "Search name" };
            lblCollectionCounts = new Label { Location = new System.Drawing.Point(570, 34), AutoSize = true, Text = "No save loaded" };
            top.Controls.Add(new Label { Location = new System.Drawing.Point(10, 8), AutoSize = true, Text = "Verified collection bitfields — checked means unlocked" });
            top.Controls.AddRange(new Control[] { cmbCollectionCategory, cmbCollectionView, txtCollectionSearch, lblCollectionCounts });
            lstCollectionUnlocks = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true, HorizontalScrollbar = true };
            Panel actions = new Panel { Dock = DockStyle.Bottom, Height = 58, Padding = new Padding(10) };
            Button selected = MakeQuestButton("Unlock selected", 10, (_, _) => UnlockSelectedCollection());
            Button visible = MakeQuestButton("Unlock visible", 175, (_, _) => UnlockVisibleCollection());
            Button all = MakeQuestButton("Unlock category", 340, (_, _) => UnlockAllCollection());
            Button photo = MakeQuestButton("Unlock Photo Mode", 505, (_, _) => UnlockPhotoMode());
            Button revert = MakeQuestButton("Revert snapshot", 670, (_, _) => RevertProgression());
            actions.Controls.AddRange(new Control[] { selected, visible, all, photo, revert });
            page.Controls.Add(lstCollectionUnlocks); page.Controls.Add(actions); page.Controls.Add(top);
            cmbCollectionCategory.SelectedIndexChanged += (_, _) => RefreshCollections();
            cmbCollectionView.SelectedIndexChanged += (_, _) => RefreshCollections();
            txtCollectionSearch.TextChanged += (_, _) => RefreshCollections();
            lstCollectionUnlocks.ItemCheck += Collection_ItemCheck;
            return page;
        }

        private TabPage BuildPartnerStatsPage()
        {
            TabPage page = new TabPage("Mentor Customisation");
            Panel top = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(10) };
            cmbPartnerCharacter = new ComboBox { Location = new System.Drawing.Point(10, 31), Width = 210, DropDownStyle = ComboBoxStyle.DropDownList };
            txtPartnerSearch = new TextBox { Location = new System.Drawing.Point(235, 31), Width = 260, PlaceholderText = "Search custom partner" };
            Button clear = new Button { Text = "Clear", Location = new System.Drawing.Point(505, 29), Size = new System.Drawing.Size(75, 28) };
            lblPartnerCounts = new Label { Location = new System.Drawing.Point(595, 34), AutoSize = true, Text = "No save loaded" };
            top.Controls.Add(new Label { Location = new System.Drawing.Point(10, 8), AutoSize = true, Text = "Stat Type only — displays materialized partner records and preserves all other fields" });
            top.Controls.AddRange(new Control[] { cmbPartnerCharacter, txtPartnerSearch, clear, lblPartnerCounts });
            lstPartnerStats = new ListBox { Dock = DockStyle.Fill, HorizontalScrollbar = true };
            Panel actions = new Panel { Dock = DockStyle.Bottom, Height = 112, Padding = new Padding(10) };
            cmbPartnerStatType = new ComboBox { Location = new System.Drawing.Point(10, 15), Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (PartnerStatPreset preset in ProgressionUnlockAccess.PartnerStatPresets) cmbPartnerStatType.Items.Add(preset);
            Button apply = MakeQuestButton("Apply selected", 285, (_, _) => ApplyPartnerStat());
            Button visible = MakeQuestButton("Apply to visible", 450, (_, _) => ApplyVisiblePartnerStats());
            Button revert = MakeQuestButton("Revert snapshot", 615, (_, _) => RevertProgression());
            Button festival = MakeQuestButton("Unlock all Festival presets", 10, (_, _) => UnlockFestivalPresets());
            festival.Location = new System.Drawing.Point(10, 62);
            festival.Size = new System.Drawing.Size(220, 34);
            Button initialize = MakeQuestButton("Unlock + initialize all", 245, (_, _) => UnlockAndInitializeAllPartners());
            initialize.Location = new System.Drawing.Point(245, 62);
            initialize.Size = new System.Drawing.Size(220, 34);
            actions.Controls.AddRange(new Control[] { cmbPartnerStatType, apply, visible, revert, festival, initialize });
            page.Controls.Add(lstPartnerStats); page.Controls.Add(actions); page.Controls.Add(top);
            cmbPartnerCharacter.SelectedIndexChanged += (_, _) => RefreshPartnerStats();
            txtPartnerSearch.TextChanged += (_, _) => RefreshPartnerStats();
            lstPartnerStats.SelectedIndexChanged += (_, _) => LoadSelectedPartnerStat();
            clear.Click += (_, _) => txtPartnerSearch.Clear();
            return page;
        }

        private void UnlockFestivalPresets()
        {
            if (currentSave == null) return;
            using CacTargetPicker picker = new(currentSave.Characters, currentSave.Characters.Where(x => !x.IsEmpty).Select(x => x.Slot - 1), "Festival preset unlock");
            if (picker.ShowDialog(this) != DialogResult.OK) return;
            if (MessageBox.Show($"Unlock every verified Festival mentor preset for {picker.SelectedSlots.Count} selected CaC(s)?\n\nMissing Festival partner records will be initialized. Existing mentor customisation records are preserved.", "Festival Presets", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                var result = FestivalPresetAccess.Unlock(currentSave.DecryptedData, picker.SelectedSlots);
                PartnerCustomizationInitializer.UnlockAllOptions(currentSave.DecryptedData);
                MarkUnsaved();
                RefreshPartnerStats();
                MessageBox.Show($"Festival presets unlocked.\n\nInitialized partner records: {result.RecordsInitialized}\nUpdated unlock bytes: {result.FlagsChanged}", "Festival Presets", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show($"Could not unlock Festival presets:\n\n{ex.Message}", "Festival Presets - Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void UnlockAndInitializeAllPartners()
        {
            if (currentSave == null) return;
            using CacTargetPicker picker = new(currentSave.Characters, currentSave.Characters.Where(character => !character.IsEmpty).Select(character => character.Slot - 1), "mentor initialization");
            if (picker.ShowDialog(this) != DialogResult.OK) return;
            if (MessageBox.Show($"Give all Customization Keys and initialize every verified regular, key, DLC, and Festival partner record for {picker.SelectedSlots.Count} selected CaC(s)?\n\nExisting materialized records and their loadouts are preserved.",
                "Unlock + Initialize Mentors", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                int keys = PartnerKeyAccess.GiveAll(currentSave.DecryptedData);
                int initialized = PartnerCustomizationInitializer.Initialize(currentSave.DecryptedData, picker.SelectedSlots);
                int optionBytes = PartnerCustomizationInitializer.UnlockAllOptions(currentSave.DecryptedData);
                var festival = FestivalPresetAccess.Unlock(currentSave.DecryptedData, picker.SelectedSlots);
                MarkUnsaved();
                RefreshPartnerStats();
                RefreshInventoryList();
                MessageBox.Show($"Mentor customization is ready.\n\nMissing keys added: {keys}\nRegular/DLC records initialized: {initialized}\nFestival records initialized: {festival.RecordsInitialized}\nCustomization option bytes updated: {optionBytes}",
                    "Mentors Initialized", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not initialize mentor customization:\n\n{ex.Message}", "Mentor Initialization - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureQuestProgressEditor()
        {
            Panel filters = new Panel { Dock = DockStyle.Top, Height = 82, Padding = new Padding(10) };
            Panel actions = new Panel { Dock = DockStyle.Bottom, Height = 142, Padding = new Padding(10) };
            lstQuestProgress = new ListBox { Dock = DockStyle.Fill, HorizontalScrollbar = true };
            cmbQuestCharacter = new ComboBox { Location = new System.Drawing.Point(10, 31), Width = 190, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbQuestCategory = new ComboBox { Location = new System.Drawing.Point(215, 31), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbQuestCategory.Items.Add("All categories");
            cmbQuestCategory.Items.AddRange(Enum.GetNames<XV2QuestCategory>());
            cmbQuestCategory.SelectedIndex = 0;
            cmbQuestView = new ComboBox { Location = new System.Drawing.Point(380, 31), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbQuestView.Items.AddRange(new object[] { "All states", "Cleared", "Not cleared", "Locked" });
            cmbQuestView.SelectedIndex = 0;
            txtQuestSearch = new TextBox { Location = new System.Drawing.Point(525, 31), Width = 190, PlaceholderText = "Search ID or category" };
            lblQuestCounts = new Label { Location = new System.Drawing.Point(730, 34), AutoSize = true, Text = "No save loaded" };
            filters.Controls.Add(new Label { Location = new System.Drawing.Point(10, 9), AutoSize = true, Text = "Existing quest records only — completion, rank, and score are independent" });
            filters.Controls.AddRange(new Control[] { cmbQuestCharacter, cmbQuestCategory, cmbQuestView, txtQuestSearch, lblQuestCounts });
            Button clear = MakeQuestButton("Clear selected", 10, (_, _) => EditQuestSelection(q => q.State = 3));
            cmbQuestRank = new ComboBox { Location = new System.Drawing.Point(175, 14), Width = 115, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbQuestRank.Items.AddRange(new object[] { "No rank", "D", "C", "B", "A", "S", "Z", "Super" });
            cmbQuestRank.SelectedIndex = 6;
            Button rank = MakeQuestButton("Apply rank", 300, (_, _) => EditQuestSelection(q => q.Rank = cmbQuestRank.SelectedIndex));
            nudQuestScore = new NumericUpDown { Location = new System.Drawing.Point(465, 14), Width = 120, Maximum = 999999999, ThousandsSeparator = true };
            Button score = MakeQuestButton("Apply score", 595, (_, _) => EditQuestSelection(q => q.Score = (int)nudQuestScore.Value));
            Button revert = MakeQuestButton("Revert quests", 760, (_, _) => RevertQuestProgress());
            Button completeVisible = MakeQuestButton("Clear + rank visible", 10, (_, _) => CompleteVisibleQuests(cmbQuestRank.SelectedIndex));
            completeVisible.Location = new System.Drawing.Point(10, 54);
            Button completeAll = MakeQuestButton("Clear + rank all", 175, (_, _) => CompleteAllQuests(cmbQuestRank.SelectedIndex));
            completeAll.Location = new System.Drawing.Point(175, 54);
            Button guru = MakeQuestButton("Complete Guru missions", 340, (_, _) => CompleteGuruMissions());
            guru.Location = new System.Drawing.Point(340, 54); guru.Size = new System.Drawing.Size(185, 32);
            Button maxScores = MakeQuestButton("Max score all", 540, (_, _) => MaxAllQuestScores());
            maxScores.Location = new System.Drawing.Point(540, 54);
            actions.Controls.AddRange(new Control[] { clear, cmbQuestRank, rank, nudQuestScore, score, revert, completeVisible, completeAll, guru, maxScores });
            actions.Controls.Add(new Label { Location = new System.Drawing.Point(710, 62), AutoSize = true, Text = "Sets every existing quest record to 999,999,999 score." });
            actions.Controls.Add(new Label { Location = new System.Drawing.Point(10, 108), AutoSize = true, Text = "Bulk actions only update existing verified quest records; identity and win-condition data are preserved." });
            questProgressPage.Controls.Add(lstQuestProgress);
            questProgressPage.Controls.Add(actions);
            questProgressPage.Controls.Add(filters);
            cmbQuestCharacter.SelectedIndexChanged += (_, _) => LoadQuestProgress();
            cmbQuestCategory.SelectedIndexChanged += (_, _) => RefreshQuestProgressList();
            cmbQuestView.SelectedIndexChanged += (_, _) => RefreshQuestProgressList();
            txtQuestSearch.TextChanged += (_, _) => RefreshQuestProgressList();
            lstQuestProgress.SelectedIndexChanged += (_, _) => { if (lstQuestProgress.SelectedItem is XV2QuestProgress q) nudQuestScore.Value = Math.Clamp(q.Score, 0, (int)nudQuestScore.Maximum); };
            questProgressPage.Enabled = false;
        }

        private Button MakeQuestButton(string text, int x, EventHandler handler)
        {
            Button button = new Button { Text = text, Location = new System.Drawing.Point(x, 10), Size = new System.Drawing.Size(155, 32) };
            button.Click += handler;
            return button;
        }

        private void LoadQuestProgress()
        {
            if (currentSave == null) { questProgress.Clear(); RefreshQuestProgressList(); return; }
            progressionSnapshot ??= (byte[])currentSave.DecryptedData.Clone();
            if (cmbQuestCharacter.Items.Count == 0)
            {
                foreach (XV2Character character in currentSave.Characters.Where(x => !x.IsEmpty)) cmbQuestCharacter.Items.Add(character);
                if (cmbQuestCharacter.Items.Count > 0) cmbQuestCharacter.SelectedIndex = 0;
            }
            if (cmbQuestCharacter.SelectedItem is not XV2Character selected) return;
            questProgressSnapshot ??= (byte[])currentSave.DecryptedData.Clone();
            questProgress.Clear();
            questProgress.AddRange(QuestProgressReader.Read(currentSave.DecryptedData, selected.Slot - 1));
            questProgressPage.Enabled = true;
            if (cmbProgressCharacter.Items.Count == 0 && currentSave != null)
            {
                foreach (XV2Character item in currentSave.Characters.Where(x => !x.IsEmpty)) cmbProgressCharacter.Items.Add(item);
                if (cmbProgressCharacter.Items.Count > 0) cmbProgressCharacter.SelectedIndex = 0;
            }
            RefreshQuestProgressList();
            if (cmbMentorCharacter.Items.Count == 0)
            {
                foreach (XV2Character item in currentSave!.Characters.Where(x => !x.IsEmpty)) cmbMentorCharacter.Items.Add(item);
                if (cmbMentorCharacter.Items.Count > 0) cmbMentorCharacter.SelectedIndex = 0;
            }
            if (cmbPartnerCharacter.Items.Count == 0)
            {
                foreach (XV2Character item in currentSave!.Characters.Where(x => !x.IsEmpty)) cmbPartnerCharacter.Items.Add(item);
                if (cmbPartnerCharacter.Items.Count > 0) cmbPartnerCharacter.SelectedIndex = 0;
            }
            RefreshCollections();
        }

        private IEnumerable<MentorGauge> FilteredMentors()
        {
            if (currentSave == null || cmbMentorCharacter.SelectedItem is not XV2Character character) return Enumerable.Empty<MentorGauge>();
            string query = txtMentorSearch.Text.Trim();
            return ProgressionUnlockAccess.ReadMentors(currentSave.DecryptedData, character.Slot - 1)
                .Where(x => query.Length == 0 || x.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        private void RefreshMentors()
        {
            if (lstMentorGauges == null) return;
            lstMentorGauges.Items.Clear();
            foreach (MentorGauge gauge in FilteredMentors()) lstMentorGauges.Items.Add(gauge);
            lblMentorCounts.Text = $"{lstMentorGauges.Items.Count} visible / {ProgressionUnlockAccess.MentorCount} mentors";
            if (lstMentorGauges.Items.Count > 0) lstMentorGauges.SelectedIndex = 0;
        }

        private void LoadSelectedMentorGauge()
        {
            if (lstMentorGauges.SelectedItem is not MentorGauge gauge) return;
            isLoadingProgression = true;
            try { nudMentorFriendship.Value = Math.Min(100, (int)gauge.Friendship); nudMentorDual.Value = Math.Min(100, (int)gauge.DualUltimate); }
            finally { isLoadingProgression = false; }
        }

        private void ApplySelectedMentor()
        {
            if (isLoadingProgression || currentSave == null || cmbMentorCharacter.SelectedItem is not XV2Character character || lstMentorGauges.SelectedItem is not MentorGauge gauge) return;
            gauge.Friendship = (ushort)nudMentorFriendship.Value; gauge.DualUltimate = (ushort)nudMentorDual.Value;
            ProgressionUnlockAccess.WriteMentor(currentSave.DecryptedData, character.Slot - 1, gauge); MarkUnsaved(); RefreshMentors();
        }

        private void MaxVisibleMentors()
        {
            if (currentSave == null || cmbMentorCharacter.SelectedItem is not XV2Character character) return;
            foreach (MentorGauge gauge in FilteredMentors().ToList())
            { gauge.Friendship = 100; gauge.DualUltimate = 100; ProgressionUnlockAccess.WriteMentor(currentSave.DecryptedData, character.Slot - 1, gauge); }
            MarkUnsaved(); RefreshMentors();
        }

        private bool ActiveCollectionIsArtwork() => cmbCollectionCategory.SelectedIndex == 0;
        private IEnumerable<CollectionUnlock> FilteredCollection()
        {
            if (currentSave == null) return Enumerable.Empty<CollectionUnlock>();
            string query = txtCollectionSearch.Text.Trim();
            return ProgressionUnlockAccess.ReadCollection(currentSave.DecryptedData, ActiveCollectionIsArtwork()).Where(x =>
                (cmbCollectionView.SelectedIndex == 0 || cmbCollectionView.SelectedIndex == 1 && x.Owned || cmbCollectionView.SelectedIndex == 2 && !x.Owned) &&
                (query.Length == 0 || x.Name.Contains(query, StringComparison.OrdinalIgnoreCase)));
        }

        private void RefreshCollections()
        {
            if (lstCollectionUnlocks == null) return;
            isLoadingProgression = true;
            try
            {
                lstCollectionUnlocks.Items.Clear();
                foreach (CollectionUnlock item in FilteredCollection()) lstCollectionUnlocks.Items.Add(item, item.Owned);
                if (currentSave == null) { lblCollectionCounts.Text = "No save loaded"; return; }
                List<CollectionUnlock> all = ProgressionUnlockAccess.ReadCollection(currentSave.DecryptedData, ActiveCollectionIsArtwork());
                lblCollectionCounts.Text = $"{lstCollectionUnlocks.Items.Count} visible | {all.Count(x => x.Owned)} owned | {all.Count(x => !x.Owned)} missing";
            }
            finally { isLoadingProgression = false; }
        }

        private void Collection_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            if (isLoadingProgression || currentSave == null || e.Index < 0 || lstCollectionUnlocks.Items[e.Index] is not CollectionUnlock item) return;
            BeginInvoke(() => { ProgressionUnlockAccess.WriteCollection(currentSave.DecryptedData, ActiveCollectionIsArtwork(), item, e.NewValue == CheckState.Checked); MarkUnsaved(); RefreshCollections(); });
        }

        private void UnlockSelectedCollection()
        {
            if (currentSave == null || lstCollectionUnlocks.SelectedItem is not CollectionUnlock item) return;
            ProgressionUnlockAccess.WriteCollection(currentSave.DecryptedData, ActiveCollectionIsArtwork(), item, true); MarkUnsaved(); RefreshCollections();
        }

        private void UnlockVisibleCollection()
        {
            if (currentSave == null) return;
            foreach (CollectionUnlock item in FilteredCollection().ToList()) ProgressionUnlockAccess.WriteCollection(currentSave.DecryptedData, ActiveCollectionIsArtwork(), item, true);
            MarkUnsaved(); RefreshCollections();
        }

        private void UnlockAllCollection()
        {
            if (currentSave == null) return;
            foreach (CollectionUnlock item in ProgressionUnlockAccess.ReadCollection(currentSave.DecryptedData, ActiveCollectionIsArtwork())) ProgressionUnlockAccess.WriteCollection(currentSave.DecryptedData, ActiveCollectionIsArtwork(), item, true);
            MarkUnsaved(); RefreshCollections();
        }

        private void UnlockPhotoMode()
        {
            if (currentSave == null) return;
            int added = InventoryWriter.AddMissingItems(currentSave.DecryptedData, InventoryReader.ImportantItemsOffset, 7, new[] { 43 }, 1);
            MarkUnsaved(); RefreshInventoryList();
            MessageBox.Show(added > 0 ? "Photo Mode Key added." : "Photo Mode Key is already present.", "Photo Mode", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RevertProgression()
        {
            if (currentSave == null || progressionSnapshot == null) return;
            for (int slot = 0; slot < CharacterReader.CharacterCount; slot++)
                Array.Copy(progressionSnapshot, ProgressionUnlockAccess.MentorOffset + slot * ProgressionUnlockAccess.MentorStride,
                    currentSave.DecryptedData, ProgressionUnlockAccess.MentorOffset + slot * ProgressionUnlockAccess.MentorStride,
                    ProgressionUnlockAccess.MentorCount * ProgressionUnlockAccess.MentorEntrySize);
            Array.Copy(progressionSnapshot, ProgressionUnlockAccess.MascotOffset, currentSave.DecryptedData, ProgressionUnlockAccess.MascotOffset, 8);
            Array.Copy(progressionSnapshot, ProgressionUnlockAccess.ArtworkOffset, currentSave.DecryptedData, ProgressionUnlockAccess.ArtworkOffset, 128);
            // Restore only the verified partner customization blocks.
            for (int slot = 0; slot < CharacterReader.CharacterCount; slot++)
            {
                Array.Copy(progressionSnapshot, 520096 + slot * 25392, currentSave.DecryptedData, 520096 + slot * 25392, 47 * 92);
                Array.Copy(progressionSnapshot, 524432 + slot * 25392, currentSave.DecryptedData, 524432 + slot * 25392, 10 * 44);
                Array.Copy(progressionSnapshot, 757220 + slot * 19588, currentSave.DecryptedData, 757220 + slot * 19588, 54 * 44);
            }
            MarkUnsaved(); RefreshMentors(); RefreshCollections(); RefreshPartnerStats();
        }

        private IEnumerable<PartnerStatEntry> FilteredPartnerStats()
        {
            if (currentSave == null || cmbPartnerCharacter.SelectedItem is not XV2Character character) return Enumerable.Empty<PartnerStatEntry>();
            string query = txtPartnerSearch.Text.Trim();
            return ProgressionUnlockAccess.ReadPartnerStats(currentSave.DecryptedData, character.Slot - 1)
                .Where(x => query.Length == 0 || x.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        private void RefreshPartnerStats()
        {
            if (lstPartnerStats == null) return;
            int selected = (lstPartnerStats.SelectedItem as PartnerStatEntry)?.Index ?? -1;
            lstPartnerStats.Items.Clear();
            foreach (PartnerStatEntry entry in FilteredPartnerStats()) lstPartnerStats.Items.Add(entry);
            List<PartnerStatEntry> all = ProgressionUnlockAccess.ReadPartnerStats(currentSave!.DecryptedData,
                (cmbPartnerCharacter.SelectedItem as XV2Character)?.Slot - 1 ?? 0);
            int ready = all.Count(x => x.Materialized);
            lblPartnerCounts.Text = $"{lstPartnerStats.Items.Count} visible | {ready} initialized / {all.Count} verified partners";
            PartnerStatEntry? restore = lstPartnerStats.Items.Cast<PartnerStatEntry>().FirstOrDefault(x => x.Index == selected);
            if (restore != null) lstPartnerStats.SelectedItem = restore; else if (lstPartnerStats.Items.Count > 0) lstPartnerStats.SelectedIndex = 0;
        }

        private void LoadSelectedPartnerStat()
        {
            if (lstPartnerStats.SelectedItem is not PartnerStatEntry entry) return;
            cmbPartnerStatType.Enabled = entry.Materialized;
            if (!entry.Materialized)
            {
                cmbPartnerStatType.SelectedIndex = -1;
                cmbPartnerStatType.Text = "Open this partner in-game first";
                return;
            }
            PartnerStatPreset? preset = ProgressionUnlockAccess.PartnerStatPresets.FirstOrDefault(x => x.ID == entry.StatType);
            cmbPartnerStatType.SelectedItem = preset;
            if (preset == null) cmbPartnerStatType.Text = $"Custom value {entry.StatType}";
        }

        private void ApplyPartnerStat()
        {
            if (currentSave == null || lstPartnerStats.SelectedItem is not PartnerStatEntry entry || cmbPartnerStatType.SelectedItem is not PartnerStatPreset preset) return;
            if (!entry.Materialized)
            {
                MessageBox.Show("Open this partner once in the in-game Partner Customisation menu so the game creates its record.", "Partner Not Initialized", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            ProgressionUnlockAccess.WritePartnerStat(currentSave.DecryptedData, entry, preset.ID); MarkUnsaved(); RefreshPartnerStats();
        }

        private void ApplyVisiblePartnerStats()
        {
            if (currentSave == null || cmbPartnerStatType.SelectedItem is not PartnerStatPreset preset) return;
            List<PartnerStatEntry> visible = FilteredPartnerStats().Where(x => x.Materialized).ToList();
            if (visible.Count == 0) return;
            if (MessageBox.Show($"Apply {preset.Name} to {visible.Count} visible custom partners?", "Apply Stat Type", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            foreach (PartnerStatEntry entry in visible) ProgressionUnlockAccess.WritePartnerStat(currentSave.DecryptedData, entry, preset.ID);
            MarkUnsaved(); RefreshPartnerStats();
        }

        private TabPage BuildTokipediaPage()
        {
            TabPage page = new TabPage("Tokipedia Routes");
            Panel top = new Panel { Dock = DockStyle.Top, Height = 55, Padding = new Padding(10) };
            cmbProgressCharacter = new ComboBox { Location = new System.Drawing.Point(10, 15), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            top.Controls.Add(cmbProgressCharacter);
            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 360 };
            lstTokipedia = new ListBox { Dock = DockStyle.Fill };
            lstTokipediaPaths = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true };
            Button complete = new Button { Dock = DockStyle.Bottom, Height = 38, Text = "Complete all referenced paths for selected entry" };
            split.Panel1.Controls.Add(lstTokipedia); split.Panel2.Controls.Add(lstTokipediaPaths); split.Panel2.Controls.Add(complete);
            page.Controls.Add(split); page.Controls.Add(top);
            cmbProgressCharacter.SelectedIndexChanged += (_, _) => LoadTokipediaProgress();
            lstTokipedia.SelectedIndexChanged += (_, _) => LoadTokipediaPaths();
            lstTokipediaPaths.ItemCheck += TokipediaPath_ItemCheck;
            complete.Click += (_, _) => CompleteTokipediaEntry();
            page.Enabled = false;
            return page;
        }

        private void LoadTokipediaProgress()
        {
            if (currentSave == null || cmbProgressCharacter.SelectedItem is not XV2Character character) return;
            tokipediaProgress = TokipediaProgressAccess.Read(currentSave.DecryptedData, character.Slot - 1, progressReferences.Tokipedia);
            lstTokipedia.Items.Clear();
            foreach (TokipediaProgressEntry entry in tokipediaProgress) lstTokipedia.Items.Add(entry);
            cmbProgressCharacter.Parent!.Parent!.Enabled = true;
            if (lstTokipedia.Items.Count > 0) lstTokipedia.SelectedIndex = 0;
            RefreshSystemFlags();
        }

        private void LoadTokipediaPaths()
        {
            isLoadingTokipediaPaths = true;
            try
            {
                lstTokipediaPaths.Items.Clear();
                if (lstTokipedia.SelectedItem is not TokipediaProgressEntry entry) return;
                foreach (string path in entry.BranchingPaths) lstTokipediaPaths.Items.Add("Branch: " + path, (entry.Flags & TokipediaFlagMap.Get(path)) != 0);
                foreach (string path in entry.AlternatePaths) lstTokipediaPaths.Items.Add("Alternate: " + path, (entry.Flags & TokipediaFlagMap.Get(path)) != 0);
            }
            finally { isLoadingTokipediaPaths = false; }
        }

        private void TokipediaPath_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            if (isLoadingTokipediaPaths || currentSave == null ||
                e.Index < 0 || e.Index >= lstTokipediaPaths.Items.Count ||
                lstTokipedia.SelectedItem is not TokipediaProgressEntry entry) return;
            string text = lstTokipediaPaths.Items[e.Index].ToString()!;
            string name = text[(text.IndexOf(':') + 1)..].Trim();
            ulong flag = TokipediaFlagMap.Get(name);
            if (e.NewValue == CheckState.Checked) entry.Flags |= flag; else entry.Flags &= ~flag;
            TokipediaProgressAccess.Write(currentSave.DecryptedData, entry); MarkUnsaved();
            BeginInvoke(() =>
            {
                if (IsDisposed || lstTokipedia.IsDisposed) return;
                int index = lstTokipedia.SelectedIndex;
                if (index < 0 || index >= lstTokipedia.Items.Count) return;
                lstTokipedia.Items[index] = entry;
                lstTokipedia.SelectedIndex = index;
            });
        }

        private void CompleteTokipediaEntry()
        {
            if (currentSave == null || lstTokipedia.SelectedItem is not TokipediaProgressEntry entry) return;
            foreach (string path in entry.BranchingPaths.Concat(entry.AlternatePaths)) entry.Flags |= TokipediaFlagMap.Get(path);
            TokipediaProgressAccess.Write(currentSave.DecryptedData, entry); MarkUnsaved(); LoadTokipediaPaths();
            int index = lstTokipedia.SelectedIndex; lstTokipedia.Items[index] = entry; lstTokipedia.SelectedIndex = index;
        }

        private TabPage BuildSystemFlagsPage()
        {
            TabPage page = new TabPage("System Flags (read-only)");
            txtSystemFlagSearch = new TextBox { Dock = DockStyle.Top, PlaceholderText = "Search flag index, name, type, or quest condition" };
            lstSystemFlags = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true };
            lstSystemFlags.Columns.Add("Set", 45); lstSystemFlags.Columns.Add("Index", 65); lstSystemFlags.Columns.Add("Name", 330); lstSystemFlags.Columns.Add("Type", 90); lstSystemFlags.Columns.Add("Conditions", 500);
            page.Controls.Add(lstSystemFlags); page.Controls.Add(txtSystemFlagSearch);
            txtSystemFlagSearch.TextChanged += (_, _) => RefreshSystemFlags();
            return page;
        }

        private void RefreshSystemFlags()
        {
            if (lstSystemFlags == null) return;
            lstSystemFlags.Items.Clear();
            if (currentSave == null || cmbProgressCharacter.SelectedItem is not XV2Character character) return;
            string query = txtSystemFlagSearch.Text.Trim();
            foreach (SystemFlagDefinition flag in progressReferences.SystemFlags)
            {
                string conditions = string.Join(", ", flag.Conditions1.Concat(flag.Conditions2));
                string search = $"{flag.Index} {flag.Name} {flag.Type} {conditions}";
                if (query.Length > 0 && !search.Contains(query, StringComparison.OrdinalIgnoreCase)) continue;
                bool set = SystemFlagAccess.Read(currentSave.DecryptedData, character.Slot - 1, flag.Index);
                ListViewItem row = new ListViewItem(set ? "Yes" : "No");
                row.SubItems.Add(flag.Index.ToString()); row.SubItems.Add(flag.Name); row.SubItems.Add(flag.Type); row.SubItems.Add(conditions);
                lstSystemFlags.Items.Add(row);
            }
        }

        private IEnumerable<XV2QuestProgress> FilteredQuests()
        {
            string query = txtQuestSearch.Text.Trim();
            return questProgress.Where(q =>
                (cmbQuestCategory.SelectedIndex == 0 || (int)q.Category == cmbQuestCategory.SelectedIndex - 1) &&
                (cmbQuestView.SelectedIndex == 0 || cmbQuestView.SelectedIndex == 1 && q.IsCleared || cmbQuestView.SelectedIndex == 2 && !q.IsCleared || cmbQuestView.SelectedIndex == 3 && q.State == 0) &&
                (query.Length == 0 || q.ID.ToString().Contains(query) || q.Category.ToString().Contains(query, StringComparison.OrdinalIgnoreCase)));
        }

        private void RefreshQuestProgressList()
        {
            if (lstQuestProgress == null) return;
            int selectedOffset = (lstQuestProgress.SelectedItem as XV2QuestProgress)?.RecordOffset ?? -1;
            lstQuestProgress.Items.Clear();
            foreach (XV2QuestProgress quest in FilteredQuests()) lstQuestProgress.Items.Add(quest);
            lblQuestCounts.Text = $"{lstQuestProgress.Items.Count} visible | {questProgress.Count(q => q.IsCleared)} / {questProgress.Count} cleared";
            XV2QuestProgress? restore = questProgress.FirstOrDefault(q => q.RecordOffset == selectedOffset);
            if (restore != null && lstQuestProgress.Items.Contains(restore)) lstQuestProgress.SelectedItem = restore;
            else if (lstQuestProgress.Items.Count > 0) lstQuestProgress.SelectedIndex = 0;
        }

        private void EditQuestSelection(Action<XV2QuestProgress> edit)
        {
            if (currentSave == null || lstQuestProgress.SelectedItem is not XV2QuestProgress quest) return;
            edit(quest); QuestProgressWriter.Write(currentSave.DecryptedData, quest); MarkUnsaved(); RefreshQuestProgressList();
        }

        private void CompleteVisibleQuests(int rank)
        {
            if (currentSave == null) return;
            foreach (XV2QuestProgress quest in FilteredQuests().ToList()) { quest.State = 3; quest.Rank = rank; QuestProgressWriter.Write(currentSave.DecryptedData, quest); }
            MarkUnsaved(); RefreshQuestProgressList();
        }

        private void CompleteAllQuests(int rank)
        {
            if (currentSave == null || questProgress.Count == 0) return;
            if (MessageBox.Show($"Clear all {questProgress.Count} existing quest records for this CaC with {cmbQuestRank.Text} rank?",
                "Clear All Quests", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            foreach (XV2QuestProgress quest in questProgress)
            {
                quest.State = 3; quest.Rank = rank; QuestProgressWriter.Write(currentSave.DecryptedData, quest);
            }
            MarkUnsaved(); RefreshQuestProgressList();
        }

        private void MaxAllQuestScores()
        {
            if (currentSave == null || questProgress.Count == 0) return;
            int maximum = (int)nudQuestScore.Maximum;
            if (MessageBox.Show($"Set the score of all {questProgress.Count} existing quest records for this CaC to {maximum:N0}?\n\nClear state and rank will not be changed.",
                "Max All Quest Scores", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            foreach (XV2QuestProgress quest in questProgress)
            {
                quest.Score = maximum;
                QuestProgressWriter.Write(currentSave.DecryptedData, quest);
            }
            MarkUnsaved();
            RefreshQuestProgressList();
        }

        private void CompleteGuruMissions()
        {
            if (currentSave == null) return;
            List<XV2QuestProgress> guruQuests = questProgress.Where(q => q.Category == XV2QuestCategory.TimeRift).ToList();
            if (guruQuests.Count == 0)
            {
                MessageBox.Show("No existing Time Rift mission records were found for this CaC.", "Complete Guru Missions", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show($"Mark all {guruQuests.Count} existing Time Rift mission records cleared with Z rank?\n\nThe undocumented GuruProgress byte will not be changed.",
                "Complete Guru Missions", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            foreach (XV2QuestProgress quest in guruQuests)
            {
                quest.State = 3; quest.Rank = 6; QuestProgressWriter.Write(currentSave.DecryptedData, quest);
            }
            MarkUnsaved(); RefreshQuestProgressList();
        }

        private void RevertQuestProgress()
        {
            if (currentSave == null || questProgressSnapshot == null || cmbQuestCharacter.SelectedItem is not XV2Character character) return;
            foreach (XV2QuestProgress quest in QuestProgressReader.Read(questProgressSnapshot, character.Slot - 1)) Array.Copy(questProgressSnapshot, quest.RecordOffset, currentSave.DecryptedData, quest.RecordOffset, 24);
            MarkUnsaved(); LoadQuestProgress();
        }

        private Button MakeSkillButton(string text, int x, EventHandler handler)
        {
            Button button = new Button { Text = text, Location = new System.Drawing.Point(x, 10), Size = new System.Drawing.Size(140, 32) };
            button.Click += handler;
            return button;
        }

        private void LoadSkillOwnership()
        {
            skillOwnership.Clear();
            if (currentSave == null) { skillOwnershipSnapshot = null; RefreshSkillOwnershipList(); return; }
            skillOwnershipSnapshot = (byte[])currentSave.DecryptedData.Clone();
            foreach (NamedValueKind category in new[] { NamedValueKind.SuperSkill, NamedValueKind.UltimateSkill, NamedValueKind.EvasiveSkill, NamedValueKind.AwokenSkill })
                skillOwnership.AddRange(SkillOwnershipReader.Read(currentSave.DecryptedData, category, nameDatabase));
            RefreshSkillOwnershipList();
            SetSkillButtons(true);
        }

        private IEnumerable<XV2SkillOwnership> FilteredSkills()
        {
            string query = txtSkillSearch.Text.Trim();
            NamedValueKind activeCategory = ActiveSkillCategory();
            return skillOwnership.Where(skill =>
                skill.Category == activeCategory &&
                (cmbSkillView.SelectedIndex == 0 || (cmbSkillView.SelectedIndex == 1 ? skill.Owned : !skill.Owned)) &&
                (query.Length == 0 || skill.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || skill.ID1.ToString().Contains(query) || skill.ID2.ToString().Contains(query)));
        }

        private NamedValueKind ActiveSkillCategory() => tabSkillCategories.SelectedIndex switch
        {
            1 => NamedValueKind.UltimateSkill, 2 => NamedValueKind.EvasiveSkill,
            3 => NamedValueKind.AwokenSkill, _ => NamedValueKind.SuperSkill
        };

        private CheckedListBox ActiveSkillList() => skillCategoryLists[ActiveSkillCategory()];

        private void RefreshSkillOwnershipList()
        {
            if (tabSkillCategories == null) return;
            lstSkillOwnership = ActiveSkillList();
            isLoadingSkills = true;
            try
            {
                lstSkillOwnership.Items.Clear();
                foreach (XV2SkillOwnership skill in FilteredSkills()) lstSkillOwnership.Items.Add(skill, skill.Owned);
                NamedValueKind category = ActiveSkillCategory();
                int total = skillOwnership.Count(x => x.Category == category);
                int owned = skillOwnership.Count(x => x.Category == category && x.Owned);
                lblSkillCounts.Text = $"{lstSkillOwnership.Items.Count} visible | {owned} owned | {total - owned} missing";
            }
            finally { isLoadingSkills = false; }
        }

        private void SkillOwnership_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            if (isLoadingSkills || currentSave == null || sender is not CheckedListBox list || list.Items[e.Index] is not XV2SkillOwnership skill) return;
            BeginInvoke(() => SetSkillOwned(skill, e.NewValue == CheckState.Checked, refresh: true));
        }

        private void SetSkillOwned(XV2SkillOwnership skill, bool owned, bool refresh)
        {
            if (currentSave == null || skill.Owned == owned) return;
            skill.Owned = owned;
            SkillOwnershipWriter.WriteOwned(currentSave.DecryptedData, skill);
            MarkUnsaved();
            if (refresh) RefreshSkillOwnershipList();
        }

        private void UnlockSelectedSkill()
        {
            if (ActiveSkillList().SelectedItem is XV2SkillOwnership skill) SetSkillOwned(skill, true, true);
        }

        private void UnlockVisibleSkills()
        {
            foreach (XV2SkillOwnership skill in FilteredSkills().ToList()) SetSkillOwned(skill, true, false);
            RefreshSkillOwnershipList();
        }

        private void UnlockAllVerifiedSkills()
        {
            foreach (XV2SkillOwnership skill in skillOwnership) SetSkillOwned(skill, true, false);
            RefreshSkillOwnershipList();
        }

        private void RevertSkillOwnership()
        {
            if (currentSave == null || skillOwnershipSnapshot == null) return;
            foreach (NamedValueKind category in new[] { NamedValueKind.SuperSkill, NamedValueKind.UltimateSkill, NamedValueKind.EvasiveSkill, NamedValueKind.AwokenSkill })
            {
                int offset = SkillOwnershipReader.GetOffset(category);
                Array.Copy(skillOwnershipSnapshot, offset, currentSave.DecryptedData, offset, SaveOffsets.SkillRecordCount * SaveOffsets.SkillRecordSize);
            }
            skillOwnership.Clear();
            foreach (NamedValueKind category in new[] { NamedValueKind.SuperSkill, NamedValueKind.UltimateSkill, NamedValueKind.EvasiveSkill, NamedValueKind.AwokenSkill })
                skillOwnership.AddRange(SkillOwnershipReader.Read(currentSave.DecryptedData, category, nameDatabase));
            MarkUnsaved();
            RefreshSkillOwnershipList();
        }

        private void SetSkillButtons(bool enabled)
        {
            if (btnUnlockSelected == null) return;
            btnUnlockSelected.Enabled = enabled;
            btnUnlockVisible.Enabled = enabled;
            btnUnlockAllSkills.Enabled = enabled;
            btnRevertSkills.Enabled = enabled;
            lstSkillOwnership.Enabled = enabled;
        }

        private int CountChangedSkillOwnership()
        {
            if (currentSave == null || skillOwnershipSnapshot == null) return 0;
            int count = 0;
            foreach (NamedValueKind category in new[] { NamedValueKind.SuperSkill, NamedValueKind.UltimateSkill, NamedValueKind.EvasiveSkill, NamedValueKind.AwokenSkill })
            {
                int offset = SkillOwnershipReader.GetOffset(category);
                for (int slot = 0; slot < SaveOffsets.SkillRecordCount; slot++)
                {
                    int record = offset + slot * SaveOffsets.SkillRecordSize;
                    if (currentSave.DecryptedData[record] != skillOwnershipSnapshot[record] || currentSave.DecryptedData[record + 1] != skillOwnershipSnapshot[record + 1]) count++;
                }
            }
            return count;
        }

        private int CountChangedQuestProgress()
        {
            if (currentSave == null || questProgressSnapshot == null) return 0;
            int count = 0;
            for (int slot = 0; slot < 8; slot++)
            {
                foreach (XV2QuestProgress quest in QuestProgressReader.Read(currentSave.DecryptedData, slot))
                {
                    bool changed = false;
                    for (int i = 8; i < 24; i++)
                        if (currentSave.DecryptedData[quest.RecordOffset + i] != questProgressSnapshot[quest.RecordOffset + i]) { changed = true; break; }
                    if (changed) count++;
                }
            }
            return count;
        }

        private int CountChangedProgressionBytes()
        {
            if (currentSave == null || progressionSnapshot == null) return 0;
            int changed = 0;
            void CountRange(int offset, int length)
            {
                for (int i = 0; i < length; i++) if (currentSave.DecryptedData[offset + i] != progressionSnapshot[offset + i]) changed++;
            }
            CountRange(ProgressionUnlockAccess.MascotOffset, 8);
            CountRange(ProgressionUnlockAccess.ArtworkOffset, 128);
            for (int slot = 0; slot < CharacterReader.CharacterCount; slot++)
                CountRange(ProgressionUnlockAccess.MentorOffset + slot * ProgressionUnlockAccess.MentorStride,
                    ProgressionUnlockAccess.MentorCount * ProgressionUnlockAccess.MentorEntrySize);
            for (int slot = 0; slot < CharacterReader.CharacterCount; slot++)
            {
                CountRange(520096 + slot * 25392, 47 * 92);
                CountRange(524432 + slot * 25392, 10 * 44);
                CountRange(757220 + slot * 19588, 54 * 44);
            }
            return changed;
        }

        private void ConfigureQQBangEditor()
        {
            TabControl inventoryTabs = new TabControl { Dock = DockStyle.Fill };
            TabPage itemsPage = new TabPage("Items");
            TabPage qqPage = new TabPage("QQ Bangs");

            tabInventory.Controls.Remove(grpInventoryBrowser);
            tabInventory.Controls.Remove(grpInventoryDetails);
            itemsPage.Controls.Add(grpInventoryBrowser);
            itemsPage.Controls.Add(grpInventoryDetails);
            inventoryTabs.TabPages.Add(itemsPage);
            inventoryTabs.TabPages.Add(qqPage);
            tabInventory.Controls.Add(inventoryTabs);

            grpQQBangEditor = new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = "Owned QQ Bangs — verified fields only",
                Padding = new Padding(15)
            };
            qqPage.Controls.Add(grpQQBangEditor);

            Panel left = new Panel { Dock = DockStyle.Left, Width = 560 };
            Panel right = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18, 8, 8, 8) };
            grpQQBangEditor.Controls.Add(right);
            grpQQBangEditor.Controls.Add(left);

            Label searchLabel = new Label { Text = "Search stats, slot, quantity, or metadata:", AutoSize = true, Location = new System.Drawing.Point(5, 8) };
            txtQQBangSearch = new TextBox { Location = new System.Drawing.Point(5, 30), Width = 435 };
            Button clear = new Button { Text = "Clear", Location = new System.Drawing.Point(448, 28), Width = 90 };
            lblQQBangCount = new Label { Text = "QQ Bangs: 0", AutoSize = true, Location = new System.Drawing.Point(5, 62) };
            lstQQBangs = new ListBox { Location = new System.Drawing.Point(5, 86), Size = new System.Drawing.Size(535, 385), HorizontalScrollbar = true };
            left.Controls.AddRange(new Control[] { searchLabel, txtQQBangSearch, clear, lblQQBangCount, lstQQBangs });

            txtQQBangSearch.TextChanged += (_, _) => RefreshQQBangList();
            clear.Click += (_, _) => txtQQBangSearch.Clear();
            lstQQBangs.SelectedIndexChanged += (_, _) => LoadSelectedQQBang();

            string[] names = { "Health", "Ki", "Stamina", "Basic Attack", "Strike Supers", "Ki Blast Supers" };
            nudQQBangStats = new NumericUpDown[6];
            for (int i = 0; i < names.Length; i++)
            {
                Label label = new Label { Text = names[i] + ":", AutoSize = true, Location = new System.Drawing.Point(8, 12 + i * 43) };
                NumericUpDown field = new NumericUpDown
                {
                    Minimum = -5,
                    Maximum = 5,
                    Location = new System.Drawing.Point(155, 9 + i * 43),
                    Width = 85,
                    TextAlign = HorizontalAlignment.Center
                };
                nudQQBangStats[i] = field;
                right.Controls.Add(label);
                right.Controls.Add(field);
            }

            right.Controls.Add(new Label { Text = "Quantity:", AutoSize = true, Location = new System.Drawing.Point(8, 270) });
            nudQQBangQuantity = new NumericUpDown { Minimum = 1, Maximum = 99, Location = new System.Drawing.Point(155, 267), Width = 85 };
            right.Controls.Add(nudQQBangQuantity);
            lblQQBangMetadata = new Label { AutoSize = true, Location = new System.Drawing.Point(8, 308), Text = "Metadata: —" };
            right.Controls.Add(lblQQBangMetadata);

            btnQQBangApply = MakeQQButton("Apply verified fields", 8, 345, 232, (_, _) => ApplySelectedQQBang());
            Button add = MakeQQButton("Add new", 265, 345, 145, (_, _) => AddQQBang());
            btnQQBangCopy = MakeQQButton("Copy", 8, 385, 110, (_, _) => CopySelectedQQBang());
            btnQQBangPaste = MakeQQButton("Paste stats", 130, 385, 110, (_, _) => PasteSelectedQQBang());
            btnQQBangRevert = MakeQQButton("Revert selected", 8, 425, 232, (_, _) => RevertSelectedQQBang());
            Button balanced = MakeQQButton("Preset: all 0", 265, 9, 145, (_, _) => SetQQBangPreset(0));
            Button perfect = MakeQQButton("Preset: all +5", 265, 49, 145, (_, _) => SetQQBangPreset(5));
            right.Controls.AddRange(new Control[] { btnQQBangApply, add, btnQQBangCopy, btnQQBangPaste, btnQQBangRevert, balanced, perfect });

            Label safety = new Label
            {
                AutoSize = false,
                Location = new System.Drawing.Point(265, 105),
                Size = new System.Drawing.Size(210, 170),
                Text = "Signed stats are shown as -5 through +5. Copy/paste transfers only the six verified stats. The metadata byte is displayed read-only and is never changed."
            };
            right.Controls.Add(safety);
            grpQQBangEditor.Enabled = false;
            UpdateQQBangButtons();
        }

        private Button MakeQQButton(string text, int x, int y, int width, EventHandler handler)
        {
            Button button = new Button { Text = text, Location = new System.Drawing.Point(x, y), Size = new System.Drawing.Size(width, 31) };
            button.Click += handler;
            return button;
        }

        private void RefreshQQBangList()
        {
            if (lstQQBangs == null) return;
            int selectedSlot = (lstQQBangs.SelectedItem as XV2QQBang)?.SlotIndex ?? -1;
            lstQQBangs.BeginUpdate();
            try
            {
                lstQQBangs.Items.Clear();
                currentQQBangs.Clear();
                if (currentSave == null)
                {
                    lblQQBangCount.Text = "QQ Bangs: 0";
                    UpdateQQBangButtons();
                    return;
                }

                string query = txtQQBangSearch.Text.Trim();
                foreach (XV2QQBang item in currentSave.QQBangs)
                {
                    string searchable = $"{item.DisplayName} metadata {item.Metadata} 0x{item.Metadata:X2} total {item.Total}";
                    if (query.Length > 0 && searchable.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0) continue;
                    currentQQBangs.Add(item);
                    lstQQBangs.Items.Add(item);
                }

                int total = currentSave.QQBangs.Count;
                lblQQBangCount.Text = $"QQ Bangs: {currentQQBangs.Count} visible / {total} owned";
                XV2QQBang? restore = currentQQBangs.FirstOrDefault(x => x.SlotIndex == selectedSlot);
                if (restore != null) lstQQBangs.SelectedItem = restore;
                else if (lstQQBangs.Items.Count > 0) lstQQBangs.SelectedIndex = 0;
            }
            finally { lstQQBangs.EndUpdate(); }
            UpdateQQBangButtons();
        }

        private void LoadSelectedQQBang()
        {
            if (lstQQBangs.SelectedItem is not XV2QQBang item)
            {
                UpdateQQBangButtons();
                return;
            }
            isLoadingQQBangControls = true;
            try
            {
                sbyte[] values = { item.Health, item.Ki, item.Stamina, item.BasicAttack, item.StrikeSupers, item.KiBlastSupers };
                for (int i = 0; i < values.Length; i++) nudQQBangStats[i].Value = values[i];
                nudQQBangQuantity.Value = Math.Clamp(item.Quantity, (byte)1, (byte)99);
                lblQQBangMetadata.Text = $"Slot: {item.SlotIndex}   Metadata: {item.Metadata} (0x{item.Metadata:X2})   Total: {item.Total:+#;-#;0}";
            }
            finally { isLoadingQQBangControls = false; }
            UpdateQQBangButtons();
        }

        private void ApplySelectedQQBang()
        {
            if (isLoadingQQBangControls || currentSave == null || lstQQBangs.SelectedItem is not XV2QQBang item) return;
            item.Health = (sbyte)nudQQBangStats[0].Value;
            item.Ki = (sbyte)nudQQBangStats[1].Value;
            item.Stamina = (sbyte)nudQQBangStats[2].Value;
            item.BasicAttack = (sbyte)nudQQBangStats[3].Value;
            item.StrikeSupers = (sbyte)nudQQBangStats[4].Value;
            item.KiBlastSupers = (sbyte)nudQQBangStats[5].Value;
            item.Quantity = (byte)nudQQBangQuantity.Value;
            QQBangWriter.WriteVerifiedFields(currentSave.DecryptedData, item);
            MarkUnsaved();
            RefreshQQBangList();
        }

        private void AddQQBang()
        {
            if (currentSave == null) return;
            XV2QQBang added = QQBangWriter.AddNeutral(currentSave.DecryptedData);
            MarkUnsaved();
            txtQQBangSearch.Clear();
            RefreshQQBangList();
            XV2QQBang? displayed = currentQQBangs.FirstOrDefault(x => x.SlotIndex == added.SlotIndex);
            if (displayed != null) lstQQBangs.SelectedItem = displayed;
        }

        private void CopySelectedQQBang()
        {
            if (lstQQBangs.SelectedItem is not XV2QQBang item) return;
            copiedQQBang = item.Clone();
            UpdateQQBangButtons();
        }

        private void PasteSelectedQQBang()
        {
            if (copiedQQBang == null) return;
            nudQQBangStats[0].Value = copiedQQBang.Health;
            nudQQBangStats[1].Value = copiedQQBang.Ki;
            nudQQBangStats[2].Value = copiedQQBang.Stamina;
            nudQQBangStats[3].Value = copiedQQBang.BasicAttack;
            nudQQBangStats[4].Value = copiedQQBang.StrikeSupers;
            nudQQBangStats[5].Value = copiedQQBang.KiBlastSupers;
        }

        private void RevertSelectedQQBang()
        {
            if (currentSave == null || lstQQBangs.SelectedItem is not XV2QQBang item || !originalQQBangs.TryGetValue(item.SlotIndex, out XV2QQBang? original)) return;
            QQBangWriter.RestoreOriginal(currentSave.DecryptedData, original);
            MarkUnsaved();
            RefreshQQBangList();
        }

        private void SetQQBangPreset(int value)
        {
            foreach (NumericUpDown field in nudQQBangStats) field.Value = value;
        }

        private void SnapshotQQBangs()
        {
            originalQQBangs.Clear();
            if (currentSave == null) return;
            foreach (XV2QQBang item in currentSave.QQBangs) originalQQBangs[item.SlotIndex] = item.Clone();
            copiedQQBang = null;
            RefreshQQBangList();
        }

        private void UpdateQQBangButtons()
        {
            if (btnQQBangApply == null) return;
            bool selected = lstQQBangs.SelectedItem is XV2QQBang;
            btnQQBangApply.Enabled = selected;
            btnQQBangCopy.Enabled = selected;
            btnQQBangRevert.Enabled = selected;
            btnQQBangPaste.Enabled = selected && copiedQQBang != null;
        }


        // =========================================================
        // APPEARANCE HELPERS
        // =========================================================

        private void LoadAppearance(
            XV2Appearance appearance)
        {
            SetNumericValue(
                nudBodyShape,
                appearance.BodyShape
            );


            SetNumericValue(
                nudFaceBase,
                appearance.FaceBase
            );


            SetNumericValue(
                nudForehead,
                appearance.FaceForehead
            );


            SetNumericValue(
                nudEyes,
                appearance.Eyes
            );


            SetNumericValue(
                nudNose,
                appearance.Nose
            );


            SetNumericValue(
                nudEars,
                appearance.Ears
            );


            SetNumericValue(
                nudHair,
                appearance.Hair
            );


            SetNumericValue(
                nudSkinColor1,
                appearance.SkinColor1
            );


            SetNumericValue(
                nudSkinColor2,
                appearance.SkinColor2
            );


            SetNumericValue(
                nudSkinColor3,
                appearance.SkinColor3
            );


            SetNumericValue(
                nudSkinColor4,
                appearance.SkinColor4
            );


            SetNumericValue(
                nudHairColor,
                appearance.HairColor
            );


            SetNumericValue(
                nudEyeColor,
                appearance.EyeColor
            );


            SetNumericValue(
                nudMakeupColor1,
                appearance.MakeupColor1
            );


            SetNumericValue(
                nudMakeupColor2,
                appearance.MakeupColor2
            );


            SetNumericValue(
                nudMakeupColor3,
                appearance.MakeupColor3
            );
        }


        private XV2Appearance ReadAppearanceFromControls()
        {
            return new XV2Appearance
            {
                BodyShape =
                    (int)nudBodyShape.Value,

                FaceBase =
                    (int)nudFaceBase.Value,

                FaceForehead =
                    (int)nudForehead.Value,

                Eyes =
                    (int)nudEyes.Value,

                Nose =
                    (int)nudNose.Value,

                Ears =
                    (int)nudEars.Value,

                Hair =
                    (int)nudHair.Value,

                SkinColor1 =
                    (ushort)nudSkinColor1.Value,

                SkinColor2 =
                    (ushort)nudSkinColor2.Value,

                SkinColor3 =
                    (ushort)nudSkinColor3.Value,

                SkinColor4 =
                    (ushort)nudSkinColor4.Value,

                HairColor =
                    (ushort)nudHairColor.Value,

                EyeColor =
                    (ushort)nudEyeColor.Value,

                MakeupColor1 =
                    (ushort)nudMakeupColor1.Value,

                MakeupColor2 =
                    (ushort)nudMakeupColor2.Value,

                MakeupColor3 =
                    (ushort)nudMakeupColor3.Value
            };
        }


        // =========================================================
        // NUMERIC HELPER
        // =========================================================

        private void SetNumericValue(
            NumericUpDown control,
            decimal value)
        {
            if (
                value <
                control.Minimum)
            {
                control.Value =
                    control.Minimum;
            }


            else if (
                value >
                control.Maximum)
            {
                control.Value =
                    control.Maximum;
            }


            else
            {
                control.Value =
                    value;
            }
        }


        // =========================================================
        // CLEAR CHARACTER
        // =========================================================

        private void ClearCharacterControls()
        {
            txtCharacterName.Text =
                "";


            cmbRace.SelectedIndex =
                -1;


            nudCharacterLevel.Value =
                1;


            nudCharacterXP.Value =
                0;


            nudAttributePoints.Value =
                0;


            nudHealth.Value =
                0;


            nudKi.Value =
                0;


            nudStamina.Value =
                0;


            nudBasicAttack.Value =
                0;


            nudStrikeSupers.Value =
                0;


            nudKiBlastSupers.Value =
                0;
        }


        // =========================================================
        // CLEAR APPEARANCE
        // =========================================================

        private void ClearAppearanceControls()
        {
            nudBodyShape.Value =
                0;


            nudFaceBase.Value =
                0;


            nudForehead.Value =
                0;


            nudEyes.Value =
                0;


            nudNose.Value =
                0;


            nudEars.Value =
                0;


            nudHair.Value =
                0;


            nudSkinColor1.Value =
                0;


            nudSkinColor2.Value =
                0;


            nudSkinColor3.Value =
                0;


            nudSkinColor4.Value =
                0;


            nudHairColor.Value =
                0;


            nudEyeColor.Value =
                0;


            nudMakeupColor1.Value =
                0;


            nudMakeupColor2.Value =
                0;


            nudMakeupColor3.Value =
                0;
        }

        private void ConfigureCharacterTransferButtons()
        {
            btnExportCac = new Button { Text = "Export CaC", Size = new System.Drawing.Size(110, 34) };
            btnImportCac = new Button { Text = "Import CaC", Size = new System.Drawing.Size(110, 34) };
            btnEmptyAllPresets = new Button { Text = "Empty All Presets", Size = new System.Drawing.Size(150, 34) };
            btnExportCac.Click += ExportSelectedCac;
            btnImportCac.Click += ImportSelectedCac;
            btnEmptyAllPresets.Click += EmptyAllPresetsForSelectedCac;
            grpPresetTools.Controls.AddRange(new Control[] { btnExportCac, btnImportCac, btnEmptyAllPresets });

            Button[] buttons = { btnCopyPreset, btnPastePreset, btnCopyOutfit, btnPasteOutfit, btnCopySkills, btnPasteSkills, btnResetPreset, btnExportCac, btnImportCac, btnEmptyAllPresets };
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Location = new System.Drawing.Point(14 + (i % 5) * 205, 27 + (i / 5) * 39);
                buttons[i].Size = new System.Drawing.Size(194, 34);
                ModernTheme.StyleButton(buttons[i], buttons[i] == btnEmptyAllPresets);
            }
            btnExportCac.Enabled = btnImportCac.Enabled = btnEmptyAllPresets.Enabled = currentSave != null;
        }

        private void EmptyAllPresetsForSelectedCac(object? sender, EventArgs e)
        {
            if (currentSave == null || cmbPresetCharacters.SelectedItem is not XV2Character character) return;
            if (character.Presets.Count != PresetWriter.PresetCount)
            {
                MessageBox.Show("This CaC does not contain the expected eight-preset structure.", "Empty All Presets", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show(
                    $"Empty all eight presets for {character.Name} (CaC slot {character.Slot})?\n\n" +
                    "This removes all equipped clothing, accessory, Super Soul, QQ Bang and skills from every preset, including Main. Colour values are preserved.\n\n" +
                    "The change remains unsaved until you save the file.",
                    "Empty All Presets",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) != DialogResult.Yes) return;

            foreach (XV2Preset preset in character.Presets)
                PresetWriter.ApplyVerifiedEmptyLoadout(preset);
            PresetWriter.WritePresets(currentSave.DecryptedData, character.Slot - 1, character.Presets);
            LoadSelectedPreset();
            MarkUnsaved();
        }

        private void ExportSelectedCac(object? sender, EventArgs e)
        {
            if (currentSave == null || cmbCharacters.SelectedItem is not XV2Character character) return;
            using SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Export CaC",
                Filter = "Exported CaC (*.excac)|*.excac",
                AddExtension = true,
                DefaultExt = "excac",
                FileName = string.IsNullOrWhiteSpace(character.Name) ? $"CaC_Slot_{character.Slot}.excac" : character.Name + ".excac"
            };
            if (dialog.ShowDialog() != DialogResult.OK) return;
            try
            {
                // Commit the visible CaC fields to the in-memory block first.
                StoreCurrentCharacterControls();
                LevelCapFlagValidator.Apply(currentSave.DecryptedData);
                ExcacFile.FromSave(currentSave.DecryptedData, character.Slot - 1,
                    currentSave.Characters[character.Slot - 1]).Save(dialog.FileName);
                MessageBox.Show("CaC exported successfully.", "Export CaC", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not export the CaC:\n\n{ex.Message}", "Export CaC - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ImportSelectedCac(object? sender, EventArgs e)
        {
            if (currentSave == null || cmbCharacters.SelectedItem is not XV2Character target) return;
            using OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Import CaC",
                Filter = "Exported CaC (*.excac)|*.excac",
                DefaultExt = "excac",
                CheckFileExists = true,
                Multiselect = false
            };
            if (dialog.ShowDialog() != DialogResult.OK) return;
            try
            {
                if (!string.Equals(Path.GetExtension(dialog.FileName), ".excac", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Only .excac character files can be imported.");
                ExcacFile imported = ExcacFile.Load(dialog.FileName);
                if (MessageBox.Show($"Replace slot {target.Slot} ({target.Name}) with {imported.Name}, level {imported.Level}?\n\nThe change remains unsaved until you create an edited save.",
                    "Import CaC", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK) return;
                imported.ImportInto(currentSave.DecryptedData, target.Slot - 1);
                RefreshLoadedSave(target.Slot - 1, preserveDirty: true);
                MarkUnsaved();
                MessageBox.Show("CaC imported. Review it, then save when ready.", "Import CaC", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not import the CaC:\n\n{ex.Message}", "Import CaC - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StoreCurrentCharacterControls()
        {
            if (currentSave == null || loadedCharacter == null || cmbRace.SelectedIndex < 0) return;
            StoreCharacterControls(loadedCharacter, loadedCharacterSlotIndex);
            StorePresetControlsInSelectedPreset();
            if (cmbPresetCharacters.SelectedItem is XV2Character presetCharacter)
                PresetWriter.WritePresets(currentSave.DecryptedData, presetCharacter.Slot - 1, presetCharacter.Presets);
        }

        private void StoreLoadedCharacterControls()
        {
            if (currentSave == null || loadedCharacter == null || isLoadingControls || cmbRace.SelectedIndex < 0) return;
            StoreCharacterControls(loadedCharacter, loadedCharacterSlotIndex);
        }

        private void StoreCharacterControls(XV2Character character, int slot)
        {
            if (currentSave == null || slot is < 0 or >= 8 || character.Slot - 1 != slot) return;
            CharacterWriter.WriteCharacter(currentSave.DecryptedData, slot, cmbRace.SelectedIndex, txtCharacterName.Text,
                (int)nudCharacterLevel.Value, (int)nudCharacterXP.Value, (int)nudAttributePoints.Value,
                (int)nudHealth.Value, (int)nudKi.Value, (int)nudStamina.Value,
                (int)nudBasicAttack.Value, (int)nudStrikeSupers.Value, (int)nudKiBlastSupers.Value);
            AppearanceWriter.WriteAppearance(currentSave.DecryptedData, slot, (int)nudBodyShape.Value,
                (ushort)nudSkinColor1.Value, (ushort)nudSkinColor2.Value, (ushort)nudSkinColor3.Value, (ushort)nudSkinColor4.Value,
                (ushort)nudHairColor.Value, (ushort)nudEyeColor.Value, (ushort)nudMakeupColor1.Value, (ushort)nudMakeupColor2.Value,
                (ushort)nudMakeupColor3.Value, (int)nudFaceBase.Value, (int)nudForehead.Value, (int)nudEyes.Value,
                (int)nudNose.Value, (int)nudEars.Value, (int)nudHair.Value);

            character.Name = txtCharacterName.Text;
            character.Race = cmbRace.SelectedIndex;
            character.Level = (int)nudCharacterLevel.Value;
            character.Experience = (int)nudCharacterXP.Value;
            character.AttributePoints = (int)nudAttributePoints.Value;
            character.Health = (int)nudHealth.Value;
            character.Ki = (int)nudKi.Value;
            character.Stamina = (int)nudStamina.Value;
            character.BasicAttack = (int)nudBasicAttack.Value;
            character.StrikeSupers = (int)nudStrikeSupers.Value;
            character.KiBlastSupers = (int)nudKiBlastSupers.Value;
            character.Appearance ??= new XV2Appearance();
            character.Appearance.BodyShape = (int)nudBodyShape.Value;
            character.Appearance.SkinColor1 = (ushort)nudSkinColor1.Value;
            character.Appearance.SkinColor2 = (ushort)nudSkinColor2.Value;
            character.Appearance.SkinColor3 = (ushort)nudSkinColor3.Value;
            character.Appearance.SkinColor4 = (ushort)nudSkinColor4.Value;
            character.Appearance.HairColor = (ushort)nudHairColor.Value;
            character.Appearance.EyeColor = (ushort)nudEyeColor.Value;
            character.Appearance.MakeupColor1 = (ushort)nudMakeupColor1.Value;
            character.Appearance.MakeupColor2 = (ushort)nudMakeupColor2.Value;
            character.Appearance.MakeupColor3 = (ushort)nudMakeupColor3.Value;
            character.Appearance.FaceBase = (int)nudFaceBase.Value;
            character.Appearance.FaceForehead = (int)nudForehead.Value;
            character.Appearance.Eyes = (int)nudEyes.Value;
            character.Appearance.Nose = (int)nudNose.Value;
            character.Appearance.Ears = (int)nudEars.Value;
            character.Appearance.Hair = (int)nudHair.Value;
        }

        private void TryAutoOpenLastSave()
        {
            if (currentSave != null) return;
            EditorPreferences preferences = EditorPreferences.Load();
            if (!preferences.AutoOpenLastSave) return;
            string? path = preferences.LastSavePath;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;
            try
            {
                currentSave = new SaveFile(path);
                enforceInfiniteDragonBalls = false;
                RefreshLoadedSave(0, preserveDirty: false);
                ResetChangeHistory();
            }
            catch
            {
                // A moved, deleted, or newly incompatible last file must never
                // prevent the editor from opening normally.
                currentSave = null;
            }
        }

        private void RefreshLoadedSave(int selectedSlot, bool preserveDirty)
        {
            if (currentSave == null) return;
            loadedCharacter = null;
            loadedCharacterSlotIndex = -1;
            bool dirty = hasUnsavedChanges;
            isLoadingControls = isLoadingPresetControls = isSynchronizingCharacters = true;
            try
            {
                nudZeni.Value = Math.Min(currentSave.Zeni, 999999999u);
                nudTPMedals.Value = Math.Min(currentSave.TPMedals, 999999999u);
                cmbCharacters.Items.Clear();
                cmbPresetCharacters.Items.Clear();
                foreach (XV2Character character in currentSave.Characters)
                {
                    cmbCharacters.Items.Add(character);
                    cmbPresetCharacters.Items.Add(character);
                }
                if (cmbCharacters.Items.Count > 0)
                {
                    selectedSlot = Math.Clamp(selectedSlot, 0, cmbCharacters.Items.Count - 1);
                    cmbCharacters.SelectedIndex = selectedSlot;
                    cmbPresetCharacters.SelectedIndex = selectedSlot;
                }
            }
            finally { isLoadingControls = isLoadingPresetControls = isSynchronizingCharacters = false; }

            CaptureOriginalPresetSnapshots();
            ResetPresetClipboardState();
            SetEditorEnabled(true);
            btnExportCac.Enabled = btnImportCac.Enabled = true;
            RefreshCacManagementHub();
            LoadSelectedCharacter();
            LoadPresetList();
            UpdatePresetToolButtons();
            RefreshInventoryList();
            SnapshotQQBangs();
            LoadSkillOwnership();
            questProgressSnapshot = null;
            progressionSnapshot = null;
            if (cmbQuestCharacter != null) cmbQuestCharacter.Items.Clear();
            if (cmbMentorCharacter != null) cmbMentorCharacter.Items.Clear();
            if (cmbPartnerCharacter != null) cmbPartnerCharacter.Items.Clear();
            LoadQuestProgress();
            RunDiagnostics();
            RefreshBackupRecovery();
            RefreshDashboard();
            hasUnsavedChanges = preserveDirty ? true : dirty;
            UpdateWindowTitle();
        }
    }
}
