using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace SpaceEngineersPrograms
{
    public class FileSystem
    {
        private static readonly IReadOnlyDictionary<string, string> escapeCharacters = new Dictionary<string, string>
        {
            { "\a", @"\a" },
            { "\b", @"\b" },
            { "\f", @"\f" },
            { "\n", @"\n" },
            { "\r", @"\r" },
            { "\t", @"\t" },
            { "\v", @"\v" },
            { "\\", @"\" },
            { "\'", @"\'" },
            { "\"", "\\\"" }
        };

        private static List<IMyProgrammableBlock> fileSystemProgrammableBlocks = new List<IMyProgrammableBlock>();

        private MyGridProgram gridProgram;

        private IMyProgrammableBlock fileSystemProgrammableBlock;

        public IMyProgrammableBlock FileSystemProgrammableBlock
        {
            get
            {
                if (fileSystemProgrammableBlock == null)
                {
                    InitFileSystem();
                }
                else if (!(fileSystemProgrammableBlock.IsWorking))
                {
                    InitFileSystem();
                }
                return fileSystemProgrammableBlock;
            }
        }

        public string CurrentDirectory
        {
            get
            {
                string ret;
                if (SendCommand("currentdirectory", out ret) != EFileSystemStatusCode.CustomData)
                {
                    ret = string.Empty;
                }
                return ret;
            }
            set
            {
                ChangeDirectory(value);
            }
        }

        public FileSystem(MyGridProgram gridProgram)
        {
            if (gridProgram == null)
            {
                throw new ArgumentNullException(nameof(gridProgram));
            }
            this.gridProgram = gridProgram;
        }

        private void InitFileSystem()
        {
            gridProgram.GridTerminalSystem.GetBlocksOfType(fileSystemProgrammableBlocks, (programmable_block) => (programmable_block.CustomName.Trim().ToLower() == "filesystem"));
            if (fileSystemProgrammableBlocks.Count > 0)
            {
                foreach (IMyProgrammableBlock file_system_programmable_block in fileSystemProgrammableBlocks)
                {
                    if (file_system_programmable_block.IsWorking)
                    {
                        fileSystemProgrammableBlock = file_system_programmable_block;
                        break;
                    }
                }
            }
            fileSystemProgrammableBlocks.Clear();
        }

        private EFileSystemStatusCode SendCommand(string argument, out string result)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(nameof(argument));
            }
            EFileSystemStatusCode ret = EFileSystemStatusCode.UnknownError;
            IMyProgrammableBlock file_system_programmable_block = FileSystemProgrammableBlock;
            result = string.Empty;
            if (file_system_programmable_block != null)
            {
                if (file_system_programmable_block.TryRun(argument))
                {
                    result = file_system_programmable_block.CustomData;
                    if (!(Enum.TryParse(result, out ret)))
                    {
                        ret = EFileSystemStatusCode.CustomData;
                    }
                }
            }
            return ret;
        }

        private static string EscapeText(string text)
        {
            string ret = text;
            foreach (KeyValuePair<string, string> escape_character in escapeCharacters)
            {
                ret = ret.Replace(escape_character.Key, escape_character.Value);
            }
            return ret;
        }

        private static bool ContainsControlCharacters(string text)
        {
            bool ret = false;
            foreach (string escape_character in escapeCharacters.Keys)
            {
                if (text.Contains(escape_character))
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }

        public bool AppendFile(string path, string data)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (ContainsControlCharacters(path))
            {
                throw new ArgumentException("Control characters are not allowed", nameof(path));
            }
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            string result;
            return (SendCommand("append|" + path + "|" + EscapeText(data), out result) == EFileSystemStatusCode.AppendedFileContents);
        }

        public bool ChangeDirectory(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (ContainsControlCharacters(path))
            {
                throw new ArgumentException("Control characters are not allowed", nameof(path));
            }
            string result;
            return (SendCommand("changedirectory|" + path, out result) == EFileSystemStatusCode.ChangedDirectory);
        }

        public bool CreateDirectory(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (ContainsControlCharacters(path))
            {
                throw new ArgumentException("Control characters are not allowed", nameof(path));
            }
            string result;
            return (SendCommand("createdirectory|" + path, out result) == EFileSystemStatusCode.CreatedDirectory);
        }

        public bool CreateFile(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (ContainsControlCharacters(path))
            {
                throw new ArgumentException("Control characters are not allowed", nameof(path));
            }
            string result;
            return (SendCommand("createfile|" + path, out result) == EFileSystemStatusCode.CreatedFile);
        }

        public bool Delete(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (ContainsControlCharacters(path))
            {
                throw new ArgumentException("Control characters are not allowed", nameof(path));
            }
            string result;
            return (SendCommand("delete|" + path, out result) == EFileSystemStatusCode.DeletedPath);
        }

        public bool IsDirectory(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (ContainsControlCharacters(path))
            {
                throw new ArgumentException("Control characters are not allowed", nameof(path));
            }
            string result;
            return (SendCommand("isdirectory|" + path, out result) == EFileSystemStatusCode.IsDirectory);
        }

        public bool IsFile(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (ContainsControlCharacters(path))
            {
                throw new ArgumentException("Control characters are not allowed", nameof(path));
            }
            string result;
            return (SendCommand("isfile|" + path, out result) == EFileSystemStatusCode.IsFile);
        }

        public bool IsPath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (ContainsControlCharacters(path))
            {
                throw new ArgumentException("Control characters are not allowed", nameof(path));
            }
            string result;
            return (SendCommand("ispath|" + path, out result) == EFileSystemStatusCode.IsPath);
        }

        public IReadOnlyList<string> List()
        {
            string[] ret = Array.Empty<string>();
            string list;
            if (SendCommand("list", out list) == EFileSystemStatusCode.CustomData)
            {
                ret = list.Split('\n');
                for (int i = 0; i < ret.Length; i++)
                {
                    ret[i] = ret[i].Trim();
                }
            }
            return ret;
        }

        public IReadOnlyList<string> List(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (ContainsControlCharacters(path))
            {
                throw new ArgumentException("Control characters are not allowed", nameof(path));
            }
            string[] ret = Array.Empty<string>();
            string list;
            if (SendCommand("list|" + path, out list) == EFileSystemStatusCode.CustomData)
            {
                ret = list.Split('\n');
                for (int i = 0; i < ret.Length; i++)
                {
                    ret[i] = ret[i].Trim();
                }
            }
            return ret;
        }

        public string ReadFile(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (ContainsControlCharacters(path))
            {
                throw new ArgumentException("Control characters are not allowed", nameof(path));
            }
            string ret;
            if (SendCommand("read|" + path, out ret) != EFileSystemStatusCode.CustomData)
            {
                ret = string.Empty;
            }
            return ret;
        }

        public bool WriteFile(string path, string data)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (ContainsControlCharacters(path))
            {
                throw new ArgumentException("Control characters are not allowed", nameof(path));
            }
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            string result;
            return (SendCommand("write|" + path + "|" + EscapeText(data), out result) == EFileSystemStatusCode.WrittenFileContents);
        }
    }
}
