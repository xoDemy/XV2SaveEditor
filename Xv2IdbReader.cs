using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XV2SaveEditor
{
    public enum Xv2IdbVersion
    {
        Old,
        V1,
        V2
    }


    public class Xv2IdbEntry
    {
        public ushort ID { get; set; }

        public ushort NameMsgID { get; set; }

        public ushort DescMsgID { get; set; }

        public ushort Type { get; set; }

        public int RaceLock { get; set; }
    }


    public class Xv2IdbFile
    {
        public Xv2IdbVersion Version { get; set; }

        public int RecordSize { get; set; }

        public List<Xv2IdbEntry> Entries { get; set; }
            = new List<Xv2IdbEntry>();
    }


    public static class Xv2IdbReader
    {
        private const int OldRecordSize =
            720;

        private const int V1RecordSize =
            748;

        private const int V2RecordSize =
            772;


        public static Xv2IdbFile Load(
            string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    "IDB file was not found.",
                    path
                );
            }

            return Load(
                File.ReadAllBytes(path)
            );
        }


        public static Xv2IdbFile Load(
            byte[] data)
        {
            if (data.Length < 16)
            {
                throw new InvalidDataException(
                    "IDB file is too small."
                );
            }


            string signature =
                Encoding.ASCII.GetString(
                    data,
                    0,
                    4
                );


            if (signature != "#IDB")
            {
                throw new InvalidDataException(
                    "Invalid IDB signature."
                );
            }


            int entryCount =
                BitConverter.ToInt32(
                    data,
                    8
                );


            int tableOffset =
                BitConverter.ToInt32(
                    data,
                    12
                );


            if (entryCount < 0)
            {
                throw new InvalidDataException(
                    "Invalid IDB entry count."
                );
            }


            if (
                tableOffset < 16 ||
                tableOffset > data.Length)
            {
                throw new InvalidDataException(
                    "Invalid IDB entry table offset."
                );
            }


            Xv2IdbFile file =
                new Xv2IdbFile();


            if (entryCount == 0)
            {
                return file;
            }


            // The original XV2 parser determines the IDB
            // version from the total file size.
            int payloadSize =
                data.Length - 16;


            if (
                payloadSize ==
                entryCount * OldRecordSize)
            {
                file.Version =
                    Xv2IdbVersion.Old;

                file.RecordSize =
                    OldRecordSize;
            }

            else if (
                payloadSize ==
                entryCount * V1RecordSize)
            {
                file.Version =
                    Xv2IdbVersion.V1;

                file.RecordSize =
                    V1RecordSize;
            }

            else if (
                payloadSize ==
                entryCount * V2RecordSize)
            {
                file.Version =
                    Xv2IdbVersion.V2;

                file.RecordSize =
                    V2RecordSize;
            }

            else
            {
                throw new InvalidDataException(
                    "Unknown IDB record format.\n\n" +
                    $"Entries: {entryCount}\n" +
                    $"Payload Size: {payloadSize}"
                );
            }


            int typeOffset;
            int raceOffset;


            switch (file.Version)
            {
                case Xv2IdbVersion.Old:

                    typeOffset = 0x08;
                    raceOffset = 0x18;

                    break;


                case Xv2IdbVersion.V1:

                    typeOffset = 0x08;
                    raceOffset = 0x1C;

                    break;


                case Xv2IdbVersion.V2:

                    typeOffset = 0x0C;
                    raceOffset = 0x20;

                    break;


                default:

                    throw new InvalidDataException(
                        "Unsupported IDB version."
                    );
            }


            for (
                int i = 0;
                i < entryCount;
                i++)
            {
                int offset =
                    tableOffset +
                    (i * file.RecordSize);


                if (
                    offset + file.RecordSize >
                    data.Length)
                {
                    throw new InvalidDataException(
                        "IDB entry extends beyond the file."
                    );
                }


                Xv2IdbEntry entry =
                    new Xv2IdbEntry
                    {
                        ID =
                            BitConverter.ToUInt16(
                                data,
                                offset + 0x00
                            ),

                        NameMsgID =
                            BitConverter.ToUInt16(
                                data,
                                offset + 0x04
                            ),

                        DescMsgID =
                            BitConverter.ToUInt16(
                                data,
                                offset + 0x06
                            ),

                        Type =
                            BitConverter.ToUInt16(
                                data,
                                offset + typeOffset
                            ),

                        RaceLock =
                            BitConverter.ToInt32(
                                data,
                                offset + raceOffset
                            )
                    };


                file.Entries.Add(
                    entry
                );
            }


            return file;
        }
    }
}