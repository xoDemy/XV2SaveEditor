namespace XV2SaveEditor;

public static class PartnerCustomizationInitializer
{
    private const int RegularOffset = 520096, RegularStride = 25392, RegularCount = 47, RegularSize = 92;
    private const int DlcOffset = 524432, DlcStride = 25392, DlcCount = 10, DlcSize = 44;
    private const int RegularFlagsOffset = 504756, DlcFlagsOffset = 506492, FestivalFlagsOffset = 722972;

    // Positions 33-44 are reserved/non-partner records and remain untouched.
    private static readonly int[] RegularPartnerPositions = Enumerable.Range(0, 33).Concat(new[] { 45, 46 }).ToArray();

    // Verified materialized blocks from the current save revision. They are only
    // used when no already-materialized CaC in the loaded save can seed a record.
    private static readonly byte[] RegularTemplate = Convert.FromBase64String(
        "MgAXALEBBgD//////////wYAAAD/////BgD///////8AAP///////wAA////////pAAAAGQAAAA0AAAAIAAAAGQAAAANAAAASwEAAP////8AAAAAAAAAAAAAAAAyABcAsQEGAP////////////////////8AAAYA/////wYABgD/////BgAAAP////8AAAAAeAAAAHkAAAAOAAAACwAAADIAAABLAQAA/////wAAAAAAAAAAAAAAADIAFwCxAQYA//////////8GAAAA/////wYA////////AAD///////8GAAAA/////wAAAAB5AAAAVAIAACAAAAAAAAAAbgAAAEsBAAD/////AAAAAAAAAAAAAAAAMgAXACoABgD//////////wYAAAD/////BgD//////////////////wAA////////XgAAAFwAAABgAAAAIAAAAFoAAAALAAAASwEAAP////8AAAAAAAAAAAAAAAAyABcADAAGAP//////////BgAAAAAA//8GAP///////wAABgD/////AAAGAP////+kAAAAgwAAAIIAAAAWAAAAlwAAAAsAAABLAQAA/////wAAAAAAAAAAAAAAADIAFwCxAQYA//////////8AAAYAAAD//wYA////////AAD///////8AAP///////wAAAABOAgAAVAIAAA4AAAA8AAAADQAAAEsBAAD/////AAAAAAAAAAAAAAAAMgAwAAwABgD//////////wAABgD///////////////8AAAYA/////wAABgD/////lgAAAJgAAAAPAAAAVgIAAA0AAAALAAAASwEAAAcAAAAAAAAAAAAAAAAAAAAeADAAhQAGAP//////////BgAGAP///////////////wYA////////BgAGAP////+jAAAAVQIAAFQCAAAWAAAAoQAAAAsAAABLAQAAoQAAAAAAAAAAAAAAAAAAADIAFwATAAYA//////////8AAAYABgAGAAYA//////////////////8AAP///////1UCAAA0AAAAVAIAACAAAADEBAAA8QAAAEsBAAD/////AAAAAAAAAAAAAAAAMgAXACMABgD//////////wYABgD//////////////////////////wYABgD/////0AQAAM4EAAD2AAAAFgAAAPAAAAALAAAASwEAAP////8AAAAAAAAAAAAAAAAyABcAmAEGAP//////////BgAGAP///////////////wYA////////BgAGAP////+kAAAANAAAAN4AAAAgAAAADQAAAAsAAABLAQAA/////wAAAAAAAAAAAAAAADIAFwAjAAYABgAGAAYA//8GAAYA////////////////BgD///////8GAP////////AAAADyAAAANQMAABYAAADwAAAACwAAAEsBAAD/////AAAAAAAAAAAAAAAACwAXALEBBgD//////////wYAAAAAAP//BgAAAAAA/////////////wAA////////VQIAAC4BAAAPAAAAFgAAAC0BAAALAAAASwEAAP////8AAAAAAAAAAAAAAAAyABcAyAEGAAYAAAAAAP//////////////////////////////////BgD////////wAAAAUQAAAFUCAAAWAAAAIAMAACgAAABLAQAA/////wAAAAAAAAAAAAAAADIAFwCoAQYA//////////8AAAYAAAAAAP////////////////////8GAAAA/////7wEAABUAgAADwAAAA4AAAC6BAAADQAAAEsBAAD/////AAAAAAAAAAAAAAAAMgAXAIgABgAGAP///////wAAAAAGAP//BgAAAAYA////////////////////////YgEAAHIBAABgAQAADgAAAF8BAABeAQAASwEAAP////8AAAAAAAAAAAAAAAAKABcAoAEGAP//////////BgAAAP////8GAP//////////////////BgAAAP////8IAgAAfAAAAAoCAAAOAAAACAIAAAoCAAC1AAAA/////wAAAAAAAAAAAAAAADIAMACxAQYA//////////8GAAAA/////wYA////////AAD///////8GAAAA/////1EAAABUAgAAUAAAACAAAABaAAAACwAAAEsBAABQAAAAAAAAAAAAAAAAAAAAMgAwADgABgD//////////wAABgAAAP//BgD///////8AAP///////wYAAAD/////UQAAAKQAAABOAgAAIAAAAKEAAAALAAAASwEAACgAAAAAAAAAAAAAAAAAAAAyABcAqAEGAP//////////AAAGAP///////////////wAABgD/////AAAGAP////9OAgAAgwAAAFUCAAAgAAAAZgMAAA0AAABLAQAA/////wAAAAAAAAAAAAAAADIAFwCcAAYA//////////8GAP///////wYABgD/////BgD///////8GAAYA/////5EBAACQAQAAkgEAABYAAABmAwAAkAEAAFQBAAD/////AAAAAAAAAAAAAAAAMgAXAHAABgD//////////wYA////////BgAAAP///////////////wAA////////UQAAAFQCAABVAgAAFgAAAJoBAAALAAAASwEAAP////8AAAAAAAAAAAAAAAAyABcAwAEGAP//////////AAAGAAAA//8GAAAAAAD//wAA////////AAAGAAYA//+kAAAANAAAAFUBAAAgAAAAAAAAAKQBAABLAQAA/////wAAAAAAAAAAAAAAADIAFwBuAAYA//////////8GAAYA////////////////BgD///////8GAP///////54CAABTAgAAUgIAAA4AAACeAgAACAIAAP//////////AAAAAAAAAAAAAAAAHwAwAMkBBgD//////////wYAAAAAAP//BgD///////8AAP///////wAABgAGAP//IQMAAAIAAAAgAwAAIAAAACADAAAEAAAASwEAAAkAAAAAAAAAAAAAAAAAAAAyABcAcAAGAP//////////AAAGAAAA//8AAP//////////////////BgAAAP////9SBQAAjQIAAIsCAABTBQAAigIAAFAFAACKAgAA/////wAAAAAAAAAAAAAAADIAFwAjAAYAAAAGAP///////////////////////////////////////////////1EAAADwAAAA8gAAAA4AAABSAwAACwAAAEsBAAD/////AAAAAAAAAAAAAAAAMgAXAL4BBgD//////////wYAAAAAAP///////////////////////wAA////////QAMAAA8AAAA/AwAAFgAAAD4DAAALAAAASwEAAP////8AAAAAAAAAAAAAAAAyADAAsQEGAP//////////BgAAAAAA//8GAP///////wAA////////AAAGAP////9eAAAATgIAAF0DAAAgAAAAWgAAAFwDAABLAQAAXAMAAAAAAAAAAAAAAAAAADIAMACdAAYA//////////8AAAYA////////////////AAD///////8AAAAABgD//1EAAABVAgAAgwAAACAAAAAMAAAACwAAAEsBAAAJAAAAAAAAAAAAAAAAAAAAMgAwAHQABgD//////////wYAAAAAAAYAAAAAAP////8AAP///////wAABgD/////UQAAAB4AAAAWAAAAcwAAAOwEAADtBAAASwEAAP////8AAAAAAAAAAAAAAAAyABcAfwAGAP//////////AAAGAAAA//8AAAYA////////////////AAAGAP////94BQAAVAIAAHoFAAAgAAAAeAUAAA0AAABLAQAA/////wAAAAAAAAAAAAAAADIAFwB9AAYA//////////8GAAYAAAAAAAAA////////////////qmtnAOIAAAD//2QFAAAeBQAAVQIAACAAAAANAAAAZAUAAEsBAAD/////AAAAAAAAAAAAAAAA//8XAP//tgAAAOcFAABvBQAAuQEAAOYFAAAiBgAA//////////////////////////////////////////////////////////////////8AAAAAAAAAAAAAAAD//xcA//8AAP///////////////////////////////////////////////////////////////////////////////////////////////wAAAAAAAAAAAAAAAP//FwD//wAA////////////////////////////////////////////////////////////////////////////////////////////////AAAAAAAAAAAAAAAA//8XAP//AAD///////////////////////////////////////////////////////////////////////////////////////////////8AAAAAAAAAAAAAAAD//xcA//8AAP///////////////////////////////////////////////////////////////////////////////////////////////wAAAAAAAAAAAAAAAP//FwD//wAA////////////////////////////////////////////////////////////////////////////////////////////////AAAAAAAAAAAAAAAA//8XAP//AAD///////////////////////////////////////////////////////////////////////////////////////////////8AAAAAAAAAAAAAAAD//xcA//8AAP///////////////////////////////////////////////////////////////////////////////////////////////wAAAAAAAAAAAAAAAP//FwD//wAA////////////////////////////////////////////////////////////////////////////////////////////////AAAAAAAAAAAAAAAA//8XAP//AAD///////////////////////////////////////////////////////////////////////////////////////////////8AAAAAAAAAAAAAAAD//xcA//8AAP///////////////////////////////////////////////////////////////////////////////////////////////wAAAAAAAAAAAAAAAP//FwD//wAA////////////////////////////////////////////////////////////////////////////////////////////////AAAAAAAAAAAAAAAA//8XAP//AAD///////////////////////////////////////////////////////////////////////////////////////////////8AAAAAAAAAAAAAAAD//xcA//8AAP///////////////////////////////////////////////////////////////////////////////////////////////wAAAAAAAAAAAAAAAP//FwD//wAA////////////////////////////////////////////////////////////////////////////////////////////////AAAAAAAAAAAAAAAAMgAXAMABBgD//////////wAABgD///////////////8AAP///////wYAAAAAAP//AAAAAKQAAABVAQAADgAAAFQBAAANAAAASwEAAP////8AAAAAAAAAAAAAAAAyABcA//8CAP//////////BgD///////8GAAAA/////wAABgD/////BgAAAP////9RAAAAbgIAAKsFAAAgAAAAbAIAANwFAABLAQAA/////wAAAAAAAAAAAAAAAA==");

