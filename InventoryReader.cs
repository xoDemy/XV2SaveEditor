using System;
using System.Collections.Generic;

namespace XV2SaveEditor
{
    public static class InventoryReader
    {
        // =========================================================
        // INVENTORY LAYOUT
        // =========================================================

        public const int EntrySize =
            8;


        public const int EntriesPerSection =
            512;


        public const int SectionSize =
            EntrySize *
            EntriesPerSection;


        // =========================================================
        // INVENTORY OFFSETS
        // =========================================================

        public const int TopsOffset =
            5136;


        public const int BottomsOffset =
            9232;


        public const int GlovesOffset =
            13328;


        public const int ShoesOffset =
            17424;


        public const int AccessoriesOffset =
            21520;


        public const int SuperSoulsOffset =
            25616;


        public const int MixItemsOffset =
            29712;


        public const int ImportantItemsOffset =
            33808;


        public const int CapsulesOffset =
            37904;


        // =========================================================
        // READ COMPLETE INVENTORY
        // =========================================================

        public static XV2Inventory Read(
            byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(
                    nameof(data)
                );
            }


            ValidateSection(
                data,
                TopsOffset,
                "Tops"
            );


            ValidateSection(
                data,
                BottomsOffset,
                "Bottoms"
            );


            ValidateSection(
                data,
                GlovesOffset,
                "Gloves"
            );


            ValidateSection(
                data,
                ShoesOffset,
                "Shoes"
            );


            ValidateSection(
                data,
                AccessoriesOffset,
                "Accessories"
            );


            ValidateSection(
                data,
                SuperSoulsOffset,
                "Super Souls"
            );


            ValidateSection(
                data,
                MixItemsOffset,
                "Mix Items"
            );


            ValidateSection(
                data,
                ImportantItemsOffset,
                "Important Items"
            );


            ValidateSection(
                data,
                CapsulesOffset,
                "Capsules"
            );


            return new XV2Inventory
            {
                Tops =
                    ReadSection(
                        data,
                        TopsOffset
                    ),

                Bottoms =
                    ReadSection(
                        data,
                        BottomsOffset
                    ),

                Gloves =
                    ReadSection(
                        data,
                        GlovesOffset
                    ),

                Shoes =
                    ReadSection(
                        data,
                        ShoesOffset
                    ),

                Accessories =
                    ReadSection(
                        data,
                        AccessoriesOffset
                    ),

                SuperSouls =
                    ReadSection(
                        data,
                        SuperSoulsOffset
                    ),

                MixItems =
                    ReadSection(
                        data,
                        MixItemsOffset
                    ),

                ImportantItems =
                    ReadSection(
                        data,
                        ImportantItemsOffset
                    ),

                Capsules =
                    ReadSection(
                        data,
                        CapsulesOffset
                    )
            };
        }


        // =========================================================
        // READ ONE CATEGORY
        // =========================================================

        private static List<XV2InventoryItem>
            ReadSection(
                byte[] data,
                int sectionOffset)
        {
            List<XV2InventoryItem> items =
                new List<XV2InventoryItem>();


            for (
                int slot = 0;
                slot < EntriesPerSection;
                slot++)
            {
                int offset =
                    sectionOffset +
                    (slot * EntrySize);


                int id =
                    BitConverter.ToInt32(
                        data,
                        offset
                    );


                byte type =
                    data[
                        offset + 4
                    ];


                // Empty inventory entry.
                if (
                    id == -1 ||
                    type == 255)
                {
                    continue;
                }


                XV2InventoryItem item =
                    new XV2InventoryItem
                    {
                        SlotIndex =
                            slot,

                        ID =
                            id,

                        Type =
                            type,

                        Quantity =
                            data[
                                offset + 5
                            ],

                        I_06 =
                            data[
                                offset + 6
                            ],

                        I_07 =
                            data[
                                offset + 7
                            ]
                    };


                items.Add(
                    item
                );
            }


            return items;
        }


        // =========================================================
        // VALIDATE SECTION
        // =========================================================

        private static void ValidateSection(
            byte[] data,
            int offset,
            string sectionName)
        {
            if (
                offset < 0 ||
                offset + SectionSize >
                data.Length)
            {
                throw new InvalidOperationException(
                    $"The {sectionName} inventory section " +
                    $"is outside the save data.\n\n" +

                    $"Offset: {offset}\n" +
                    $"Required size: {SectionSize}\n" +
                    $"Save size: {data.Length}"
                );
            }
        }
    }
}
