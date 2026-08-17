namespace XV2SaveEditor;

public sealed record PreSaveIssue(string Severity, string Area, string Message, string? SafeRepair, Action? Repair);

public static class PreSaveValidator
{
    public static List<PreSaveIssue> Inspect(SaveFile save)
    {
        List<PreSaveIssue> issues = new();
        if (save.DecryptedData.Length != SaveOffsets.DecryptedSize)
            issues.Add(new("Error", "Format", $"Decrypted data has an invalid size ({save.DecryptedData.Length:N0} bytes).", null, null));

        if (save.Zeni > 999_999_999u)
            issues.Add(new("Warning", "Currency", $"Zeni exceeds 999,999,999 ({save.Zeni:N0}).", "Clamp Zeni", () => save.Zeni = 999_999_999u));
        if (save.TPMedals > 999_999_999u)
            issues.Add(new("Warning", "Currency", $"TP Medals exceed 999,999,999 ({save.TPMedals:N0}).", "Clamp TP Medals", () => save.TPMedals = 999_999_999u));

        foreach (XV2Character character in save.Characters.Where(x => !x.IsEmpty))
        {
            int slot = character.Slot - 1;
            if (character.Race is < 0 or > 7)
                issues.Add(new("Error", $"CaC {character.Slot}", $"{character.Name} has invalid race ID {character.Race}.", null, null));
            if (!LevelExperience.IsValidLevel(character.Level))
                issues.Add(new("Error", $"CaC {character.Slot}", $"{character.Name} has invalid level {character.Level}.", null, null));
            else
            {
                int expected = LevelExperience.ExperienceForLevel(character.Level);
                if (character.Experience != expected)
                    issues.Add(new("Warning", $"CaC {character.Slot}", $"{character.Name}'s XP does not match level {character.Level}.", "Correct XP", () => WriteInt(save.DecryptedData, slot, 176, expected)));
            }

            int[] attributes = { character.Health, character.Ki, character.Stamina, character.BasicAttack, character.StrikeSupers, character.KiBlastSupers };
            if (attributes.Any(value => value < 0))
                issues.Add(new("Error", $"CaC {character.Slot}", $"{character.Name} has a negative allocated attribute.", null, null));
            if (attributes.Any(value => value > 200))
                issues.Add(new("Error", $"CaC {character.Slot}", $"{character.Name} has an attribute above the current game limit of 200.", null, null));
            if (character.AttributePoints < 0)
                issues.Add(new("Error", $"CaC {character.Slot}", $"{character.Name} has negative unspent attribute points.", null, null));
            int total = attributes.Sum() + character.AttributePoints;
            if (total > 600)
                issues.Add(new("Warning", $"CaC {character.Slot}", $"{character.Name} has {total} total attribute points; the verified level-199 total is 600.", null, null));

            foreach (XV2QuestProgress quest in QuestProgressReader.Read(save.DecryptedData, slot))
            {
                if (quest.State is < 0 or > 3) issues.Add(new("Error", $"CaC {character.Slot} quests", $"{quest.Category} {quest.ID} has invalid state {quest.State}.", null, null));
                if (quest.Rank is < 0 or > 7) issues.Add(new("Error", $"CaC {character.Slot} quests", $"{quest.Category} {quest.ID} has invalid rank {quest.Rank}.", null, null));
                if (quest.Score < 0) issues.Add(new("Error", $"CaC {character.Slot} quests", $"{quest.Category} {quest.ID} has a negative score.", null, null));
            }
        }

        byte[] verified = (byte[])save.DecryptedData.Clone();
        LevelCapFlagValidator.Apply(verified);
        if (!verified.SequenceEqual(save.DecryptedData))
            issues.Add(new("Warning", "Level caps", "Verified level-cap flags are missing for one or more CaCs.", "Apply verified cap flags", () => LevelCapFlagValidator.Apply(save.DecryptedData)));
        return issues;
    }

    private static void WriteInt(byte[] data, int slot, int relativeOffset, int value) =>
        BitConverter.GetBytes(value).CopyTo(data, CharacterReader.CharacterSectionOffset + slot * CharacterReader.CharacterStride + relativeOffset);
}
