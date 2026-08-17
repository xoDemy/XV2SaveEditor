using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace XV2SaveEditor
{
    public class Xv2MsgEntry
    {
        public string Key { get; set; } = "";

        public int NumericId { get; set; }

        public List<string> Lines { get; set; }
            = new List<string>();


        public string FirstLine
        {
            get
            {
                if (Lines.Count == 0)
                {
                    return "";
                }

                return Lines[0];
            }
        }
    }


    public class Xv2MsgFile
    {
        public List<Xv2MsgEntry> Entries { get; set; }
            = new List<Xv2MsgEntry>();


        // =========================================================
        // GET BY PHYSICAL MSG POSITION
        // =========================================================

        public string GetText(
            int entryIndex)
        {
            if (
                entryIndex < 0 ||
                entryIndex >= Entries.Count)
            {
                return "";
            }


            return Entries[
                entryIndex
            ].FirstLine;
        }


        // =========================================================
        // GET BY MSG KEY
        // =========================================================

        public string GetTextByKey(
            string key)
        {
            foreach (
                Xv2MsgEntry entry
                in Entries)
            {
                if (
                    string.Equals(
                        entry.Key,
                        key,
                        StringComparison.OrdinalIgnoreCase
                    ))
                {
                    return entry.FirstLine;
                }
            }


            return "";
        }


        // =========================================================
        // FIND ENTRY BY KEY
        // =========================================================

        public Xv2MsgEntry? FindByKey(
            string key)
        {
            foreach (
                Xv2MsgEntry entry
                in Entries)
            {
                if (
                    string.Equals(
                        entry.Key,
                        key,
                        StringComparison.OrdinalIgnoreCase
                    ))
                {
                    return entry;
                }
            }


            return null;
        }
    }


    public static class Xv2MsgReader
    {
        public static Xv2MsgFile Load(
            string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    "MSG file was not found.",
                    path
                );
            }


            return Load(
                File.ReadAllBytes(path)
            );
        }


        public static Xv2MsgFile Load(
            byte[] data)
        {
            if (data.Length < 32)
            {
                throw new InvalidDataException(
                    "MSG file is too small."
                );
            }


            string signature =
                Encoding.ASCII.GetString(
                    data,
                    0,
                    4
                );


            if (signature != "#MSG")
            {
                throw new InvalidDataException(
                    "Invalid MSG signature."
                );
            }


            short namesEncodingFlag =
                BitConverter.ToInt16(
                    data,
                    4
                );


            short messageEncodingFlag =
                BitConverter.ToInt16(
                    data,
                    6
                );


            int entryCount =
                BitConverter.ToInt32(
                    data,
                    8
                );


            int nameTableOffset =
                BitConverter.ToInt32(
                    data,
                    0x0C
                );


            int numericIdTableOffset =
                BitConverter.ToInt32(
                    data,
                    0x10
                );


            int messageTableOffset =
                BitConverter.ToInt32(
                    data,
                    0x14
                );


            if (entryCount < 0)
            {
                throw new InvalidDataException(
                    "Invalid MSG entry count."
                );
            }


            Xv2MsgFile file =
                new Xv2MsgFile();


            for (
                int i = 0;
                i < entryCount;
                i++)
            {
                // =================================================
                // KEY
                // =================================================

                int nameDescriptor =
                    nameTableOffset +
                    (i * 16);


                EnsureRange(
                    data,
                    nameDescriptor,
                    16
                );


                int nameOffset =
                    BitConverter.ToInt32(
                        data,
                        nameDescriptor + 0x00
                    );


                int nameAsciiLength =
                    BitConverter.ToInt32(
                        data,
                        nameDescriptor + 0x04
                    );


                int nameUtf16Length =
                    BitConverter.ToInt32(
                        data,
                        nameDescriptor + 0x08
                    );


                string key =
                    ReadEncodedString(
                        data,

                        nameOffset,

                        namesEncodingFlag ==
                        0x0100,

                        nameAsciiLength,

                        nameUtf16Length
                    );


                // =================================================
                // NUMERIC ID
                // =================================================

                int numericOffset =
                    numericIdTableOffset +
                    (i * 4);


                EnsureRange(
                    data,
                    numericOffset,
                    4
                );


                int numericId =
                    BitConverter.ToInt32(
                        data,
                        numericOffset
                    );


                // =================================================
                // MESSAGE DESCRIPTOR
                // =================================================

                int messageDescriptor =
                    messageTableOffset +
                    (i * 8);


                EnsureRange(
                    data,
                    messageDescriptor,
                    8
                );


                int lineCount =
                    BitConverter.ToInt32(
                        data,
                        messageDescriptor + 0
                    );


                int lineTableOffset =
                    BitConverter.ToInt32(
                        data,
                        messageDescriptor + 4
                    );


                Xv2MsgEntry entry =
                    new Xv2MsgEntry
                    {
                        Key =
                            key,

                        NumericId =
                            numericId
                    };


                // =================================================
                // MESSAGE LINES
                // =================================================

                for (
                    int line = 0;
                    line < lineCount;
                    line++)
                {
                    int lineDescriptor =
                        lineTableOffset +
                        (line * 16);


                    EnsureRange(
                        data,
                        lineDescriptor,
                        16
                    );


                    int textOffset =
                        BitConverter.ToInt32(
                            data,
                            lineDescriptor + 0x00
                        );


                    int asciiLength =
                        BitConverter.ToInt32(
                            data,
                            lineDescriptor + 0x04
                        );


                    int utf16Length =
                        BitConverter.ToInt32(
                            data,
                            lineDescriptor + 0x08
                        );


                    string text =
                        ReadEncodedString(
                            data,

                            textOffset,

                            messageEncodingFlag == 1,

                            asciiLength,

                            utf16Length
                        );


                    text =
                        WebUtility.HtmlDecode(
                            text
                        );


                    entry.Lines.Add(
                        text
                    );
                }


                file.Entries.Add(
                    entry
                );
            }


            return file;
        }


        // =========================================================
        // STRING READER
        // =========================================================

        private static string ReadEncodedString(
            byte[] data,
            int offset,
            bool unicode,
            int asciiLength,
            int utf16Length)
        {
            int length =
                unicode
                    ? utf16Length
                    : asciiLength;


            if (length <= 0)
            {
                return "";
            }


            EnsureRange(
                data,
                offset,
                length
            );


            string value;


            if (unicode)
            {
                value =
                    Encoding.Unicode.GetString(
                        data,
                        offset,
                        length
                    );
            }
            else
            {
                value =
                    Encoding.UTF8.GetString(
                        data,
                        offset,
                        length
                    );
            }


            return value.TrimEnd(
                '\0'
            );
        }


        // =========================================================
        // RANGE CHECK
        // =========================================================

        private static void EnsureRange(
            byte[] data,
            int offset,
            int length)
        {
            if (
                offset < 0 ||
                length < 0 ||
                offset + length >
                data.Length)
            {
                throw new InvalidDataException(
                    "MSG structure points outside the file."
                );
            }
        }
    }
}