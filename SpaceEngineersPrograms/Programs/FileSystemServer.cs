using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceEngineersPrograms
{
    public class FileSystemServer : MyGridProgram
    {
        private enum StorageReaderSeekOrigin
        {
            Begin,

            Current,

            End
        }

        private class StorageReaderArgumentException : Exception
        {
            public StorageReaderArgumentException(int position, int length) : base("Can not read " + length + " bytes from position " + position + ".")
            {
                // ...
            }

            public StorageReaderArgumentException(string message) : base(message)
            {
                // ...
            }
        }

        private class StorageReader
        {
            private byte[] data = Array.Empty<byte>();

            private int position;

            public int Position
            {
                get
                {
                    return position;
                }
                set
                {
                    position = ((value < 0) ? 0 : ((value > data.Length) ? data.Length : value));
                }
            }

            public int Length => data.Length;

            public bool IsAtEnd => (position >= data.Length);

            public string Base64 => Convert.ToBase64String(data);

            public StorageReader()
            {
                // ...
            }

            public StorageReader(StorageReader storage)
            {
                if (storage != null)
                {
                    data = (byte[])(storage.data.Clone());
                }
            }

            public StorageReader(byte[] byteStorage)
            {
                if (byteStorage != null)
                {
                    data = (byte[])(byteStorage.Clone());
                }
            }

            public StorageReader(string base64Storage)
            {
                if ((base64Storage != null) && (base64Storage.Length > 0))
                {
                    data = Convert.FromBase64String(base64Storage);
                }
            }

            public void Seek(int position, StorageReaderSeekOrigin seekOrigin)
            {
                switch (seekOrigin)
                {
                    case StorageReaderSeekOrigin.Begin:
                        Position = position;
                        break;
                    case StorageReaderSeekOrigin.Current:
                        Position += position;
                        break;
                    case StorageReaderSeekOrigin.End:
                        Position = data.Length - position;
                        break;
                }
            }

            public bool CanRead(int length) => ((length > 0) ? ((position + data.Length) <= data.Length) : false);

            private bool Validate(int length, bool throwWhenInvalid)
            {
                bool ret = CanRead(length);
                if (throwWhenInvalid && (!ret))
                {
                    throw new StorageReaderArgumentException(position, length);
                }
                return ret;
            }

            public byte[] ReadBytes(int length, bool throwWhenInvalid = true)
            {
                byte[] ret = Array.Empty<byte>();
                if (Validate(length, throwWhenInvalid))
                {
                    ret = new byte[length];
                    Array.Copy(data, position, ret, 0, ret.Length);
                }
                return ret;
            }

            public byte ReadByte(byte? defaultValue = null)
            {
                byte ret = ((defaultValue == null) ? (byte)0 : defaultValue.Value);
                if (Validate(sizeof(byte), (defaultValue == null)))
                {
                    ret = data[position];
                    Position += sizeof(byte);
                }
                return ret;
            }

            public sbyte ReadSByte(sbyte? defaultValue = null)
            {
                sbyte ret = ((defaultValue == null) ? (sbyte)0 : defaultValue.Value);
                if (Validate(sizeof(sbyte), (defaultValue == null)))
                {
                    ret = (sbyte)(data[position]);
                    Position += sizeof(byte);
                }
                return ret;
            }

            public short ReadInt16(short? defaultValue = null)
            {
                short ret = ((defaultValue == null) ? (short)0 : defaultValue.Value);
                if (Validate(sizeof(short), (defaultValue == null)))
                {
                    ret = BitConverter.ToInt16(data, position);
                    Position += sizeof(short);
                }
                return ret;
            }

            public ushort ReadUInt16(ushort? defaultValue = null)
            {
                ushort ret = ((defaultValue == null) ? (ushort)0 : defaultValue.Value);
                if (Validate(sizeof(ushort), (defaultValue == null)))
                {
                    ret = BitConverter.ToUInt16(data, position);
                    Position += sizeof(ushort);
                }
                return ret;
            }

            public int ReadInt32(int? defaultValue = null)
            {
                int ret = ((defaultValue == null) ? 0 : defaultValue.Value);
                if (Validate(sizeof(int), (defaultValue == null)))
                {
                    ret = BitConverter.ToInt32(data, position);
                    Position += sizeof(int);
                }
                return ret;
            }

            public uint ReadUInt32(uint? defaultValue = null)
            {
                uint ret = ((defaultValue == null) ? 0U : defaultValue.Value);
                if (Validate(sizeof(uint), (defaultValue == null)))
                {
                    ret = BitConverter.ToUInt32(data, position);
                    Position += sizeof(uint);
                }
                return ret;
            }

            public long ReadInt64(long? defaultValue = null)
            {
                long ret = ((defaultValue == null) ? 0L : defaultValue.Value);
                if (Validate(sizeof(long), (defaultValue == null)))
                {
                    ret = BitConverter.ToInt64(data, position);
                    Position += sizeof(long);
                }
                return ret;
            }

            public ulong ReadUInt64(ulong? defaultValue = null)
            {
                ulong ret = ((defaultValue == null) ? 0UL : defaultValue.Value);
                if (Validate(sizeof(ulong), (defaultValue == null)))
                {
                    ret = BitConverter.ToUInt64(data, position);
                    Position += sizeof(ulong);
                }
                return ret;
            }

            public float ReadSingle(float? defaultValue = null)
            {
                float ret = ((defaultValue == null) ? 0.0f : defaultValue.Value);
                if (Validate(sizeof(float), (defaultValue == null)))
                {
                    ret = BitConverter.ToSingle(data, position);
                    Position += sizeof(float);
                }
                return ret;
            }

            public double ReadDouble(double? defaultValue = null)
            {
                double ret = ((defaultValue == null) ? 0.0 : defaultValue.Value);
                if (Validate(sizeof(double), (defaultValue == null)))
                {
                    ret = BitConverter.ToDouble(data, position);
                    Position += sizeof(double);
                }
                return ret;
            }

            public byte[] ReadByteSequence(bool throwWhenInvalid = true)
            {
                byte[] ret = Array.Empty<byte>();
                try
                {
                    uint byte_sequence_length = ReadUInt32();
                    ret = ReadBytes((int)byte_sequence_length);
                }
                catch (StorageReaderArgumentException e)
                {
                    if (throwWhenInvalid)
                    {
                        throw e;
                    }
                }
                return ret;
            }

            public string ReadString(Encoding encoding, bool throwWhenInvalid = true)
            {
                if (encoding == null)
                {
                    throw new StorageReaderArgumentException("Encoding can't be null!");
                }
                return encoding.GetString(ReadByteSequence(throwWhenInvalid));
            }

            public string ReadString(bool throwWhenInvalid = true) => ReadString(Encoding.UTF8, throwWhenInvalid);

            public override string ToString() => Base64;
        }

        private class StorageBuilder
        {
            private List<byte> data = new List<byte>();

            public string Base64 => Convert.ToBase64String(data.ToArray());

            public void Add(byte value) => data.Add(value);

            public void Add(sbyte value) => data.Add((byte)value);

            public void Add(IReadOnlyCollection<byte> value)
            {
                if ((value != null) && (value.Count > 0))
                {
                    data.AddRange(value);
                }
            }

            public void Add(short value) => data.AddRange(BitConverter.GetBytes(value));

            public void Add(ushort value) => data.AddRange(BitConverter.GetBytes(value));

            public void Add(int value) => data.AddRange(BitConverter.GetBytes(value));

            public void Add(uint value) => data.AddRange(BitConverter.GetBytes(value));

            public void Add(long value) => data.AddRange(BitConverter.GetBytes(value));

            public void Add(ulong value) => data.AddRange(BitConverter.GetBytes(value));

            public void Add(float value) => data.AddRange(BitConverter.GetBytes(value));

            public void Add(double value) => data.AddRange(BitConverter.GetBytes(value));

            public void AddSequence(IReadOnlyCollection<byte> value)
            {
                if (value != null)
                {
                    Add((uint)(value.Count));
                    Add(value);
                }
            }

            public void Add(string value, Encoding encoding)
            {
                if ((value != null) && (encoding != null))
                {
                    AddSequence(encoding.GetBytes(value));
                }
            }

            public void Add(string value) => Add(value);

            public override string ToString() => Base64;
        }

        [Flags]
        private enum FileFlags
        {
            Nothing = 0x0,

            CanEveryoneRead = 0x1,

            CanEveryoneWrite = 0x2,

            CanEveryoneReadWrite = CanEveryoneRead | CanEveryoneWrite
        }

        private class File
        {
            public string Path { get; private set; } = string.Empty;

            public string Owner { get; private set; } = string.Empty;

            public bool CanEveryoneRead { get; private set; } = false;

            public bool CanEveryoneWrite { get; private set; } = false;

            public List<byte> Data { get; private set; } = new List<byte>();

            public File(string path, string owner, bool canEveryoneRead, bool canEveryoneWrite, byte[] data)
            {
                Path = ((path == null) ? string.Empty : path);
                Owner = ((owner == null) ? string.Empty : owner);
                CanEveryoneRead = canEveryoneRead;
                CanEveryoneWrite = canEveryoneWrite;
                Data.AddRange(data);
            }

            public bool CanList(User user) => ((CanEveryoneRead || CanEveryoneWrite) ? true : ((user == null) ? false : (user.UserName == Owner)));

            public bool CanRead(User user) => (CanEveryoneRead ? true : ((user == null) ? false : (user.UserName == Owner)));

            public bool CanWrite(User user) => (CanEveryoneWrite ? true : ((user == null) ? false : (user.UserName == Owner)));
        }

        private class User
        {
            public string UserName { get; private set; }
            public string Password { get; private set; }

            public User(string userName, string password)
            {
                UserName = ((userName == null) ? string.Empty : userName);
                Password = ((password == null) ? string.Empty : password);
            }
        }

        private struct Session
        {
            public long Address { get; private set; }

            public string UserName { get; private set; }

            public string CurrentDirectory { get; private set; }

            public Session(long address, string userName)
            {
                Address = address;
                UserName = ((userName == null) ? string.Empty : userName);
                CurrentDirectory = "/";
            }
        }

        private delegate void ExecuteCommandDelegate(string[] arguments, string rawArguments, Session session, User user);

        private static readonly string fileSystemServerTag = "fss";

        private static readonly string fileSystemClientTag = "fsc";

        private Dictionary<string, File> files = new Dictionary<string, File>();

        private Dictionary<string, User> users = new Dictionary<string, User>();

        private Dictionary<long, Session> sessions = new Dictionary<long, Session>();

        private IDictionary<string, ExecuteCommandDelegate> Commands { get; set; }

        private IMyBroadcastListener fileSystemListener;

        public FileSystemServer()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            try
            {
                StorageReader storage_data = new StorageReader(Storage);
                if (Storage.Length > 0)
                {
                    uint num_users = storage_data.ReadUInt32();
                    for (uint i = 0U; i != num_users; i++)
                    {
                        string user_name = storage_data.ReadString();
                        string password = storage_data.ReadString();
                        if (!(users.ContainsKey(user_name)))
                        {
                            users.Add(user_name, new User(user_name, password));
                        }
                    }
                    uint num_files = storage_data.ReadUInt32();
                    for (uint i = 0U; i != num_files; i++)
                    {
                        string path = storage_data.ReadString();
                        string owner = storage_data.ReadString();
                        FileFlags read_write_flags = (FileFlags)(storage_data.ReadByte());
                        byte[] data = storage_data.ReadByteSequence();
                        files.Add(path, new File(path, owner, (read_write_flags & FileFlags.CanEveryoneRead) == FileFlags.CanEveryoneRead, (read_write_flags & FileFlags.CanEveryoneWrite) == FileFlags.CanEveryoneWrite, data));
                    }
                }
            }
            catch (StorageReaderArgumentException e)
            {
                Echo(e.ToString());
            }
            fileSystemListener = IGC.RegisterBroadcastListener(fileSystemServerTag);
        }

        private void SendIGCMessage(long address, string message)
        {
            IGC.SendUnicastMessage(address, fileSystemClientTag, message);
        }

        private void SendIGCMessage(long address, string message, string arguments)
        {
            IGC.SendUnicastMessage(address, fileSystemClientTag, message + " " + arguments);
        }

        private void DiscoverCommand(string[] arguments, string rawArguments, Session session, User user)
        {
            if (user == null)
            {
                try
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(Me.CustomName, rawArguments))
                    {
                        SendIGCMessage(session.Address, "available", Me.CustomName);
                    }
                }
                catch
                {
                    // ...
                }
            }
        }

        private void AuthenticateCommand(string[] arguments, string rawArguments, Session session, User user)
        {
            if (user == null)
            {
                if (arguments.Length >= 2)
                {
                    string user_name = arguments[0];
                    string password = arguments[1];
                    if ((user_name.Length > 0) && (users.ContainsKey(user_name)))
                    {
                        User registered_user = users[user_name];
                        if (registered_user.Password == password)
                        {
                            sessions.Add(session.Address, new Session(session.Address, user_name));
                            SendIGCMessage(session.Address, "authenticated", registered_user.UserName);
                        }
                    }
                    else
                    {
                        SendIGCMessage(session.Address, "failed", user_name);
                    }
                }
            }
        }

        private void ExitCommand(string[] arguments, string rawArguments, Session session, User user)
        {
            if (user != null)
            {
                if (sessions.Remove(session.Address))
                {
                    SendIGCMessage(session.Address, "bye");
                }
            }
        }

        private void ListCommand(string[] arguments, string rawArguments, Session session, User user)
        {
            if (user != null)
            {
                List<string> file_list = new List<string>();
                foreach (File file in files.Values)
                {
                    if (file.Path.StartsWith(session.CurrentDirectory) && file.CanList(user))
                    {
                        file_list.Add(file.Path);
                    }
                }
                file_list.Sort();
                StringBuilder message_builder = new StringBuilder();
                message_builder.Append("./ ../");
                foreach (string file_name in file_list)
                {
                    message_builder.Append(" ");
                    message_builder.Append(file_name);
                }
            }
        }

        private void ChangeDirectoryCommand(string[] arguments, string rawArguments, Session session, User user)
        {
            if ((user != null) && (arguments.Length > 0))
            {
                // TODO
            }
        }

        private void ReadFileCommand(string[] arguments, string rawArguments, Session session, User user)
        {
            if ((user != null) && (arguments.Length > 0))
            {
                string path = arguments[0];
                if (files.ContainsKey(path))
                {
                    File file = files[path];
                    if (file.CanRead(user))
                    {
                        SendIGCMessage(session.Address, "file", path + " " + Convert.ToBase64String(file.Data.ToArray()));
                    }
                }
            }
        }

        private void WriteFileCommand(string[] arguments, string rawArguments, Session session, User user)
        {
            if ((user != null) && (arguments.Length > 1))
            {
                string path = arguments[0];
                string content = arguments[1];
                if (files.ContainsKey(path))
                {
                    File file = files[path];
                    if (file.CanWrite(user))
                    {
                        try
                        {
                            byte[] data = Convert.FromBase64String(content);
                            if (data != null)
                            {
                                file.Data.Clear();
                                file.Data.AddRange(data);
                                SendIGCMessage(session.Address, "success", path);
                            }
                            SendIGCMessage(session.Address, "error", path);
                        }
                        catch
                        {
                            SendIGCMessage(session.Address, "error", path);
                        }
                    }
                    else
                    {
                        SendIGCMessage(session.Address, "denied", path);
                    }
                }
                else
                {
                    SendIGCMessage(session.Address, "denied", path);
                }
            }
        }

        private void ReadStringFileCommand(string[] arguments, string rawArguments, Session session, User user)
        {
            if ((user != null) && (arguments.Length > 0))
            {
                string path = arguments[0];
                if (files.ContainsKey(path))
                {
                    File file = files[path];
                    if (file.CanRead(user))
                    {
                        try
                        {
                            string content = Encoding.UTF8.GetString(file.Data.ToArray());
                            if (content != null)
                            {
                                SendIGCMessage(session.Address, "stringfile", path + " " + content);
                            }
                            else
                            {
                                SendIGCMessage(session.Address, "error", path);
                            }
                        }
                        catch
                        {
                            SendIGCMessage(session.Address, "error", path);
                        }
                    }
                    else
                    {
                        SendIGCMessage(session.Address, "denied", path);
                    }
                }
                else
                {
                    SendIGCMessage(session.Address, "denied", path);
                }
            }
        }

        private void WriteStringFileCommand(string[] arguments, string rawArguments, Session session, User user)
        {
            if ((user != null) && (arguments.Length > 1))
            {
                string path = arguments[0];
                string content = arguments[1];
                if (files.ContainsKey(path))
                {
                    File file = files[path];
                    if (file.CanWrite(user))
                    {
                        try
                        {
                            byte[] data = Encoding.UTF8.GetBytes(content);
                            if (data != null)
                            {
                                file.Data.Clear();
                                file.Data.AddRange(data);
                                SendIGCMessage(session.Address, "success", path);
                            }
                            SendIGCMessage(session.Address, "error", path);
                        }
                        catch
                        {
                            SendIGCMessage(session.Address, "error", path);
                        }
                    }
                    else
                    {
                        SendIGCMessage(session.Address, "denied", path);
                    }
                }
                else
                {
                    SendIGCMessage(session.Address, "denied", path);
                }
            }
        }

        public void Main()
        {
            Commands = new Dictionary<string, ExecuteCommandDelegate>
            {
                { "discover", DiscoverCommand },

                { "authenticate", AuthenticateCommand },
                { "auth", AuthenticateCommand },
                { "login", AuthenticateCommand },

                { "exit", ExitCommand },
                { "leave", ExitCommand },
                { "bye", ExitCommand },

                { "list", ListCommand },
                { "ls", ListCommand },
                { "l", ListCommand },

                { "changedirectory", ChangeDirectoryCommand },
                { "cd", ChangeDirectoryCommand },

                { "read", ReadFileCommand },
                { "r", ReadFileCommand },

                { "write", WriteFileCommand },
                { "w", WriteFileCommand },

                { "readstring", ReadStringFileCommand },
                { "rs", ReadStringFileCommand },

                { "writestring", WriteStringFileCommand },
                { "ws", WriteStringFileCommand },
            };
            while (fileSystemListener.HasPendingMessage)
            {
                try
                {
                    MyIGCMessage message = fileSystemListener.AcceptMessage();
                    if (message.Data is string)
                    {
                        string raw_command = message.As<string>().TrimStart();
                        if (raw_command.Length > 0)
                        {
                            List<string> arguments = new List<string>(raw_command.Split(' '));
                            if (arguments.Count > 0)
                            {
                                string command = arguments[0].ToLower();
                                arguments.RemoveAt(0);
                                if (command.Length > 0)
                                {
                                    if (Commands.ContainsKey(command))
                                    {
                                        Session session = (sessions.ContainsKey(message.Source) ? sessions[message.Source] : new Session(message.Source, string.Empty));
                                        Commands[command](arguments.ToArray(), (raw_command.Length > command.Length) ? raw_command.Substring(command.Length).TrimStart() : string.Empty, session, users.ContainsKey(session.UserName) ? users[session.UserName] : null);
                                    }
                                    
                                }

                            }
                            arguments.Clear();
                        }
                    }
                }
                catch (Exception e)
                {
                    Echo(e.ToString());
                }
            }
        }

        public void Save()
        {
            StorageBuilder storage_builder = new StorageBuilder();
            storage_builder.Add((uint)(users.Count));
            foreach (User user in users.Values)
            {
                storage_builder.Add(user.UserName);
                storage_builder.Add(user.Password);
            }

            storage_builder.Add((uint)(files.Count));
            foreach (File file in files.Values)
            {
                storage_builder.Add(file.Path);
                storage_builder.Add(file.Owner);
                storage_builder.Add((byte)((file.CanEveryoneRead ? FileFlags.CanEveryoneRead : FileFlags.Nothing) | (file.CanEveryoneWrite ? FileFlags.CanEveryoneWrite : FileFlags.Nothing)));
                storage_builder.AddSequence(file.Data);
            }
            Storage = storage_builder.Base64;
        }
    }
}
