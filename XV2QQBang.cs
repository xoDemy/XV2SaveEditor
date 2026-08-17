using System;

namespace XV2SaveEditor
{
    public sealed class XV2QQBang
    {
        public int SlotIndex { get; init; }
        public sbyte Health { get; set; }
        public sbyte Ki { get; set; }
        public sbyte Stamina { get; set; }
        public sbyte BasicAttack { get; set; }
        public sbyte StrikeSupers { get; set; }
        public sbyte KiBlastSupers { get; set; }
        public byte Metadata { get; init; }
        public byte I_07 { get; init; }
        public byte Quantity { get; set; }
        public byte[] OriginalBytes { get; init; } = Array.Empty<byte>();

        public int Total =>
            Health + Ki + Stamina + BasicAttack + StrikeSupers + KiBlastSupers;

        public string StatsText =>
            $"HP {Format(Health)}  Ki {Format(Ki)}  STM {Format(Stamina)}  " +
            $"BAS {Format(BasicAttack)}  STR {Format(StrikeSupers)}  KBL {Format(KiBlastSupers)}";

        public string DisplayName =>
            $"Slot {SlotIndex:D3}  x{Quantity}  |  {StatsText}";

        public XV2QQBang Clone() => new XV2QQBang
        {
            SlotIndex = SlotIndex,
            Health = Health,
            Ki = Ki,
            Stamina = Stamina,
            BasicAttack = BasicAttack,
            StrikeSupers = StrikeSupers,
            KiBlastSupers = KiBlastSupers,
            Metadata = Metadata,
            I_07 = I_07,
            Quantity = Quantity,
            OriginalBytes = (byte[])OriginalBytes.Clone()
        };

        public override string ToString() => DisplayName;

        private static string Format(sbyte value) =>
            value > 0 ? $"+{value}" : value.ToString();
    }
}