    private static readonly byte[] DlcTemplate = Convert.FromBase64String(
        "MgAXAMkBBgD//////////wAG//8A////AAb//1EAHgBVAiAAFAAEAEsB//8yABcAuwEGAP//////////AAb//wb///8ABv//HgAvBqEAFgAtBioDSwH//zIAFwCFAAYA/////wYABv8GBv//AP///wYA//8eAFEACAEgACoFBAFLAVwDMgAXAMkBBgD/////AAb//wYA//8F////BQD//1EAgwUgAxQAIAOGAUsB//8yABcAyQEGAP////8ABv//AAb//wD///8ABv//HgAFBiADpACwBAQGSwH//zIAFwD//wIA/////wYABgD/////Bgb//wYG//9VAr8FVAK+BQQBKgVLAf//MgAXAP//AgD/////BgYG/wb/////////Bv///1EAHgVwBR4AbwVuBUsB//8yABcAVgAGAP////8GBAD/BgYA/wb///8G////UQAbAcgFIgEiAQ0ASwH//zIAFwAjAAYA/////wAA//8A/////////wD///+kAPAA8gBUAnIBDQBLAf//MgAXAP//AgD//////////wYA//8A////AAb//w4GVAIQBlUCDgZoAw4G//8=");

    private static readonly byte[] CanonicalRegularTemplate = Inflate(
        "7ZjPb01BFMc/c388P1osJLqrh41IyEtTiYVgLxGqtZCqRhoWpEQtSohEIxEiwYJGEKILYWVBQkQsLNiIP0AsLNiQYGH75HRmzL1z584rO0mni3Nnet+3337POd85eX308Fg1aJvVgNlne4IXZ4AxoB9omuclwHal36Cw+jxsu0CfNf5E99lJ4CSwFOiaxZgbdh3vIrbgDiaat6zxCPY66rHd/6HjCDAMjBpN9hruddjdHra85XjrJxtF7yngHLACuNEBu6iJYJSx/Qg7Eq2J6L05mstWibfl5+9tvA5MA8uAPYnGtbwXFIGBXlqcL2lSxrY7e34fGDK8RZN7BWx59jVZ7vHWP+FcCm5/oU5eZ/AjoveaCG///H0G7zL4ZXL5vUMup9XcNZE6Ed4fTQ0W9a7jbXWox9ZReP4ENqadeXcF+lLXoI4hvTcoXSeCvV7Feb9RwraMVdXb8d5lakWwmymsjWA/qPRO9a9YL3iR6RoR3tI7z7O4D14o1Jycad39LtLrgILjCkaVxt6vYESFsRfTw92ID/rnCxM4LZ8zPS97eX5CuOf/xmNFa9FkZ8AH5czH3kRnrypizxi/ano9LzntlMuYVwnmlKkTwT6YxnN5q9TzNtq7rLy/quCKgmtK16Bgy36wBvtYELtch3Zn9bb1fbND77wK3g26yt2doJlbPxlS7r6cUfXY43Pybx3vJLA7gQFTg7KXOnTc3FpJi7dBPylrYXmvSiExvd40MTOaLArUSVFvq4nf9Vb/gRwuJ3BJ+OdwUWrdxDqPDXd4dUkurc+KJgNpPJcvA5r4y55tS7VPbTX+vSWKXe35Or3t7CP9sy91PT+camyJPvbt6Azh8i+/sd4tvSnY3YWer+ayxYnKXOWU8XkLdq+5cyaArxl8y+r1Phusk7CvTOa6L0/lmrfsY35yxusdvYrIDw8f4tPs2VgOvbnzKsGVsxB2u91Du/0U+JLD0RyeKficw+pG7P6sW1Xsf0GZx57XZL5O/sfe6QvMEOV3dXQzhZ7ZZIaQO01mnpgPtttJ7ezjvjNw8+B4Ao+Mxx5J4EOND/4G");

