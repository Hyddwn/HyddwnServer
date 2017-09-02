// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Aura.Data
{
    public class CsvReader : IDisposable
    {
        public CsvReader(string path)
        {
            Path = path;
            Seperator = ',';
        }

        public string Path { get; set; }
        public char Seperator { get; set; }

        public void Dispose()
        {
        }

        public IEnumerable<CsvEntry> Next()
        {
            using (var fs = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new StreamReader(fs))
            {
                for (var i = 0; !reader.EndOfStream; ++i)
                {
                    var entry = GetEntry(reader.ReadLine());
                    if (entry.Count < 1)
                        continue;

                    entry.Line = i + 1;

                    yield return entry;
                }
            }
        }

        protected CsvEntry GetEntry(string csv)
        {
            var result = new CsvEntry();

            csv = csv.Trim();

            // Check for empty and commented lines.
            if (csv != string.Empty && !csv.StartsWith("//"))
            {
                int ptr = 0, braces = 0;
                bool inString = false, comment = false;
                for (var i = 0; i < csv.Length; ++i)
                {
                    // End of line?
                    var eol = i == csv.Length - 1;

                    // Quotes
                    if (csv[i] == '"' && braces == 0)
                        inString = !inString;

                    // Braces
                    if (!inString)
                        if (csv[i] == '{')
                            braces++;
                        else if (csv[i] == '}')
                            braces--;

                    // Comments
                    if (!inString && csv[i] == '/' && csv[i + 1] == '/')
                        comment = true;

                    if (csv[i] == Seperator && braces == 0 && !inString || eol || comment)
                    {
                        // Inc by one to get the last char
                        if (eol) i++;

                        // Get value
                        var v = csv.Substring(ptr, i - ptr).Trim(' ', '\t', '"');

                        // Trim surrounding braces
                        if (v.Length >= 2 && v[0] == '{' && v[v.Length - 1] == '}')
                            v = v.Substring(1, v.Length - 2);

                        result.Fields.Add(v);

                        // Skip over seperator
                        ptr = i + 1;

                        // Stop at comments
                        if (comment)
                            break;
                    }
                }
            }

            return result;
        }
    }

    public class CsvEntry
    {
        public CsvEntry()
        {
            Fields = new List<string>();
        }

        public CsvEntry(List<string> fields)
        {
            Fields = fields;
        }

        public List<string> Fields { get; set; }
        public int Pointer { get; set; }
        public int Line { get; set; }

        public int Count => Fields.Count;

        public bool End => Pointer > Fields.Count - 1;

        public int Remaining => Fields.Count - Pointer - 1;

        public void Skip(int fields)
        {
            Pointer += fields;
            if (Pointer >= Fields.Count)
                Pointer = 0;
        }

        public bool IsFieldEmpty(int index = -1)
        {
            return string.IsNullOrWhiteSpace(Fields[index < 0 ? Pointer : index]);
        }

        public bool ReadBool(int index = -1)
        {
            if (IsFieldEmpty(index))
            {
                Pointer++;
                return false;
            }

            var val = Fields[index < 0 ? Pointer++ : index];
            return val == "1" || val == "true" || val == "yes";
        }

        public byte ReadByte(int index = -1)
        {
            if (IsFieldEmpty(index))
            {
                Pointer++;
                return 0;
            }
            return Convert.ToByte(Fields[index < 0 ? Pointer++ : index]);
        }

        public sbyte ReadSByte(int index = -1)
        {
            if (IsFieldEmpty(index))
            {
                Pointer++;
                return 0;
            }
            return Convert.ToSByte(Fields[index < 0 ? Pointer++ : index]);
        }

        public byte ReadByteHex(int index = -1)
        {
            if (IsFieldEmpty(index))
            {
                Pointer++;
                return 0;
            }
            return Convert.ToByte(Fields[index < 0 ? Pointer++ : index], 16);
        }

        public sbyte ReadSByteHex(int index = -1)
        {
            if (IsFieldEmpty(index))
            {
                Pointer++;
                return 0;
            }
            return Convert.ToSByte(Fields[index < 0 ? Pointer++ : index], 16);
        }

        public short ReadShort(int index = -1)
        {
            if (IsFieldEmpty(index))
            {
                Pointer++;
                return 0;
            }
            return Convert.ToInt16(Fields[index < 0 ? Pointer++ : index]);
        }

        public ushort ReadUShort(int index = -1)
        {
            if (IsFieldEmpty(index))
            {
                Pointer++;
                return 0;
            }
            return Convert.ToUInt16(Fields[index < 0 ? Pointer++ : index]);
        }

        public short ReadShortHex(int index = -1)
        {
            if (IsFieldEmpty(index))
            {
                Pointer++;
                return 0;
            }
            return Convert.ToInt16(Fields[index < 0 ? Pointer++ : index], 16);
        }

        public ushort ReadUShortHex(int index = -1)
        {
            if (IsFieldEmpty(index))
            {
                Pointer++;
                return 0;
            }
            return Convert.ToUInt16(Fields[index < 0 ? Pointer++ : index], 16);
        }

        public int ReadInt(int index = -1)
        {
            if (IsFieldEmpty(index))
            {
                Pointer++;
                return 0;
            }
            return Convert.ToInt32(Fields[index < 0 ? Pointer++ : index]);
        }

        public uint ReadUInt(int index = -1)
        {
            if (IsFieldEmpty(index))
            {
                Pointer++;
                return 0;
            }
            return Convert.ToUInt32(Fields[index < 0 ? Pointer++ : index]);
        }

        public int ReadIntHex(int index = -1)
        {
            if (IsFieldEmpty(index))
            {
                Pointer++;
                return 0;
            }
            return Convert.ToInt32(Fields[index < 0 ? Pointer++ : index], 16);
        }

        public uint ReadUIntHex(int index = -1)
        {
            if (IsFieldEmpty(index))
            {
                Pointer++;
                return 0;
            }
            return Convert.ToUInt32(Fields[index < 0 ? Pointer++ : index], 16);
        }

        public long ReadLong(int index = -1)
        {
            if (IsFieldEmpty(index))
            {
                Pointer++;
                return 0;
            }
            return Convert.ToInt64(Fields[index < 0 ? Pointer++ : index]);
        }

        public ulong ReadULong(int index = -1)
        {
            if (IsFieldEmpty(index))
            {
                Pointer++;
                return 0;
            }
            return Convert.ToUInt64(Fields[index < 0 ? Pointer++ : index]);
        }

        public long ReadLongHex(int index = -1)
        {
            if (IsFieldEmpty(index))
            {
                Pointer++;
                return 0;
            }
            return Convert.ToInt64(Fields[index < 0 ? Pointer++ : index], 16);
        }

        public ulong ReadULongHex(int index = -1)
        {
            if (IsFieldEmpty(index))
            {
                Pointer++;
                return 0;
            }
            return Convert.ToUInt64(Fields[index < 0 ? Pointer++ : index], 16);
        }

        public float ReadFloat(int index = -1)
        {
            if (IsFieldEmpty(index))
            {
                Pointer++;
                return 0;
            }
            return float.Parse(Fields[index < 0 ? Pointer++ : index], NumberStyles.Any,
                CultureInfo.GetCultureInfo("en-US"));
        }

        public double ReadDouble(int index = -1)
        {
            if (IsFieldEmpty(index))
            {
                Pointer++;
                return 0;
            }
            return double.Parse(Fields[index < 0 ? Pointer++ : index], NumberStyles.Any,
                CultureInfo.GetCultureInfo("en-US"));
        }

        public string ReadString(int index = -1)
        {
            return Fields[index < 0 ? Pointer++ : index];
        }

        public string[] ReadStringList(int index = -1)
        {
            return ReadString(index).Split(':');
        }
    }
}