using System;
using System.Collections.Generic;
using System.IO;

namespace XV2SaveEditor
{
    public class SaveFile
    {
        public string FilePath { get; private set; }

        public byte[] EncryptedData { get; private set; }

        public byte[] DecryptedData { get; private set; }

        public SavePlatform Platform { get; private set; }

        public int Size =>
            EncryptedData.Length;


        // =========================================================
        // CHARACTERS
        // =========================================================

        public List<XV2Character> Characters
        {
            get
            {
                return CharacterReader.ReadCharacters(
                    DecryptedData
                );
            }
        }


        // =========================================================
        // INVENTORY
        // =========================================================

        public XV2Inventory Inventory
        {
            get
            {
                return InventoryReader.Read(
                    DecryptedData
                );
            }
        }

        public List<XV2QQBang> QQBangs =>
            QQBangReader.Read(DecryptedData);


        // =========================================================
        // ZENI
        // =========================================================

        public uint Zeni
        {
            get
            {
                return BitConverter.ToUInt32(
                    DecryptedData,
                    SaveOffsets.Zeni
                );
            }

            set
            {
                byte[] bytes =
                    BitConverter.GetBytes(
                        value
                    );


                Array.Copy(
                    bytes,
                    0,
                    DecryptedData,
                    SaveOffsets.Zeni,
                    bytes.Length
                );
            }
        }


        // =========================================================
        // TP MEDALS
        // =========================================================

        public uint TPMedals
        {
            get
            {
                return BitConverter.ToUInt32(
                    DecryptedData,
                    SaveOffsets.TPMedals
                );
            }

            set
            {
                byte[] bytes =
                    BitConverter.GetBytes(
                        value
                    );


                Array.Copy(
                    bytes,
                    0,
                    DecryptedData,
                    SaveOffsets.TPMedals,
                    bytes.Length
                );
            }
        }


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public SaveFile(
            string filePath)
        {
            FilePath =
                filePath;


            PlatformSaveData platformSave = PlatformSaveAdapter.Load(filePath);
            Platform = platformSave.Platform;
            EncryptedData = platformSave.OriginalData;
            DecryptedData = platformSave.DecryptedData;


            if (
                DecryptedData.Length !=
                SaveOffsets.DecryptedSize)
            {
                throw new InvalidDataException(
                    "The save did not decrypt to the expected size."
                );
            }


            // =====================================================
            // VALIDATE INVENTORY LAYOUT
            // =====================================================

            // This forces the inventory reader to run once when
            // the save is opened.
            //
            // If one of our inventory offsets is incorrect or the
            // save isn't large enough, we'll find out immediately
            // instead of later when opening the Inventory tab.

            _ =
                InventoryReader.Read(
                    DecryptedData
                );

            QQBangReader.ValidateSection(
                DecryptedData
            );
        }


        // =========================================================
        // BACKUP
        // =========================================================

        public string CreateBackup()
        {
            return CreateBackupForPath(FilePath, Platform);
        }

        public static string GetBackupRootDirectory() => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "XV2 Save Editor Backups");

        public static string GetBackupDirectory(string savePath, SavePlatform? platform = null)
        {
            string fullPath = Path.GetFullPath(savePath);
            string platformFolder = (platform ?? GuessBackupPlatform(fullPath)) switch
            {
                SavePlatform.PC => "PC - Steam",
                SavePlatform.Xbox => "Xbox",
                SavePlatform.PlayStation or SavePlatform.PlayStationEncrypted => "PlayStation",
                _ => "Unknown"
            };
            string name = Path.GetFileNameWithoutExtension(fullPath);
            if (string.IsNullOrWhiteSpace(name)) name = "Unnamed Save";
            foreach (char invalid in Path.GetInvalidFileNameChars()) name = name.Replace(invalid, '_');
            if (name.Length > 48) name = name[..48];
            byte[] pathBytes = System.Text.Encoding.UTF8.GetBytes(fullPath.ToUpperInvariant());
            string sourceId = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(pathBytes))[..8];
            return Path.Combine(GetBackupRootDirectory(), platformFolder, $"{name}_{sourceId}");
        }

        public static IEnumerable<string> GetLegacyBackupDirectories(string savePath)
        {
            string directory = Path.GetDirectoryName(Path.GetFullPath(savePath)) ?? "";
            yield return Path.Combine(directory, "XV2SaveEditor Backups");
            yield return directory;
        }

        public static string CreateBackupForPath(string savePath, SavePlatform? platform = null)
        {
            if (!File.Exists(savePath)) throw new FileNotFoundException("The save to back up was not found.", savePath);
            platform ??= TryDetectBackupPlatform(savePath);
            string backupDirectory = GetBackupDirectory(savePath, platform);
            Directory.CreateDirectory(backupDirectory);
            string sourceInfo = Path.Combine(backupDirectory, "SOURCE.txt");
            if (!File.Exists(sourceInfo))
                File.WriteAllText(sourceInfo, $"Original save: {Path.GetFullPath(savePath)}{Environment.NewLine}Platform: {platform?.ToString() ?? "Unknown"}{Environment.NewLine}");
            string name = Path.GetFileNameWithoutExtension(savePath);
            string extension = Path.GetExtension(savePath);
            string stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
            string backupPath = Path.Combine(backupDirectory, $"{name}_BACKUP_{stamp}{extension}");
            File.Copy(savePath, backupPath, false);
            return backupPath;
        }

        private static SavePlatform GuessBackupPlatform(string savePath) => Path.GetExtension(savePath).ToLowerInvariant() switch
        {
            ".sav" => SavePlatform.PC,
            ".bin" => SavePlatform.Xbox,
            ".dat" => SavePlatform.PlayStation,
            _ => SavePlatform.Xbox
        };

        private static SavePlatform? TryDetectBackupPlatform(string savePath)
        {
            try { return PlatformSaveAdapter.Load(savePath).Platform; }
            catch { return GuessBackupPlatform(savePath); }
        }


        // =========================================================
        // SAVE AS
        // =========================================================

        public void SaveAs(
            string outputPath)
        {
            if (File.Exists(outputPath))
            {
                CreateBackupForPath(outputPath);
            }

            byte[] encrypted = PlatformSaveAdapter.Encode(Platform, DecryptedData);


            File.WriteAllBytes(
                outputPath,
                encrypted
            );
        }
    }
}
