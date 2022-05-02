using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceEngineersPrograms
{
    internal class FileSystemProgram : MyGridProgram
    {
        private delegate bool TraversePathDelegate(IFileSystemObject currentFileSystemObject, string nextPathPart);

        private struct FileSystemCommandResult
        {
            public static FileSystemCommandResult UnknownError { get; } = new FileSystemCommandResult(EFileSystemStatusCode.UnknownError, string.Empty);

            public static FileSystemCommandResult MissingArguments { get; } = new FileSystemCommandResult(EFileSystemStatusCode.MissingArguments, string.Empty);

            public static FileSystemCommandResult InvalidArguments { get; } = new FileSystemCommandResult(EFileSystemStatusCode.InvalidArguments, string.Empty);

            public static FileSystemCommandResult SavedState { get; } = new FileSystemCommandResult(EFileSystemStatusCode.SavedState, string.Empty);

            public static FileSystemCommandResult ShownHelpTopic { get; } = new FileSystemCommandResult(EFileSystemStatusCode.ShownHelpTopic, string.Empty);

            public EFileSystemStatusCode StatusCode { get; private set; }

            public string Path { get; private set; }

            public FileSystemCommandResult(EFileSystemStatusCode statusCode, string path)
            {
                if (path == null)
                {
                    throw new ArgumentNullException(path);
                }
                StatusCode = statusCode;
                Path = path;
            }
        }

        private static class PathUtils
        {
            public static IFileSystemObject TraversePath(IFileSystemObject beginFileSystemObject, string path, TraversePathDelegate onTraversePath)
            {
                if (beginFileSystemObject == null)
                {
                    throw new ArgumentNullException(nameof(beginFileSystemObject));
                }
                if (path == null)
                {
                    throw new ArgumentNullException(nameof(path));
                }
                if (path.Contains("|"))
                {
                    throw new ArgumentException("Illegal path character \"|\"", nameof(path));
                }
                IFileSystemObject ret = beginFileSystemObject;
                string[] path_parts = path.Split('/');
                bool traverse_to_root_directory = ((path_parts.Length > 0) ? string.IsNullOrEmpty(path_parts[0].Trim()) : false);
                foreach (string path_part in path_parts)
                {
                    string trimmed_path_part = path_part.Trim();
                    if ((onTraversePath == null) ? true : onTraversePath.Invoke(ret, trimmed_path_part))
                    {
                        if (traverse_to_root_directory)
                        {
                            traverse_to_root_directory = false;
                            while (ret.Parent != null)
                            {
                                ret = ret.Parent;
                            }
                        }
                        else
                        {
                            switch (trimmed_path_part)
                            {
                                case ".":
                                    break;
                                case "..":
                                    ret = ret.Parent;
                                    break;
                                default:
                                    ret = (ret.Children.ContainsKey(trimmed_path_part) ? ret.Children[trimmed_path_part] : null);
                                    break;
                            }
                        }
                        if (ret == null)
                        {
                            break;
                        }
                    }
                }
                return ret;
            }
        }

        private interface IFileSystemObject
        {
            IReadOnlyDictionary<string, IFileSystemObject> Children { get; }

            string FullPath { get; }

            string Name { get; }

            IFileSystemObject Parent { get; }

            bool Append(string content);

            void Clear();

            IFileSystemObject CreateDirectory(string directoryName);

            IFileSystemObject CreateFile(string fileName);

            bool Delete();

            bool Delete(string path);

            bool DoesDirectoryExist(string path);

            bool DoesFileExist(string path);

            bool DoesPathExist(string path);

            IFileSystemObject GetRelativeFileSystemObject(string path);

            string Read();

            string Read(uint startIndex);

            string Read(uint startIndex, uint length);

            bool Write(string content);
        }

        private abstract class AFileSystemObject : IFileSystemObject
        {
            public abstract IReadOnlyDictionary<string, IFileSystemObject> Children { get; }

            public string FullPath
            {
                get
                {
                    string ret;
                    if (Parent == null)
                    {
                        ret = "/";
                    }
                    else
                    {
                        StringBuilder full_path_string_builder = new StringBuilder();
                        IFileSystemObject current_file_system_object = this;
                        while (current_file_system_object.Parent != null)
                        {
                            full_path_string_builder.Insert(0, "/" + current_file_system_object.Name);
                            current_file_system_object = current_file_system_object.Parent;
                        }
                        ret = full_path_string_builder.ToString();
                    }
                    return ret;
                }
            }

            public string Name { get; }

            public IFileSystemObject Parent { get; }

            public AFileSystemObject(string name, IFileSystemObject parent)
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }
                Name = name;
                Parent = parent;
            }

            public abstract bool Append(string content);

            public abstract IFileSystemObject CreateDirectory(string directoryName);

            public abstract IFileSystemObject CreateFile(string fileName);

            public abstract void Clear();

            public bool Delete() => ((Parent == null) ? false : Parent.Delete(Name));

            public abstract bool Delete(string path);

            public bool DoesDirectoryExist(string path) => (GetRelativeFileSystemObject(path) is Directory);

            public bool DoesFileExist(string path) => (GetRelativeFileSystemObject(path) is File);

            public bool DoesPathExist(string path) => (GetRelativeFileSystemObject(path) != null);

            public IFileSystemObject GetRelativeFileSystemObject(string path) => PathUtils.TraversePath(this, path, (currentFileSystemObject, nextPathPart) => true);

            public abstract string Read();

            public abstract string Read(uint startIndex);

            public abstract string Read(uint startIndex, uint length);

            public abstract bool Write(string content);
        }

        private class Directory : AFileSystemObject
        {
            private Dictionary<string, IFileSystemObject> children = new Dictionary<string, IFileSystemObject>();

            public override IReadOnlyDictionary<string, IFileSystemObject> Children => children;

            public Directory(string name, IFileSystemObject parent) : base(name, parent)
            {
                // ...
            }

            public override bool Append(string content) => false;

            public override void Clear()
            {
                foreach (IFileSystemObject child in children.Values)
                {
                    child.Clear();
                }
                children.Clear();
            }

            public override IFileSystemObject CreateDirectory(string directoryName) => PathUtils.TraversePath(this, directoryName, (currentFileSystemObject, nextPathPart) =>
            {
                bool ret = false;
                if (currentFileSystemObject is Directory)
                {
                    switch (nextPathPart)
                    {
                        case "":
                        case ".":
                        case "..":
                            ret = true;
                            break;
                        default:
                            if (currentFileSystemObject.Children.ContainsKey(nextPathPart))
                            {
                                ret = (currentFileSystemObject.Children[nextPathPart] is Directory);
                            }
                            else
                            {
                                ((Directory)currentFileSystemObject).children.Add(nextPathPart, new Directory(nextPathPart, currentFileSystemObject));
                                ret = true;
                            }
                            break;
                    }
                }
                return ret;
            });

            public override IFileSystemObject CreateFile(string fileName)
            {
                IFileSystemObject ret = null;
                Directory directory = null;
                string file_name = null;
                if (PathUtils.TraversePath(this, fileName, (currentFileSystemObject, nextPathPart) =>
                {
                    directory = currentFileSystemObject as Directory;
                    if (directory == null)
                    {
                        file_name = null;
                    }
                    else
                    {
                        file_name = nextPathPart;
                    }
                    return (file_name != null);
                }) == null)
                {
                    if ((directory != null) && (!(string.IsNullOrWhiteSpace(file_name))))
                    {
                        switch (file_name)
                        {
                            case ".":
                            case "..":
                                break;
                            default:
                                ret = new File(file_name, directory);
                                directory.children.Add(file_name, ret);
                                break;
                        }
                    }
                }
                return ret;
            }

            public override bool Delete(string path)
            {
                bool ret = false;
                IFileSystemObject delete_file_system_object = GetRelativeFileSystemObject(path);
                if (delete_file_system_object != null)
                {
                    delete_file_system_object.Clear();
                    if (delete_file_system_object.Parent is Directory)
                    {
                        ret = ((Directory)(delete_file_system_object.Parent)).children.Remove(delete_file_system_object.Name);
                    }
                }
                return ret;
            }

            public override string Read() => string.Empty;

            public override string Read(uint startIndex) => string.Empty;

            public override string Read(uint startIndex, uint length) => string.Empty;

            public override bool Write(string content) => false;
        }

        private class File : AFileSystemObject
        {
            private static readonly IReadOnlyDictionary<string, IFileSystemObject> emptyFileSystemObjects = new Dictionary<string, IFileSystemObject>();

            private string content = string.Empty;

            public override IReadOnlyDictionary<string, IFileSystemObject> Children => emptyFileSystemObjects;

            public File(string name, IFileSystemObject parent) : base(name, parent)
            {
                // ...
            }

            public override bool Append(string content)
            {
                if (content == null)
                {
                    throw new ArgumentNullException(nameof(content));
                }
                this.content += content;
                return true;
            }

            public override void Clear()
            {
                content = string.Empty;
            }

            public override IFileSystemObject CreateDirectory(string directoryName) => null;

            public override IFileSystemObject CreateFile(string fileName) => null;

            public override bool Delete(string path) => false;

            public override string Read() => content;

            public override string Read(uint startIndex) => ((startIndex < content.Length) ? content.Substring((int)startIndex) : string.Empty);

            public override string Read(uint startIndex, uint length) => (((startIndex < content.Length) && (length <= (content.Length - startIndex))) ? content.Substring((int)startIndex, (int)length) : string.Empty);

            public override bool Write(string content)
            {
                if (content == null)
                {
                    throw new ArgumentNullException(nameof(content));
                }
                this.content = content;
                return true;
            }
        }

        private static readonly IReadOnlyDictionary<string, string> escapeCharacters = new Dictionary<string, string>
        {
            { @"\a", "\a" },
            { @"\b", "\b" },
            { @"\f", "\f" },
            { @"\n", "\n" },
            { @"\r", "\r" },
            { @"\t", "\t" },
            { @"\v", "\v" },
            { @"\", "\\" },
            { @"\'", "\'" },
            { "\\\"", "\"" }
        };

        private Directory rootDirectory;

        private Directory currentDirectory;

        private Logger standardOutputLogger;

        private Logger errorOutputLogger;

        private Directory RootDirectory
        {
            get
            {
                if (rootDirectory == null)
                {
                    rootDirectory = new Directory(string.Empty, null);
                    try
                    {
                        StorageDataSerializer storage_data_serializer = new StorageDataSerializer(Storage);
                        if (storage_data_serializer.ReadInt32() == 0x53464553)
                        {
                            uint num_directories = storage_data_serializer.ReadUInt32();
                            uint num_files = storage_data_serializer.ReadUInt32();
                            HashSet<string> directory_paths = new HashSet<string>();
                            Dictionary<string, string> file_paths_contents = new Dictionary<string, string>();

                            for (uint directory_index = 0U; directory_index != num_directories; directory_index++)
                            {
                                string directory_path = storage_data_serializer.ReadString();
                                if (!directory_paths.Add(directory_path))
                                {
                                    throw new FormatException("Duplicate directory entry \"" + directory_path + "\"");
                                }
                            }

                            for (uint file_index = 0U; file_index != num_files; file_index++)
                            {
                                string file_path = storage_data_serializer.ReadString();
                                string file_contents = storage_data_serializer.ReadString();
                                if (file_paths_contents.ContainsKey(file_path))
                                {
                                    throw new FormatException("Duplicate file entry \"" + file_path + "\"");
                                }
                                else
                                {
                                    file_paths_contents.Add(file_path, file_contents);
                                }
                            }

                            foreach (string directory_path in directory_paths)
                            {
                                if (rootDirectory.CreateDirectory(directory_path) == null)
                                {
                                    throw new InvalidOperationException("Failed to create directory \"" + directory_path + "\" from storage.");
                                }
                            }

                            foreach (KeyValuePair<string, string> file_path_content in file_paths_contents)
                            {
                                File file = rootDirectory.CreateFile(file_path_content.Key) as File;
                                if (file == null)
                                {
                                    throw new InvalidOperationException("Failed to create file \"" + file_path_content.Key + "\" from storage.");
                                }
                                if (!file.Write(file_path_content.Value))
                                {
                                    throw new InvalidOperationException("Failed to write file contents into \"" + file_path_content.Key + "\" from storage.");
                                }
                            }
                        }
                    }
                    catch
                    {
                        rootDirectory.Clear();
                    }
                }
                return rootDirectory;
            }
        }

        private Directory CurrentDirectory
        {
            get
            {
                if (currentDirectory == null)
                {
                    currentDirectory = RootDirectory;
                }
                return currentDirectory;
            }
            set
            {
                currentDirectory = ((value == null) ? RootDirectory : value);
            }
        }

        private Commands<FileSystemCommandResult> commands = new Commands<FileSystemCommandResult>('|', FileSystemCommandResult.InvalidArguments);

        public FileSystemProgram()
        {
            standardOutputLogger = new Logger(this, ELoggerType.StandardOutput);
            errorOutputLogger = new Logger(this, ELoggerType.ErrorOutput);
            commands.Add("append", "Append file contents to file", "This command appends file contents to the specified file path.", AppendCommand, "data");
            commands.Add("changedirectory", "Change directory", "This command changes directory.", ChangeDirectoryCommand, "directory path");
            commands.Add("createdirectory", "Create new directory", "This command creates a new directory.", CreateDirectoryCommand, "directory path");
            commands.Add("createfile", "Create new file", "This command creates a new file.", CreateFileCommand, "file path");
            commands.Add("currentdirectory", "Show current directory", "This command shows the current directory.", CurrentDirectoryCommand);
            commands.Add("delete", "Delete file or directory", "This command either deletes a file or a directory.", DeleteCommand, "path");
            commands.Add("help", "Help topics", "This command shows help topics.", HelpCommand);
            commands.Add("isdirectory", "Is a directory", "Shows if the specified path is a directory.", IsDirectoryCommand, "directory path");
            commands.Add("isfile", "Is a file", "Shows if the specified path is a file", IsFileCommand, "file path");
            commands.Add("ispath", "Is a path", "Shows if the specified path exists", IsPathCommand, "path");
            commands.Add("list", "List directories and files", "This command lists directories or files of the working directory or directories and files from the specified directory path.", ListCommand, ArgumentString.Optional("directory path"));
            commands.Add("read", "Read from file", "This command reads file contents from a file.", ReadCommand, "file path");
            commands.Add("save", "Save current state", "This command saves the current state.", SaveCommand);
            commands.Add("write", "Write file contents to file", "This command writes file contents to the specified file path.", WriteCommand, "file path", "data");
            commands.AddAliases("append", "a");
            commands.AddAliases("changedirectory", "cd");
            commands.AddAliases("createdirectory", "makedirectory", "createdir", "makedir", "mkdir", "md");
            commands.AddAliases("createfile", "cf");
            commands.AddAliases("currentdirectory", "pwd");
            commands.AddAliases("delete", "del", "rm");
            commands.AddAliases("help", "commands", "manual", "cmds", "cmd", "man", "h", "?");
            commands.AddAliases("list", "ls", "dir");
            commands.AddAliases("isdirectory", "isdir");
            commands.AddAliases("read", "r");
            commands.AddAliases("write", "w");
        }

        private FileSystemCommandResult AppendCommand(IReadOnlyList<string> arguments)
        {
            FileSystemCommandResult ret;
            string path = GetPathArgument(arguments);
            if (string.IsNullOrWhiteSpace(path))
            {
                ret = FileSystemCommandResult.InvalidArguments;
            }
            else
            {
                IFileSystemObject append_file = CurrentDirectory.GetRelativeFileSystemObject(path);
                if (append_file is File)
                {
                    if (((File)append_file).Append(GetContentArgument(arguments)))
                    {
                        ret = new FileSystemCommandResult(EFileSystemStatusCode.AppendedFileContents, append_file.FullPath);
                    }
                    else
                    {
                        ret = new FileSystemCommandResult(EFileSystemStatusCode.FailedAppendingFileContents, append_file.FullPath);
                    }
                }
                else if (append_file != null)
                {
                    ret = new FileSystemCommandResult(EFileSystemStatusCode.IsFileMissing, append_file.FullPath);
                }
                else
                {
                    ret = new FileSystemCommandResult(EFileSystemStatusCode.IsFileMissing, path);
                }
            }
            return ret;
        }

        private FileSystemCommandResult ChangeDirectoryCommand(IReadOnlyList<string> arguments)
        {
            FileSystemCommandResult ret;
            string path = GetPathArgument(arguments);
            if (string.IsNullOrWhiteSpace(path))
            {
                ret = FileSystemCommandResult.InvalidArguments;
            }
            else
            {
                IFileSystemObject result_file_system_object = CurrentDirectory.GetRelativeFileSystemObject(path);
                if (result_file_system_object is Directory)
                {
                    CurrentDirectory = (Directory)result_file_system_object;
                    ret = new FileSystemCommandResult(EFileSystemStatusCode.ChangedDirectory, result_file_system_object.FullPath);
                }
                else if (result_file_system_object != null)
                {
                    ret = new FileSystemCommandResult(EFileSystemStatusCode.FailedChangingDirectory, result_file_system_object.FullPath);
                }
                else
                {
                    ret = new FileSystemCommandResult(EFileSystemStatusCode.FailedChangingDirectory, path);
                }
            }
            return ret;
        }

        private FileSystemCommandResult CreateDirectoryCommand(IReadOnlyList<string> arguments)
        {
            FileSystemCommandResult ret;
            string path = GetPathArgument(arguments);
            if (string.IsNullOrWhiteSpace(path))
            {
                ret = FileSystemCommandResult.InvalidArguments;
            }
            else
            {
                IFileSystemObject created_directory = CurrentDirectory.CreateDirectory(path);
                if (created_directory == null)
                {
                    ret = new FileSystemCommandResult(EFileSystemStatusCode.FailedCreatingDirectory, path);
                }
                else
                {
                    ret = new FileSystemCommandResult(EFileSystemStatusCode.CreatedDirectory, created_directory.FullPath);
                }
            }
            return ret;
        }

        private FileSystemCommandResult CreateFileCommand(IReadOnlyList<string> arguments)
        {
            FileSystemCommandResult ret;
            string path = GetPathArgument(arguments);
            if (string.IsNullOrWhiteSpace(path))
            {
                ret = FileSystemCommandResult.InvalidArguments;
            }
            else
            {
                IFileSystemObject created_file = CurrentDirectory.CreateFile(path);
                if (created_file == null)
                {
                    ret = new FileSystemCommandResult(EFileSystemStatusCode.FailedCreatingFile, path);
                }
                else
                {
                    ret = new FileSystemCommandResult(EFileSystemStatusCode.CreatedFile, created_file.FullPath);
                }
            }
            return ret;
        }

        private FileSystemCommandResult CurrentDirectoryCommand(IReadOnlyList<string> arguments)
        {
            Me.CustomData = CurrentDirectory.FullPath;
            standardOutputLogger.AppendLine("Current directory: " + Me.CustomData);
            return new FileSystemCommandResult(EFileSystemStatusCode.CustomData, Me.CustomData);
        }

        private FileSystemCommandResult DeleteCommand(IReadOnlyList<string> arguments)
        {
            FileSystemCommandResult ret;
            string path = GetPathArgument(arguments);
            if (string.IsNullOrWhiteSpace(path))
            {
                ret = FileSystemCommandResult.InvalidArguments;
            }
            else
            {
                if (CurrentDirectory.Delete(path))
                {
                    ret = new FileSystemCommandResult(EFileSystemStatusCode.DeletedPath, path);
                }
                else
                {
                    ret = new FileSystemCommandResult(EFileSystemStatusCode.FailedDeletingPath, path);
                }
            }
            return ret;
        }

        private FileSystemCommandResult HelpCommand(IReadOnlyList<string> arguments)
        {
            if (arguments.Count > 0)
            {
                Echo(commands.GetHelpTopic(arguments[0]));
            }
            else
            {
                Echo(commands.GetHelpTopic());
            }
            return FileSystemCommandResult.ShownHelpTopic;
        }

        private FileSystemCommandResult IsDirectoryCommand(IReadOnlyList<string> arguments)
        {
            FileSystemCommandResult ret;
            string path = GetPathArgument(arguments);
            if (string.IsNullOrWhiteSpace(path))
            {
                ret = FileSystemCommandResult.InvalidArguments;
            }
            else
            {
                if (CurrentDirectory.DoesDirectoryExist(path))
                {
                    ret = new FileSystemCommandResult(EFileSystemStatusCode.IsDirectory, path);
                }
                else
                {
                    ret = new FileSystemCommandResult(EFileSystemStatusCode.IsNotDirectory, path);
                }
            }
            return ret;
        }

        private FileSystemCommandResult IsFileCommand(IReadOnlyList<string> arguments)
        {
            FileSystemCommandResult ret;
            string path = GetPathArgument(arguments);
            if (string.IsNullOrWhiteSpace(path))
            {
                ret = FileSystemCommandResult.InvalidArguments;
            }
            else
            {
                if (CurrentDirectory.DoesFileExist(path))
                {
                    ret = new FileSystemCommandResult(EFileSystemStatusCode.IsFile, path);
                }
                else
                {
                    ret = new FileSystemCommandResult(EFileSystemStatusCode.IsNotFile, path);
                }
            }
            return ret;
        }

        private FileSystemCommandResult IsPathCommand(IReadOnlyList<string> arguments)
        {
            FileSystemCommandResult ret;
            string path = GetPathArgument(arguments);
            if (string.IsNullOrWhiteSpace(path))
            {
                ret = FileSystemCommandResult.InvalidArguments;
            }
            else
            {
                if (CurrentDirectory.DoesPathExist(path))
                {
                    ret = new FileSystemCommandResult(EFileSystemStatusCode.IsPath, path);
                }
                else
                {
                    ret = new FileSystemCommandResult(EFileSystemStatusCode.IsNotPath, path);
                }
            }
            return ret;
        }

        private FileSystemCommandResult ListCommand(IReadOnlyList<string> arguments)
        {
            FileSystemCommandResult ret;
            Directory list_directory = ((arguments.Count > 0) ? CurrentDirectory.GetRelativeFileSystemObject(arguments[0]) : CurrentDirectory) as Directory;
            if (list_directory == null)
            {
                ret = new FileSystemCommandResult(EFileSystemStatusCode.IsDirectoryMissing, arguments[0]);
            }
            else
            {
                List<IFileSystemObject> sorted_file_system_objects = new List<IFileSystemObject>(list_directory.Children.Values);
                sorted_file_system_objects.Sort((left, right) =>
                {
                    int result = (left is Directory).CompareTo(right is Directory);
                    if (result == 0)
                    {
                        result = left.Name.CompareTo(right.Name);
                    }
                    return result;
                });
                StringBuilder list_directory_string_builder = new StringBuilder();
                list_directory_string_builder.AppendLine("/");
                list_directory_string_builder.AppendLine(".");
                list_directory_string_builder.AppendLine("..");
                foreach (IFileSystemObject sorted_file_system_object in sorted_file_system_objects)
                {
                    list_directory_string_builder.Append(sorted_file_system_object.Name);
                    if (sorted_file_system_object is Directory)
                    {
                        list_directory_string_builder.Append("/");
                    }
                    list_directory_string_builder.AppendLine();
                }
                Me.CustomData = list_directory_string_builder.ToString();
                list_directory_string_builder.Clear();
                ret = new FileSystemCommandResult(EFileSystemStatusCode.CustomData, list_directory.FullPath);
            }
            return ret;
        }

        private FileSystemCommandResult ReadCommand(IReadOnlyList<string> arguments)
        {
            FileSystemCommandResult ret;
            string path = GetPathArgument(arguments);
            if (string.IsNullOrWhiteSpace(path))
            {
                ret = FileSystemCommandResult.InvalidArguments;
            }
            else
            {
                IFileSystemObject read_file = CurrentDirectory.GetRelativeFileSystemObject(path);
                if (read_file is File)
                {
                    Me.CustomData = ((File)read_file).Read();
                    standardOutputLogger.AppendLine("Read file \"" + read_file.FullPath + "\".");
                    ret = new FileSystemCommandResult(EFileSystemStatusCode.CustomData, path);
                }
                else if (read_file != null)
                {
                    Me.CustomData = string.Empty;
                    path = read_file.FullPath;
                    errorOutputLogger.AppendLine("\"" + read_file.FullPath + "\" is not a file.");
                    ret = new FileSystemCommandResult(EFileSystemStatusCode.CustomData, path);
                }
                else
                {
                    Me.CustomData = string.Empty;
                    errorOutputLogger.AppendLine("\"" + path + "\" does not exist.");
                    ret = new FileSystemCommandResult(EFileSystemStatusCode.CustomData, path);
                }
            }
            return ret;
        }

        private FileSystemCommandResult SaveCommand(IReadOnlyList<string> arguments)
        {
            Save();
            return FileSystemCommandResult.SavedState;
        }

        private FileSystemCommandResult WriteCommand(IReadOnlyList<string> arguments)
        {
            FileSystemCommandResult ret;
            string path = GetPathArgument(arguments);
            if (string.IsNullOrWhiteSpace(path))
            {
                ret = FileSystemCommandResult.InvalidArguments;
            }
            else
            {
                IFileSystemObject write_file = CurrentDirectory.GetRelativeFileSystemObject(path);
                if (write_file is File)
                {
                    if (((File)write_file).Write(GetContentArgument(arguments)))
                    {
                        ret = new FileSystemCommandResult(EFileSystemStatusCode.WrittenFileContents, write_file.FullPath);
                    }
                    else
                    {
                        ret = new FileSystemCommandResult(EFileSystemStatusCode.FailedWritingFileContents, write_file.FullPath);
                    }
                }
                else if (write_file != null)
                {
                    ret = new FileSystemCommandResult(EFileSystemStatusCode.IsFileMissing, write_file.FullPath);
                }
                else
                {
                    ret = new FileSystemCommandResult(EFileSystemStatusCode.IsFileMissing, path);
                }
            }
            return ret;
        }

        private static string GetPathArgument(IReadOnlyList<string> arguments) => ((arguments.Count > 0) ? arguments[0].Trim() : string.Empty);

        private static string UnescapeText(string escapedText)
        {
            string ret = escapedText;
            foreach (KeyValuePair<string, string> escape_character in escapeCharacters)
            {
                ret = ret.Replace(escape_character.Key, escape_character.Value);
            }
            return ret;
        }

        private string GetContentArgument(IReadOnlyList<string> arguments)
        {
            StringBuilder content_string_builder = new StringBuilder();
            bool first = true;
            for (int i = 1; i < arguments.Count; i++)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    content_string_builder.Append(commands.Delimiter);
                }
                content_string_builder.Append(UnescapeText(arguments[i]));
            }
            return content_string_builder.ToString();
        }

        private void LogHelp(string helpTopic) => standardOutputLogger.AppendLine(commands.GetHelpTopic(helpTopic));

        private void Main(string argument, UpdateType updateSource)
        {
            string command_name;
            FileSystemCommandResult command_result = commands.Parse(argument, out command_name);
            switch (command_result.StatusCode)
            {
                case EFileSystemStatusCode.MissingArguments:
                    LogHelp(command_name);
                    break;
                case EFileSystemStatusCode.InvalidArguments:
                    LogHelp(command_name);
                    break;
                case EFileSystemStatusCode.IsFileMissing:
                    errorOutputLogger.AppendLine("File \"" + command_result.Path + "\" is missing.");
                    break;
                case EFileSystemStatusCode.IsFile:
                    standardOutputLogger.AppendLine("\"" + command_result.Path + "\" is a file.");
                    break;
                case EFileSystemStatusCode.IsNotFile:
                    standardOutputLogger.AppendLine("\"" + command_result.Path + "\" is not a file.");
                    break;
                case EFileSystemStatusCode.IsDirectory:
                    standardOutputLogger.AppendLine("\"" + command_result.Path + "\" is a directory.");
                    break;
                case EFileSystemStatusCode.IsNotDirectory:
                    standardOutputLogger.AppendLine("\"" + command_result.Path + "\" is not a directory.");
                    break;
                case EFileSystemStatusCode.IsPath:
                    standardOutputLogger.AppendLine("\"" + command_result.Path + "\" is a path.");
                    break;
                case EFileSystemStatusCode.IsNotPath:
                    standardOutputLogger.AppendLine("\"" + command_result.Path + "\" is not a path.");
                    break;
                case EFileSystemStatusCode.ChangedDirectory:
                    standardOutputLogger.AppendLine("Changed directory to \"" + command_result.Path + "\"");
                    break;
                case EFileSystemStatusCode.FailedChangingDirectory:
                    errorOutputLogger.AppendLine("\"" + command_result.Path + "\" is not a directory.");
                    break;
                case EFileSystemStatusCode.CreatedDirectory:
                    standardOutputLogger.AppendLine("Successfully created directory \"" + command_result.Path + "\".");
                    break;
                case EFileSystemStatusCode.FailedCreatingDirectory:
                    errorOutputLogger.AppendLine("Failed to create directory \"" + command_result.Path + "\".");
                    break;
                case EFileSystemStatusCode.CreatedFile:
                    standardOutputLogger.AppendLine("Successfully created file \"" + command_result.Path + "\".");
                    break;
                case EFileSystemStatusCode.FailedCreatingFile:
                    errorOutputLogger.AppendLine("Failed to create file \"" + command_result.Path + "\".");
                    break;
                case EFileSystemStatusCode.WrittenFileContents:
                    standardOutputLogger.AppendLine("Successfully written contents to file \"" + command_result.Path + "\".");
                    break;
                case EFileSystemStatusCode.FailedWritingFileContents:
                    errorOutputLogger.AppendLine("Failed to write contents to file \"" + command_result.Path + "\".");
                    break;
                case EFileSystemStatusCode.AppendedFileContents:
                    standardOutputLogger.AppendLine("Successfully appended contents to file \"" + command_result.Path + "\".");
                    break;
                case EFileSystemStatusCode.FailedAppendingFileContents:
                    errorOutputLogger.AppendLine("Failed to append contents to file \"" + command_result.Path + "\".");
                    break;
                case EFileSystemStatusCode.DeletedPath:
                    standardOutputLogger.AppendLine("Successfully deleted \"" + command_result.Path + "\".");
                    break;
                case EFileSystemStatusCode.FailedDeletingPath:
                    errorOutputLogger.AppendLine("Failed to delete \"" + command_result.Path + "\".");
                    break;
                case EFileSystemStatusCode.UnknownError:
                    errorOutputLogger.AppendLine("Unknown error.");
                    LogHelp(command_name);
                    break;
            }
            if (command_result.StatusCode != EFileSystemStatusCode.CustomData)
            {
                Me.CustomData = command_result.StatusCode.ToString();
            }
        }

        private void GetFileSystemObjectsRecursive(IFileSystemObject fileSystemObject, HashSet<string> directoryPaths, Dictionary<string, string> filePathsContents)
        {
            foreach (IFileSystemObject file_system_object in fileSystemObject.Children.Values)
            {
                if (file_system_object is Directory)
                {
                    directoryPaths.Add(file_system_object.FullPath);
                }
                else if (file_system_object is File)
                {
                    filePathsContents.Add(file_system_object.FullPath, file_system_object.Read());
                }
                GetFileSystemObjectsRecursive(file_system_object, directoryPaths, filePathsContents);
            }
        }

        private void Save()
        {
            StorageDataSerializer storage_data_serializer = new StorageDataSerializer();
            storage_data_serializer.WriteUInt32(0x53464553);
            HashSet<string> directory_paths = new HashSet<string>();
            Dictionary<string, string> file_paths_contents = new Dictionary<string, string>();
            GetFileSystemObjectsRecursive(RootDirectory, directory_paths, file_paths_contents);
            storage_data_serializer.WriteInt32(directory_paths.Count);
            storage_data_serializer.WriteInt32(file_paths_contents.Count);
            foreach (string directory_path in directory_paths)
            {
                storage_data_serializer.WriteString(directory_path);
            }
            foreach (KeyValuePair<string, string> file_path_content in file_paths_contents)
            {
                storage_data_serializer.WriteString(file_path_content.Key);
                storage_data_serializer.WriteString(file_path_content.Value);
            }
            Storage = storage_data_serializer.StorageString;
        }
    }
}
