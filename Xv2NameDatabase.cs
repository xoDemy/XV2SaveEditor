using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace XV2SaveEditor
{
    public class Xv2NameDatabase
    {
        private readonly Dictionary<
            NamedValueKind,
            Dictionary<int, NamedSaveValue>>
            values =
                new Dictionary<
                    NamedValueKind,
                    Dictionary<int, NamedSaveValue>>();


        private readonly Dictionary<
            NamedValueKind,
            List<NamedSaveValue>>
            sortedCache =
                new Dictionary<
                    NamedValueKind,
                    List<NamedSaveValue>>();


        public bool IsLoaded { get; private set; }


        public Xv2NameDatabase()
        {
            foreach (
                NamedValueKind kind
                in Enum.GetValues(
                    typeof(NamedValueKind)))
            {
                values[kind] =
                    new Dictionary<
                        int,
                        NamedSaveValue>();


                sortedCache[kind] =
                    new List<NamedSaveValue>();
            }
        }


        // =========================================================
        // LOAD DATABASE
        // =========================================================

        public void LoadFromGameData(
            string rootDirectory,
            bool clearExisting = true)
        {
            if (
                !Directory.Exists(
                    rootDirectory
                ))
            {
                throw new DirectoryNotFoundException(
                    $"GameData directory was not found:\n{rootDirectory}"
                );
            }


            if (clearExisting)
            {
                Clear();
            }


            // =====================================================
            // EQUIPMENT MSG FILES
            // =====================================================

            Xv2MsgFile costumeMsg =
                LoadMsg(
                    rootDirectory,
                    "proper_noun_costume_name_en.msg"
                );


            Xv2MsgFile accessoryMsg =
                LoadMsg(
                    rootDirectory,
                    "proper_noun_accessory_name_en.msg"
                );


            Xv2MsgFile soulMsg =
                LoadMsg(
                    rootDirectory,
                    "proper_noun_talisman_name_en.msg"
                );


            // =====================================================
            // INVENTORY ITEM MSG FILES
            // =====================================================

            Xv2MsgFile materialMsg =
                LoadMsg(
                    rootDirectory,
                    "proper_noun_material_name_en.msg"
                );


            Xv2MsgFile battleMsg =
                LoadMsg(
                    rootDirectory,
                    "proper_noun_battle_name_en.msg"
                );


            // =====================================================
            // CLOTHING
            // =====================================================

            LoadEquipment(
                rootDirectory,
                "costume_top_item.idb",
                costumeMsg,
                NamedValueKind.Top
            );


            LoadEquipment(
                rootDirectory,
                "costume_bottom_item.idb",
                costumeMsg,
                NamedValueKind.Bottom
            );


            LoadEquipment(
                rootDirectory,
                "costume_gloves_item.idb",
                costumeMsg,
                NamedValueKind.Gloves
            );


            LoadEquipment(
                rootDirectory,
                "costume_shoes_item.idb",
                costumeMsg,
                NamedValueKind.Shoes
            );


            // =====================================================
            // ACCESSORY
            // =====================================================

            LoadEquipment(
                rootDirectory,
                "accessory_item.idb",
                accessoryMsg,
                NamedValueKind.Accessory
            );


            // =====================================================
            // SUPER SOUL
            // =====================================================

            LoadEquipment(
                rootDirectory,
                "talisman_item.idb",
                soulMsg,
                NamedValueKind.SuperSoul
            );


            // =====================================================
            // MIX ITEMS
            // =====================================================

            LoadEquipment(
                rootDirectory,
                "material_item.idb",
                materialMsg,
                NamedValueKind.MixItem
            );


            // =====================================================
            // BATTLE ITEMS / CAPSULES
            // =====================================================

            LoadEquipment(
                rootDirectory,
                "battle_item.idb",
                battleMsg,
                NamedValueKind.Capsule
            );


            // =====================================================
            // SKILL MSG FILES
            // =====================================================

            Xv2MsgFile superMsg =
                LoadMsg(
                    rootDirectory,
                    "proper_noun_skill_spa_name_en.msg"
                );


            Xv2MsgFile ultimateMsg =
                LoadMsg(
                    rootDirectory,
                    "proper_noun_skill_ult_name_en.msg"
                );


            Xv2MsgFile evasiveMsg =
                LoadMsg(
                    rootDirectory,
                    "proper_noun_skill_esc_name_en.msg"
                );


            Xv2MsgFile awokenMsg =
                LoadMsg(
                    rootDirectory,
                    "proper_noun_skill_met_name_en.msg"
                );


            // =====================================================
            // CUS
            // =====================================================

            string cusPath =
                FindFile(
                    rootDirectory,
                    "custom_skill.cus"
                );


            Xv2CusFile cus =
                Xv2CusReader.Load(
                    cusPath
                );


            foreach (
                Xv2CusSkill skill
                in cus.Skills)
            {
                switch (skill.Kind)
                {
                    case Xv2CusSkillKind.Super:

                        AddCusSkill(
                            skill,
                            superMsg,
                            NamedValueKind.SuperSkill
                        );

                        break;


                    case Xv2CusSkillKind.Ultimate:

                        AddCusSkill(
                            skill,
                            ultimateMsg,
                            NamedValueKind.UltimateSkill
                        );

                        break;


                    case Xv2CusSkillKind.Evasive:

                        AddCusSkill(
                            skill,
                            evasiveMsg,
                            NamedValueKind.EvasiveSkill
                        );

                        break;


                    case Xv2CusSkillKind.Awoken:

                        AddCusSkill(
                            skill,
                            awokenMsg,
                            NamedValueKind.AwokenSkill
                        );

                        break;


                    case Xv2CusSkillKind.Blast:

                        break;
                }
            }


            // =====================================================
            // NEWER DLC / MODDED OVERRIDES
            // =====================================================

            ApplyOverrides();


            // =====================================================
            // BUILD SORTED CACHE ONCE
            // =====================================================

            BuildSortedCache();


            IsLoaded =
                true;
        }


        // =========================================================
        // EMBEDDED FULL DATABASE INVENTORY CATALOGUES
        // =========================================================

        public void LoadFromEmbeddedInventoryCatalogs()
        {
            Clear();

            LoadEmbeddedCatalog("equip_top.json", NamedValueKind.Top);
            LoadEmbeddedCatalog("equip_bottom.json", NamedValueKind.Bottom);
            LoadEmbeddedCatalog("equip_gloves.json", NamedValueKind.Gloves);
            LoadEmbeddedCatalog("equip_shoes.json", NamedValueKind.Shoes);
            LoadEmbeddedCatalog("equip_accessory.json", NamedValueKind.Accessory);
            LoadEmbeddedCatalog("equip_supersoul.json", NamedValueKind.SuperSoul);
            LoadEmbeddedCatalog("equip_material.json", NamedValueKind.MixItem);
            LoadEmbeddedCatalog("equip_important.json", NamedValueKind.ImportantItem);

            // The archive's equip_capsule.json is empty. XenoKing's
            // battle-item catalogue is the verified Capsule name source.
            LoadEmbeddedCatalog("equip_battle.json", NamedValueKind.Capsule);

            LoadEmbeddedSkillCatalog("skills_super.json", NamedValueKind.SuperSkill);
            LoadEmbeddedSkillCatalog("skills_ultimate.json", NamedValueKind.UltimateSkill);
            LoadEmbeddedSkillCatalog("skills_evasive.json", NamedValueKind.EvasiveSkill);
            LoadEmbeddedSkillCatalog("skills_awoken.json", NamedValueKind.AwokenSkill);

            ApplyOverrides();
            BuildSortedCache();
            IsLoaded = true;
        }


        private void LoadEmbeddedCatalog(
            string fileName,
            NamedValueKind kind)
        {
            Assembly assembly = typeof(Xv2NameDatabase).Assembly;
            string suffix = $".Data.Inventory.{fileName}";
            string? resourceName = assembly
                .GetManifestResourceNames()
                .FirstOrDefault(x => x.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));

            if (resourceName == null)
            {
                throw new FileNotFoundException(
                    $"Embedded inventory catalogue was not found: {fileName}");
            }

            using Stream stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new FileNotFoundException(
                    $"Embedded inventory catalogue could not be opened: {fileName}");

            CatalogEntry[] entries =
                JsonSerializer.Deserialize<CatalogEntry[]>(
                    stream,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    })
                ?? Array.Empty<CatalogEntry>();

            foreach (CatalogEntry entry in entries)
            {
                if (entry.Id < 0 || string.IsNullOrWhiteSpace(entry.Name))
                {
                    continue;
                }

                values[kind][entry.Id] = new NamedSaveValue
                {
                    SaveId = entry.Id,
                    DatabaseId = entry.Id,
                    Name = entry.Name.Trim(),
                    Kind = kind
                };
            }
        }


        private sealed class CatalogEntry
        {
            public int Id { get; set; }

            public string Name { get; set; } = "";
        }

        private void LoadEmbeddedSkillCatalog(string fileName, NamedValueKind kind)
        {
            Assembly assembly = typeof(Xv2NameDatabase).Assembly;
            string suffix = $".Data.Skills.{fileName}";
            string? resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(x => x.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
            if (resourceName == null) throw new FileNotFoundException($"Embedded skill catalogue was not found: {fileName}");
            using Stream stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new FileNotFoundException($"Embedded skill catalogue could not be opened: {fileName}");
            SkillCatalogEntry[] entries = JsonSerializer.Deserialize<SkillCatalogEntry[]>(stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? Array.Empty<SkillCatalogEntry>();
            foreach (SkillCatalogEntry entry in entries)
            {
                if (entry.Id < 0 || string.IsNullOrWhiteSpace(entry.Name) || entry.Name.Equals("- Error -", StringComparison.OrdinalIgnoreCase)) continue;
                values[kind][entry.Id] = new NamedSaveValue
                {
                    SaveId = entry.Id, DatabaseId = entry.Id2, Name = entry.Name.Trim(), Kind = kind
                };
            }
        }

        private sealed class SkillCatalogEntry
        {
            public int Id { get; set; }
            public int Id2 { get; set; }
            public string Name { get; set; } = "";
        }


        // =========================================================
        // EQUIPMENT / NORMAL ITEMS
        // =========================================================

        private void LoadEquipment(
            string rootDirectory,
            string idbName,
            Xv2MsgFile msg,
            NamedValueKind kind)
        {
            string path =
                FindFile(
                    rootDirectory,
                    idbName
                );


            Xv2IdbFile idb =
                Xv2IdbReader.Load(
                    path
                );


            foreach (
                Xv2IdbEntry entry
                in idb.Entries)
            {
                if (values[kind].TryGetValue(entry.ID, out NamedSaveValue? existing))
                {
                    // Embedded catalogues are the primary vanilla/DLC name source.
                    // Preserve their name while enriching the record with IDB metadata.
                    existing.RaceMask = entry.RaceLock;
                    continue;
                }

                string name =
                    msg.GetText(
                        entry.NameMsgID
                    );


                if (
                    string.IsNullOrWhiteSpace(
                        name
                    ))
                {
                    name =
                        $"Unknown {kind}";
                }


                NamedSaveValue value =
                    new NamedSaveValue
                    {
                        SaveId =
                            entry.ID,

                        DatabaseId =
                            entry.ID,

                        Name =
                            name,

                        Kind =
                            kind,

                        RaceMask =
                            entry.RaceLock
                    };


                values[kind][
                    value.SaveId
                ] = value;
            }
        }


        // =========================================================
        // CUS SKILLS
        // =========================================================

        private void AddCusSkill(
            Xv2CusSkill skill,
            Xv2MsgFile msg,
            NamedValueKind kind)
        {
            int saveId =
                skill.ID1;


            int databaseId =
                skill.ID2;


            string msgKey =
                CreateSkillMsgKey(
                    skill.Kind,
                    databaseId
                );


            string name =
                msg.GetTextByKey(
                    msgKey
                );


            if (
                string.IsNullOrWhiteSpace(
                    name
                ))
            {
                if (
                    !string.IsNullOrWhiteSpace(
                        skill.ShortCode
                    ))
                {
                    name =
                        $"Unknown {kind} [{skill.ShortCode}]";
                }
                else
                {
                    name =
                        $"Unknown {kind}";
                }
            }


            NamedSaveValue value =
                new NamedSaveValue
                {
                    SaveId =
                        saveId,

                    DatabaseId =
                        databaseId,

                    Name =
                        name,

                    Kind =
                        kind
                };


            values[kind][
                saveId
            ] = value;
        }


        // =========================================================
        // OVERRIDES
        // =========================================================

        private void ApplyOverrides()
        {
            foreach (
                NamedSaveValue value
                in Xv2NameOverrides.GetAll())
            {
                values[value.Kind][
                    value.SaveId
                ] = value;
            }
        }


        // =========================================================
        // BUILD SORTED CACHE
        // =========================================================

        private void BuildSortedCache()
        {
            foreach (
                NamedValueKind kind
                in values.Keys)
            {
                sortedCache[kind] =
                    values[kind]
                        .Values
                        .OrderBy(
                            x => x.Name,
                            StringComparer.OrdinalIgnoreCase
                        )
                        .ThenBy(
                            x => x.SaveId
                        )
                        .ToList();
            }
        }


        // =========================================================
        // BUILD SKILL MSG KEY
        // =========================================================

        private static string CreateSkillMsgKey(
            Xv2CusSkillKind kind,
            int id2)
        {
            return kind switch
            {
                Xv2CusSkillKind.Super =>
                    $"spe_skill_{id2:D4}",

                Xv2CusSkillKind.Ultimate =>
                    $"ult_{id2:D4}",

                Xv2CusSkillKind.Evasive =>
                    $"avoid_skill_{id2:D4}",

                Xv2CusSkillKind.Awoken =>
                    $"met_skill_{id2:D4}",

                _ =>
                    ""
            };
        }


        // =========================================================
        // GET VALUES
        // =========================================================

        public List<NamedSaveValue> GetValues(
            NamedValueKind kind)
        {
            return sortedCache[kind];
        }


        // =========================================================
        // GET OR UNKNOWN
        // =========================================================

        public NamedSaveValue GetOrUnknown(
            NamedValueKind kind,
            int saveId)
        {
            if (
                values[kind]
                    .TryGetValue(
                        saveId,
                        out NamedSaveValue? value
                    ))
            {
                return value;
            }


            return new NamedSaveValue
            {
                SaveId =
                    saveId,

                DatabaseId =
                    saveId,

                Name =
                    "Unknown / Modded",

                Kind =
                    kind
            };
        }


        // =========================================================
        // TRY GET
        // =========================================================

        public bool TryGetValue(
            NamedValueKind kind,
            int saveId,
            out NamedSaveValue? value)
        {
            return values[kind]
                .TryGetValue(
                    saveId,
                    out value
                );
        }


        // =========================================================
        // FILE HELPERS
        // =========================================================

        private static Xv2MsgFile LoadMsg(
            string rootDirectory,
            string fileName)
        {
            return Xv2MsgReader.Load(
                FindFile(
                    rootDirectory,
                    fileName
                )
            );
        }


        private static string FindFile(
            string rootDirectory,
            string fileName)
        {
            string[] matches =
                Directory.GetFiles(
                    rootDirectory,
                    fileName,
                    SearchOption.AllDirectories
                );


            if (matches.Length == 0)
            {
                throw new FileNotFoundException(
                    $"Required game data file was not found:\n{fileName}"
                );
            }


            return matches[0];
        }


        // =========================================================
        // CLEAR
        // =========================================================

        private void Clear()
        {
            foreach (
                Dictionary<int, NamedSaveValue>
                dictionary
                in values.Values)
            {
                dictionary.Clear();
            }


            foreach (
                List<NamedSaveValue>
                list
                in sortedCache.Values)
            {
                list.Clear();
            }


            IsLoaded =
                false;
        }
    }
}