    public static int Initialize(byte[] data, IEnumerable<int> characterSlots)
    {
        Validate(data, RegularOffset, RegularStride * 7 + RegularCount * RegularSize);
        Validate(data, DlcOffset, DlcStride * 7 + DlcCount * DlcSize);
        int initialized = 0;
        foreach (int slot in characterSlots.Distinct())
        {
            if (slot is < 0 or >= 8) throw new ArgumentOutOfRangeException(nameof(characterSlots));
            foreach (int position in RegularPartnerPositions)
                initialized += InitializeRecord(data, slot, position, RegularOffset, RegularStride, RegularSize, CanonicalRegularTemplate);
            for (int position = 0; position < DlcCount; position++)
                initialized += InitializeRecord(data, slot, position, DlcOffset, DlcStride, DlcSize, DlcTemplate);
        }
        return initialized;
    }

    public static int UnlockAllOptions(byte[] data)
    {
        int changed = 0;
        changed += FillFlags(data, RegularFlagsOffset, 28 * RegularCount);
        changed += FillFlags(data, DlcFlagsOffset, 28 * DlcCount);
        changed += FillFlags(data, FestivalFlagsOffset, 28 * 54);
        return changed;
    }

    private static int InitializeRecord(byte[] data, int targetSlot, int position, int baseOffset, int stride, int size, byte[] fallback)
    {
        int target = baseOffset + targetSlot * stride + position * size;
        if (IsMaterialized(data, target)) return 0;
        int source = -1;
        for (int slot = 0; slot < 8; slot++)
        {
            int candidate = baseOffset + slot * stride + position * size;
            if (IsMaterialized(data, candidate)) { source = candidate; break; }
        }
        if (source >= 0) Buffer.BlockCopy(data, source, data, target, size);
        else Buffer.BlockCopy(fallback, position * size, data, target, size);
        // A newly initialized record starts at the neutral/default stat type.
        data[target + 2] = 0;
        data[target + 3] = 0;
        return 1;
    }

    private static bool IsMaterialized(byte[] data, int offset)
    {
        ushort marker = BitConverter.ToUInt16(data, offset);
        return marker is not 0 and not ushort.MaxValue;
    }

    private static int FillFlags(byte[] data, int offset, int length)
    {
        Validate(data, offset, length);
        int changed = 0;
        for (int i = 0; i < length; i++)
        {
            if (data[offset + i] != byte.MaxValue) changed++;
            data[offset + i] = byte.MaxValue;
        }
        return changed;
    }

    private static byte[] Inflate(string value)
    {
        using MemoryStream input = new(Convert.FromBase64String(value));
        using System.IO.Compression.DeflateStream inflater = new(input, System.IO.Compression.CompressionMode.Decompress);
        using MemoryStream output = new();
        inflater.CopyTo(output);
        byte[] result = output.ToArray();
        if (result.Length != RegularCount * RegularSize)
            throw new InvalidDataException("The verified regular mentor template has an invalid length.");
        return result;
    }

    private static void Validate(byte[] data, int offset, int length)
    {
        if (offset < 0 || offset + length > data.Length)
            throw new InvalidOperationException("The verified mentor customization structure is outside this save revision.");
    }
}
