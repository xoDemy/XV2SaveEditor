using System;
using System.Text;

namespace XV2SaveEditor
{
    public static class CharacterWriter
    {
        public const int CharacterSectionOffset = 124976;
        public const int CharacterStride = 50888;

        public static void WriteCharacter(
            byte[] data,
            int slotIndex,
            int race,
            string name,
            int level,
            int experience,
            int attributePoints,
            int health,
            int ki,
            int stamina,
            int basicAttack,
            int strikeSupers,
            int kiBlastSupers)
        {
            if (slotIndex < 0 || slotIndex >= 8)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(slotIndex)
                );
            }

            int offset =
                CharacterSectionOffset +
                (CharacterStride * slotIndex);

            // Race
            WriteInt32(
                data,
                offset + 20,
                race
            );

            // Name
            WriteName(
                data,
                offset + 68,
                name
            );

            // Level
            WriteInt32(
                data,
                offset + 172,
                level
            );

            // Experience
            WriteInt32(
                data,
                offset + 176,
                experience
            );

            // Attribute Points
            WriteInt32(
                data,
                offset + 180,
                attributePoints
            );

            // Health
            WriteInt32(
                data,
                offset + 184,
                health
            );

            // Ki
            WriteInt32(
                data,
                offset + 188,
                ki
            );

            // Basic Attack
            WriteInt32(
                data,
                offset + 192,
                basicAttack
            );

            // Strike Supers
            WriteInt32(
                data,
                offset + 196,
                strikeSupers
            );

            // Ki Blast Supers
            WriteInt32(
                data,
                offset + 200,
                kiBlastSupers
            );

            // Stamina
            WriteInt32(
                data,
                offset + 204,
                stamina
            );
        }

        private static void WriteInt32(
            byte[] data,
            int offset,
            int value)
        {
            byte[] bytes =
                BitConverter.GetBytes(value);

            Array.Copy(
                bytes,
                0,
                data,
                offset,
                4
            );
        }

        private static void WriteName(
            byte[] data,
            int offset,
            string name)
        {
            if (name == null)
            {
                name = "";
            }

            byte[] nameBytes =
                Encoding.UTF8.GetBytes(name);

            if (nameBytes.Length > 63)
            {
                throw new ArgumentException(
                    "Character name is too long."
                );
            }

            Array.Clear(
                data,
                offset,
                64
            );

            Array.Copy(
                nameBytes,
                0,
                data,
                offset,
                nameBytes.Length
            );
        }
    }
}