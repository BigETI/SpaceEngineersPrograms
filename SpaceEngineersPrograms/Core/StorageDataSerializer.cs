using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceEngineersPrograms
{
    public class StorageDataSerializer
    {
        private List<byte> data = new List<byte>();

        private uint position;

        private string storageString;

        public uint Position
        {
            get
            {
                return position;
            }
            set
            {
                position = Math.Min(value, (uint)(data.Count));
            }
        }

        public string StorageString
        {
            get
            {
                if (storageString == null)
                {
                    char[] storage_string_characters = new char[data.Count * 2];
                    for (int i = 0; i < data.Count; i++)
                    {
                        storage_string_characters[i * 2] = ByteToHexChar((byte)(data[i] & 0xF));
                        storage_string_characters[(i * 2) + 1] = ByteToHexChar((byte)((data[i] & 0xF0) >> 4));
                    }
                    storageString = new string(storage_string_characters);
                }
                return storageString;
            }
        }

        public StorageDataSerializer()
        {
            // ...
        }

        public StorageDataSerializer(string storageString)
        {
            if (storageString == null)
            {
                throw new ArgumentNullException(nameof(storageString));
            }
            if (!ValidateHexString(storageString))
            {
                throw new ArgumentException("Storage string is not valid", nameof(storageString));
            }
            this.storageString = storageString;
            for (int i = 0, len = storageString.Length / 2; i < len; i++)
            {
                data.Add((byte)(HexCharToByte(storageString[i * 2]) | (HexCharToByte(storageString[(i * 2) + 1]) << 4)));
            }
        }

        public StorageDataSerializer(IEnumerable<byte> data)
        {
            WriteBytes(data);
        }

        public static bool ValidateHexString(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            bool ret = false;
            if ((input.Length % 2) == 0)
            {
                ret = true;
                foreach (char storage_string_character in input)
                {
                    if (((storage_string_character < '0') || (storage_string_character > '9')) && ((storage_string_character < 'a') || (storage_string_character > 'f')) && ((storage_string_character < 'A') || (storage_string_character > 'F')))
                    {
                        ret = false;
                        break;
                    }
                }
            }
            return ret;
        }

        private static byte HexCharToByte(char hexCharacter)
        {
            byte ret;
            if ((hexCharacter >= 'A') && (hexCharacter <= 'F'))
            {
                ret = (byte)((hexCharacter - 'A') + 0xA);
            }
            else if ((hexCharacter >= 'a') && (hexCharacter <= 'f'))
            {
                ret = (byte)((hexCharacter - 'a') + 0xA);
            }
            else
            {
                ret = (byte)(hexCharacter - '0');
            }
            return ret;
        }

        private static char ByteToHexChar(byte b) => (char)(((b >= 0x0) && (b <= 0x9)) ? ('0' + b) : ('A' + (b - 0xA)));

        public void ReadBytes(int length, byte[] result)
        {
            if (length < 0)
            {
                throw new ArgumentException("Argument is smaller than 0.", nameof(length));
            }
            if (length > (data.Count - position))
            {
                throw new ArgumentException("Can't read more than " + (data.Count - position) + " bytes.", nameof(length));
            }
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }
            if (result.Length < length)
            {
                throw new ArgumentException("Insufficient capacity of result array", nameof(result));
            }
            for (int i = 0; i < length; i++)
            {
                result[i] = data[(int)position + i];
            }
            position += (uint)length;
        }

        public byte[] ReadBytes(int length)
        {
            byte[] ret = new byte[length];
            ReadBytes(length, ret);
            return ret;
        }

        public byte ReadByte()
        {
            if (position >= data.Count)
            {
                throw new InvalidOperationException("Position exceeded data size");
            }
            return data[(int)position++];
        }

        public sbyte ReadSByte() => (sbyte)(ReadByte());

        public ushort ReadUInt16()
        {
            byte low = ReadByte();
            byte high = ReadByte();
            return (ushort)(low | (high << 8));
        }

        public short ReadInt16() => (short)(ReadUInt16());

        public uint ReadUInt32()
        {
            ushort low = ReadUInt16();
            ushort high = ReadUInt16();
            return (low | ((uint)high << 16));
        }

        public int ReadInt32() => (int)(ReadUInt32());

        public ulong ReadUInt64()
        {
            uint low = ReadUInt32();
            uint high = ReadUInt32();
            return (low | ((ulong)high << 32));
        }

        public long ReadInt64() => (long)(ReadUInt64());

        public string ReadString()
        {
            int string_size = ReadInt32();
            return Encoding.UTF8.GetString(ReadBytes(string_size));
        }

        public void WriteBytes(IEnumerable<byte> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            this.data.AddRange(data);
            storageString = null;
        }

        public void WriteByte(byte data)
        {
            this.data.Add(data);
            storageString = null;
        }

        public void WriteUInt16(ushort data)
        {
            WriteByte((byte)(data & 0xFF));
            WriteByte((byte)((data & 0xFF00) >> 8));
        }

        public void WriteInt16(short data) => WriteUInt16((ushort)data);

        public void WriteUInt32(uint data)
        {
            WriteUInt16((ushort)(data & 0xFFFF));
            WriteUInt16((ushort)((data & 0xFFFF0000) >> 16));
        }

        public void WriteInt32(int data) => WriteUInt32((uint)data);

        public void WriteUInt64(ulong data)
        {
            WriteUInt32((uint)(data & 0xFFFFFFFF));
            WriteUInt32((uint)((data & 0xFFFFFFFF00000000) >> 32));
        }

        public void WriteInt64(long data) => WriteUInt64((ulong)data);

        public void WriteString(string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            WriteInt32(bytes.Length);
            WriteBytes(bytes);
        }

        public void Clear()
        {
            data.Clear();
            position = 0U;
        }
    }
}
