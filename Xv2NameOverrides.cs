using System.Collections.Generic;

namespace XV2SaveEditor
{
    public static class Xv2NameOverrides
    {
        public static IEnumerable<NamedSaveValue> GetAll()
        {
            // =====================================================
            // NEWER / MISSING CLOTHING
            // =====================================================

            yield return new NamedSaveValue
            {
                SaveId = 534,
                DatabaseId = 534,
                Name = "Launch Costume (Custom)",
                Kind = NamedValueKind.Top
            };


            yield return new NamedSaveValue
            {
                SaveId = 543,
                DatabaseId = 543,
                Name = "Fu's Clothes (Custom)",
                Kind = NamedValueKind.Gloves
            };


            // =====================================================
            // NEWER / MODDED AWOKENS
            // =====================================================

            yield return new NamedSaveValue
            {
                SaveId = 27320,
                DatabaseId = 27320,
                Name = "Power to Overcome",
                Kind = NamedValueKind.AwokenSkill
            };


            yield return new NamedSaveValue
            {
                SaveId = 27321,
                DatabaseId = 27321,
                Name = "Power to Overcome (Story Variant)",
                Kind = NamedValueKind.AwokenSkill
            };
        }
    }
}