using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceEngineersPrograms
{
    public class EngineersOS : MyGridProgram
    {
        private static class PathUtils
        {
            public static string EscapePath(string input)
            {
                if (input == null)
                {
                    throw new ArgumentNullException(nameof(input));
                }
                return input
                    .Replace(" ", "%20")
                    .Replace("$", "%24")
                    .Replace("&", "%26")
                    .Replace("`", "%60")
                    .Replace("<", "%3C")
                    .Replace(">", "%3E")
                    .Replace("[", "%5B")
                    .Replace("]", "%5D")
                    .Replace("{", "%7B")
                    .Replace("}", "%7D")
                    .Replace("\"", "%22")
                    .Replace("+", "%2B")
                    .Replace("#", "%23")
                    .Replace("%", "%25")
                    .Replace("@", "%40")
                    .Replace("/", "%2F")
                    .Replace(";", "%3B")
                    .Replace("=", "%3D")
                    .Replace("?", "%3F")
                    .Replace("\\", "%5C")
                    .Replace("^", "%5E")
                    .Replace("|", "%7C")
                    .Replace("~", "%7E")
                    .Replace("‘", "%27")
                    .Replace(",", "%2C");
            }
        }

        private interface IFileSystemObject
        {
            string Name { get; }

            bool IsFile { get; }

            bool IsDirectory { get; }

            bool CanRead { get; }

            bool CanWrite { get; }

            bool CanExecute { get; }

            bool CanCreateFile { get; }

            bool CanCreateDirectory { get; }

            bool CanDeleteSelf { get; }

            bool CanDeleteFile { get; }

            bool CanDeleteDirectory { get; }

            IReadOnlyDictionary<string, IFileSystemObject> Children { get; }

            IFileSystemObject ParentFileSystemObject { get; }

            bool Read(out string content);

            bool Write(string content);

            bool Execute(string argument, Console console);

            IFileSystemObject CreateFile(string path);

            IFileSystemObject CreateDirectory(string path);

            bool Delete();

            bool DeleteChild(string name);
        }

        private abstract class AFileSystemObject : IFileSystemObject
        {
            public string Name { get; private set; }

            public abstract bool IsFile { get; }

            public abstract bool IsDirectory { get; }

            public abstract bool CanRead { get; }

            public abstract bool CanWrite { get; }

            public abstract bool CanExecute { get; }

            public abstract bool CanCreateFile { get; }

            public abstract bool CanCreateDirectory { get; }

            public bool CanDeleteSelf
            {
                get
                {
                    return ((ParentFileSystemObject == null) ? false : ((IsFile && ParentFileSystemObject.CanDeleteFile) || (IsDirectory && ParentFileSystemObject.CanDeleteDirectory)));
                }
            }

            public abstract bool CanDeleteFile { get; }

            public abstract bool CanDeleteDirectory { get; }

            public abstract IReadOnlyDictionary<string, IFileSystemObject> Children { get; }

            public IFileSystemObject ParentFileSystemObject { get; private set; }

            public AFileSystemObject(string name, IFileSystemObject parentFileSystemObject)
            {
                Name = ((name == null) ? string.Empty : name);
                ParentFileSystemObject = parentFileSystemObject;
            }

            public abstract bool Read(out string content);

            public abstract bool Write(string content);

            public abstract bool Execute(string argument, Console console);

            public abstract IFileSystemObject CreateFile(string path);

            public abstract IFileSystemObject CreateDirectory(string path);

            public bool Delete() => ((ParentFileSystemObject == null) ? false : ParentFileSystemObject.DeleteChild(Name));

            public abstract bool DeleteChild(string name);
        }

        private class CommandFileSystemObject : AFileSystemObject
        {
            public delegate bool CommandExecuteDelegate(string argument, Console console);

            private CommandExecuteDelegate onCommandExecuted;

            public override bool IsFile => false;

            public override bool IsDirectory => false;

            public override bool CanRead => false;

            public override bool CanWrite => false;

            public override bool CanExecute => true;

            public override bool CanCreateFile => false;

            public override bool CanCreateDirectory => false;

            public override bool CanDeleteFile => false;

            public override bool CanDeleteDirectory => false;

            public override IReadOnlyDictionary<string, IFileSystemObject> Children { get; } = new Dictionary<string, IFileSystemObject>();

            public CommandFileSystemObject(string name, IFileSystemObject parentFileSystemObject, CommandExecuteDelegate onCommandExecuted) : base(name, parentFileSystemObject)
            {
                this.onCommandExecuted = onCommandExecuted;
            }

            public override IFileSystemObject CreateDirectory(string path) => null;

            public override IFileSystemObject CreateFile(string path) => null;

            public override bool DeleteChild(string name) => false;

            public override bool Execute(string argument, Console console)
            {
                bool ret = false;
                if (onCommandExecuted != null)
                {
                    ret = onCommandExecuted((argument == null) ? string.Empty : argument, console);
                }
                return ret;
            }

            public override bool Read(out string content)
            {
                content = string.Empty;
                return false;
            }

            public override bool Write(string content) => false;
        }

        private class CommandsDirectoryFileSystemObject : AFileSystemObject
        {
            private Dictionary<string, IFileSystemObject> children;

            public override bool IsFile => false;

            public override bool IsDirectory => true;

            public override bool CanRead => false;

            public override bool CanWrite => false;

            public override bool CanExecute => false;

            public override bool CanCreateFile => false;

            public override bool CanCreateDirectory => false;

            public override bool CanDeleteFile => false;

            public override bool CanDeleteDirectory => false;

            public override IReadOnlyDictionary<string, IFileSystemObject> Children => children;

            public CommandsDirectoryFileSystemObject(string name, IFileSystemObject parentFileSystemObject) : base(name, parentFileSystemObject)
            {
                children = new Dictionary<string, IFileSystemObject>
                {
                    { "list", new CommandFileSystemObject("list", this, ListCommand) },
                    { "ls", new CommandFileSystemObject("ls", this, ListCommand) },
                    { "dir", new CommandFileSystemObject("dir", this, ListCommand) }
                };
            }

            public bool ListCommand(string argument, Console console)
            {
                if (console != null)
                {
                    IFileSystemObject file_system_object = console.Shell.GetPath(argument);
                    if (file_system_object == null)
                    {
                        console.Print("Failed to list \"");
                        console.Print(argument);
                        console.PrintLine("\": Invalid path");
                    }
                    else
                    {
                        if (file_system_object.IsDirectory)
                        {
                            foreach (string file_system_object_child in file_system_object.Children.Keys)
                            {
                                console.Print("\t");
                                console.PrintLine(file_system_object_child);
                            }
                        }
                        else if (file_system_object.IsFile)
                        {
                            console.Print("Failed to list \"");
                            console.Print(argument);
                            console.PrintLine("\": Is a file");
                        }
                        else
                        {
                            console.Print("Failed to list \"");
                            console.Print(argument);
                            console.PrintLine("\": Is not a directory");
                        }
                    }
                }
                return true;
            }

            public override IFileSystemObject CreateDirectory(string path) => null;

            public override IFileSystemObject CreateFile(string path) => null;

            public override bool DeleteChild(string name) => false;

            public override bool Execute(string argument, Console console) => false;

            public override bool Read(out string content)
            {
                content = string.Empty;
                return false;
            }

            public override bool Write(string content) => false;
        }

        private class AppFileSystemObject : AFileSystemObject
        {
            private Dictionary<string, IFileSystemObject> children = new Dictionary<string, IFileSystemObject>(); 

            public override bool IsFile => false;

            public override bool IsDirectory => false;

            public override bool CanRead => false;

            public override bool CanWrite => false;

            public override bool CanExecute => true;

            public override bool CanCreateFile => false;

            public override bool CanCreateDirectory => false;

            public override bool CanDeleteFile => false;

            public override bool CanDeleteDirectory => false;

            public override IReadOnlyDictionary<string, IFileSystemObject> Children => children;

            public IMyProgrammableBlock ProgrammableBlock { get; private set; }

            public AppFileSystemObject(string name, IFileSystemObject parentFileSystemObject, IMyProgrammableBlock programmableBlock) : base(name, parentFileSystemObject)
            {
                ProgrammableBlock = programmableBlock;
            }

            public override IFileSystemObject CreateFile(string path) => null;

            public override IFileSystemObject CreateDirectory(string path) => null;

            public override bool DeleteChild(string name) => false;

            public override bool Execute(string argument, Console console) => ((ProgrammableBlock == null) ? false : ProgrammableBlock.TryRun(argument));

            public override bool Read(out string content)
            {
                content = string.Empty;
                return false;
            }

            public override bool Write(string content) => false;
        }

        private class AppsDirectoryFileSystemObject : AFileSystemObject
        {
            private IMyGridTerminalSystem gridTerminalSystem;

            private Dictionary<string, IFileSystemObject> children = new Dictionary<string, IFileSystemObject>();

            private List<IMyProgrammableBlock> programmableBlocks = new List<IMyProgrammableBlock>();

            public override bool IsFile => false;

            public override bool IsDirectory => true;

            public override bool CanRead => false;

            public override bool CanWrite => false;

            public override bool CanExecute => false;

            public override bool CanCreateFile => false;

            public override bool CanCreateDirectory => false;

            public override bool CanDeleteFile => false;

            public override bool CanDeleteDirectory => false;

            public override IReadOnlyDictionary<string, IFileSystemObject> Children
            {
                get
                {
                    if (gridTerminalSystem != null)
                    {
                        children.Clear();
                        programmableBlocks.Clear();
                        gridTerminalSystem.GetBlocksOfType(programmableBlocks);
                        foreach (IMyProgrammableBlock programmable_block in programmableBlocks)
                        {
                            string key = PathUtils.EscapePath(programmable_block.CustomName);
                            if (key.Length > 0)
                            {
                                if (children.ContainsKey(key))
                                {
                                    AppFileSystemObject existing_app_path = (AppFileSystemObject)(children[key]);
                                    string original_key = key;
                                    string new_key = key;
                                    do
                                    {
                                        original_key = original_key + "_" + existing_app_path.ProgrammableBlock.EntityId;
                                        new_key = new_key + "_" + programmable_block.EntityId;
                                    }
                                    while ((original_key == new_key) || children.ContainsKey(original_key) || children.ContainsKey(new_key));
                                    children.Remove(key);
                                    children.Add(original_key, existing_app_path);
                                    children.Add(new_key, new AppFileSystemObject(new_key, this, programmable_block));
                                }
                                else
                                {
                                    children.Add(key, new AppFileSystemObject(key, this, programmable_block));
                                }
                            }
                            else
                            {
                                key = programmable_block.EntityId.ToString();
                                children.Add(key, new AppFileSystemObject(key, this, programmable_block));
                            }
                        }
                    }
                    return children;
                }
            }

            public AppsDirectoryFileSystemObject(string name, IFileSystemObject parentFileSystemObject, IMyGridTerminalSystem gridTerminalSystem) : base(name, parentFileSystemObject)
            {
                this.gridTerminalSystem = gridTerminalSystem;
            }

            public override IFileSystemObject CreateFile(string path) => null;

            public override IFileSystemObject CreateDirectory(string path) => null;

            public override bool DeleteChild(string name) => false;

            public override bool Execute(string argument, Console console) => false;

            public override bool Read(out string content)
            {
                content = string.Empty;
                return false;
            }

            public override bool Write(string content) => false;
        }

        private class FileFileSystemObject : AFileSystemObject
        {
            private string content = string.Empty;

            public override bool IsFile => true;

            public override bool IsDirectory => false;

            public override bool CanRead => true;

            public override bool CanWrite => true;

            public override bool CanExecute => false;

            public override bool CanCreateFile => false;

            public override bool CanCreateDirectory => false;

            public override bool CanDeleteFile => false;

            public override bool CanDeleteDirectory => false;

            public override IReadOnlyDictionary<string, IFileSystemObject> Children { get; } = new Dictionary<string, IFileSystemObject>();

            public FileFileSystemObject(string name, IFileSystemObject parentFileSystemObject) : base(name, parentFileSystemObject)
            {
                // ...
            }

            public override IFileSystemObject CreateFile(string path) => null;

            public override IFileSystemObject CreateDirectory(string path) => null;

            public override bool DeleteChild(string name) => false;

            public override bool Execute(string argument, Console console) => false;

            public override bool Read(out string content)
            {
                content = this.content;
                return true;
            }

            public override bool Write(string content)
            {
                bool ret = false;
                if (content != null)
                {
                    this.content = content;
                    ret = true;
                }
                return ret;
            }
        }

        private class DirectoryFileSystemObject : AFileSystemObject
        {
            private Dictionary<string, IFileSystemObject> children = new Dictionary<string, IFileSystemObject>();

            public override bool IsFile => false;

            public override bool IsDirectory => true;

            public override bool CanRead => false;

            public override bool CanWrite => false;

            public override bool CanExecute => false;

            public override bool CanCreateFile => true;

            public override bool CanCreateDirectory => true;

            public override bool CanDeleteFile => true;

            public override bool CanDeleteDirectory => true;

            public override IReadOnlyDictionary<string, IFileSystemObject> Children => children;

            public DirectoryFileSystemObject(string name, IFileSystemObject parentFileSystemObject) : base(name, parentFileSystemObject)
            {
                // ...
            }

            public override IFileSystemObject CreateFile(string fileName)
            {
                FileFileSystemObject ret = null;
                if (fileName != null)
                {
                    string key = PathUtils.EscapePath(fileName);
                    if (key.Length > 0)
                    {
                        if (!(children.ContainsKey(key)))
                        {
                            ret = new FileFileSystemObject(key, this);
                            children.Add(key, ret);
                        }
                    }
                }
                return ret;
            }

            public override IFileSystemObject CreateDirectory(string directoryName)
            {
                DirectoryFileSystemObject ret = null;
                if (directoryName != null)
                {
                    string key = PathUtils.EscapePath(directoryName);
                    if (key.Length > 0)
                    {
                        if (!(children.ContainsKey(key)))
                        {
                            ret = new DirectoryFileSystemObject(key, this);
                            children.Add(key, ret);
                        }
                    }
                }
                return ret;
            }

            public override bool DeleteChild(string name)
            {
                bool ret = false;
                if (name != null)
                {
                    string key = PathUtils.EscapePath(name);
                    if (key.Length > 0)
                    {
                        if (children.ContainsKey(key))
                        {
                            IFileSystemObject file_system_object = children[key];
                            if ((file_system_object.IsFile && CanDeleteFile) || (file_system_object.IsDirectory && CanDeleteDirectory))
                            {
                                ret = children.Remove(key);
                            }
                        }
                    }
                }
                return ret;
            }

            public override bool Execute(string argument, Console console) => false;

            public override bool Read(out string content)
            {
                content = string.Empty;
                return false;
            }

            public override bool Write(string content) => false;
        }

        private class FilesDirectoryPath : DirectoryFileSystemObject
        {
            public FilesDirectoryPath(string name, IFileSystemObject parentDirectory, string filesStorage) : base(name, parentDirectory)
            {
                if (filesStorage != null)
                {
                    // TODO
                }
            }

            public string FilesStorage
            {
                get
                {
                    string ret = string.Empty;
                    // TODO
                    return ret;
                }
            }
        }

        private class RootDirectory : AFileSystemObject
        {
            private Dictionary<string, IFileSystemObject> children = new Dictionary<string, IFileSystemObject>();

            private CommandsDirectoryFileSystemObject commandsDirectory;

            private FilesDirectoryPath filesDirectory;

            public override bool IsFile => false;

            public override bool IsDirectory => true;

            public override bool CanRead => false;

            public override bool CanWrite => false;

            public override bool CanExecute => false;

            public override bool CanCreateFile => false;

            public override bool CanCreateDirectory => false;

            public override bool CanDeleteFile => false;

            public override bool CanDeleteDirectory => false;

            public override IReadOnlyDictionary<string, IFileSystemObject> Children => children;

            public string FilesStorage => filesDirectory.FilesStorage;

            public RootDirectory(IMyGridTerminalSystem gridTerminalSystem, string filesStorage) : base(string.Empty, null)
            {
                children.Add("apps", new AppsDirectoryFileSystemObject("apps", this, gridTerminalSystem));
                filesDirectory = new FilesDirectoryPath("files", this, filesStorage);
                children.Add("files", filesDirectory);
                commandsDirectory = new CommandsDirectoryFileSystemObject("commands", this);
                children.Add("commands", commandsDirectory);
            }

            public override IFileSystemObject CreateFile(string path) => null;

            public override IFileSystemObject CreateDirectory(string path) => null;

            public override bool DeleteChild(string name) => false;

            public override bool Execute(string argument, Console console) => false;

            public override bool Read(out string content)
            {
                content = string.Empty;
                return false;
            }

            public override bool Write(string content) => false;
        }

        private class Shell
        {
            private IFileSystemObject currentDirectoryPath;

            private string currentDirectory;

            public RootDirectory RootDirectory { get; private set; }

            public string FileStorage => RootDirectory.FilesStorage;

            public IFileSystemObject CurrentDirectoryPath
            {
                get
                {
                    return currentDirectoryPath;
                }
                private set
                {
                    currentDirectory = null;
                    currentDirectoryPath = ((value == null) ? RootDirectory : value);
                }
            }

            public string CurrentDirectory
            {
                get
                {
                    if (currentDirectory == null)
                    {
                        StringBuilder current_directory_string_builder = new StringBuilder();
                        IFileSystemObject path = CurrentDirectoryPath;
                        while (path != null)
                        {
                            current_directory_string_builder.Insert(0, path.Name + "/");
                            path = path.ParentFileSystemObject;
                        }
                        currentDirectory = current_directory_string_builder.ToString();
                        current_directory_string_builder.Clear();
                    }
                    return currentDirectory;
                }
            }

            public Shell(IMyGridTerminalSystem gridTerminalSystem, string filesStorage)
            {
                RootDirectory = new RootDirectory(gridTerminalSystem, filesStorage);
                CurrentDirectoryPath = RootDirectory;
            }

            public IFileSystemObject ChangeDirectory(string path)
            {
                IFileSystemObject ret = GetPath(path);
                if (ret != null)
                {
                    if (ret.IsDirectory)
                    {
                        CurrentDirectoryPath = ret;
                    }
                }
                return ret;
            }

            public IFileSystemObject GetPath(string path)
            {
                IFileSystemObject ret = CurrentDirectoryPath;
                if (path != null)
                {
                    string trimmed_path = path.Trim();
                    if (trimmed_path.Length > 0)
                    {
                        if (trimmed_path[0] == '/')
                        {
                            ret = RootDirectory;
                        }
                    }
                    string[] parts = trimmed_path.Split('/');
                    if (parts != null)
                    {
                        foreach (string part in parts)
                        {
                            switch (part)
                            {
                                case ".":
                                    break;
                                case "..":
                                    ret = ret.ParentFileSystemObject;
                                    break;
                                default:
                                    if (ret.Children.ContainsKey(part))
                                    {
                                        ret = ret.Children[part];
                                    }
                                    else
                                    {
                                        ret = null;
                                    }
                                    break;
                            }
                            if (ret == null)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        ret = CurrentDirectoryPath;
                    }
                }
                return ret;
            }

            public bool Execute(string command, Console console)
            {
                bool ret = false;
                if (command != null)
                {
                    string app_path = command.Split(' ')[0];
                    IFileSystemObject file_system_object = GetPath(app_path);
                    if (file_system_object != null)
                    {
                        if (file_system_object.CanExecute)
                        {
                            ret = file_system_object.Execute((command.Length > app_path.Length) ? command.Substring(app_path.Length) : string.Empty, console);
                        }
                    }
                }
                return ret;
            }
        }

        private class Console
        {
            private string input = string.Empty;

            private List<IMyTextSurfaceProvider> textSurfaceProviders = new List<IMyTextSurfaceProvider>();

            private IMyTextSurface[] textSurfaces = Array.Empty<IMyTextSurface>();

            protected IMyGridTerminalSystem GridTerminalSystem { get; private set; }

            protected IReadOnlyList<IMyTextSurface> TextSurfaces => textSurfaces;

            public Shell Shell { get; private set; }

            public void Update()
            {
                textSurfaceProviders.Clear();
                if (GridTerminalSystem != null)
                {
                    GridTerminalSystem.GetBlocksOfType(textSurfaceProviders, (text_surface_provider) => ((text_surface_provider is IMyTerminalBlock) ? (((IMyTerminalBlock)text_surface_provider).CustomData.Trim() == "EngineersOS") : false));
                    int text_surface_count = 0;
                    foreach (IMyTextSurfaceProvider text_surface_provider in textSurfaceProviders)
                    {
                        text_surface_count += text_surface_provider.SurfaceCount;
                    }
                    if (textSurfaces.Length != text_surface_count)
                    {
                        textSurfaces = new IMyTextSurface[text_surface_count];
                    }
                    int text_surface_index = 0;
                    foreach (IMyTextSurfaceProvider text_surface_provider in textSurfaceProviders)
                    {
                        for (int index = 0, count = text_surface_provider.SurfaceCount; index < count; index++)
                        {
                            textSurfaces[text_surface_index++] = text_surface_provider.GetSurface(index);
                        }
                    }
                }
            }

            public Console(IMyGridTerminalSystem gridTerminalSystem, string filesStorage)
            {
                Shell = new Shell(gridTerminalSystem, filesStorage);
                GridTerminalSystem = gridTerminalSystem;
            }

            public void Print(object obj)
            {
                if (obj != null)
                {
                    string output = obj.ToString();
                    foreach (IMyTextSurface text_surface in TextSurfaces)
                    {
                        text_surface.WriteText(output, true);
                    }
                }
            }

            public void PrintLine(object obj)
            {
                Print(obj);
                Print(Environment.NewLine);
            }

            public void ReadInput(string input)
            {
                if (input != null)
                {
                    Print(input);
                    if (input.Length > 0)
                    {
                        string[] lines = (this.input + input).Split('\n');
                        if (lines.Length > 0)
                        {
                            this.input = lines[lines.Length - 1];
                            for (int index = 0, length = lines.Length - 1; index < length; index++)
                            {
                                string command = lines[index];
                                if (Shell.Execute(command, this))
                                {
                                    Print("Executed \"");
                                    Print(command);
                                    PrintLine("\" successfully!");
                                }
                                else
                                {
                                    Print("Failed to execute \"");
                                    Print(command);
                                    PrintLine("\"!");
                                }
                            }
                        }
                    }
                }
            }

            public void Clear()
            {
                foreach (IMyTextSurface text_surface in TextSurfaces)
                {
                    text_surface.WriteText(string.Empty);
                }
            }
        }

        private class Input
        {
            private static IReadOnlyDictionary<string, char> characterInputDictionary = new Dictionary<string, char>
            {
                { "tab", '\t' },
                { "enter", '\n' },
                { "space", ' ' },
                { "question", '?' },
                { "quote", '"' },
                { "openbrackets", '(' },
                { "closebrackets", ')' },
                { "nummultiply", '*' },
                { "plus", '+' },
                { "numadd", '+' },
                { "comma", ',' },
                { "minus", '-' },
                { "numsubtract", '-' },
                { "period", '.' },
                { "numdecimal", '.' },
                { "numdivide", '/' },
                { "0", '0' },
                { "num0", '0' },
                { "1", '1' },
                { "num1", '1' },
                { "2", '2' },
                { "num2", '2' },
                { "3", '3' },
                { "num3", '3' },
                { "4", '4' },
                { "num4", '4' },
                { "5", '5' },
                { "num5", '5' },
                { "6", '6' },
                { "num6", '6' },
                { "7", '7' },
                { "num7", '7' },
                { "8", '8' },
                { "num8", '8' },
                { "9", '9' },
                { "num9", '9' },
                { "semicolon", ';' },
                { "a", 'a' },
                { "b", 'b' },
                { "c", 'c' },
                { "d", 'd' },
                { "e", 'e' },
                { "f", 'f' },
                { "g", 'g' },
                { "h", 'h' },
                { "i", 'i' },
                { "j", 'j' },
                { "k", 'k' },
                { "l", 'l' },
                { "m", 'm' },
                { "n", 'n' },
                { "o", 'o' },
                { "p", 'p' },
                { "q", 'q' },
                { "r", 'r' },
                { "s", 's' },
                { "t", 't' },
                { "u", 'u' },
                { "v", 'v' },
                { "w", 'w' },
                { "x", 'x' },
                { "y", 'y' },
                { "z", 'z' },
                { "backslash", '\\' },
                { "pipe", '|' },
                { "tilde", '~' }
            };

            private IMyProgrammableBlock programmableBlock;

            private Dictionary<string, object> lastInputs = new Dictionary<string, object>();

            private Dictionary<string, object> deltaInputs = new Dictionary<string, object>();

            private HashSet<string> removeLastInputs = new HashSet<string>();

            public Input(IMyProgrammableBlock programmableBlock)
            {
                this.programmableBlock = programmableBlock;
            }

            public string GetKeyboardInput()
            {
                string ret = string.Empty;
                Dictionary<string, object> inputs = programmableBlock.GetProperty("ControlModule.Inputs")?.As<Dictionary<string, object>>()?.GetValue(programmableBlock);
                if (inputs != null)
                {
                    deltaInputs.Clear();
                    foreach (string last_input_key in lastInputs.Keys)
                    {
                        if (!(inputs.ContainsKey(last_input_key)))
                        {
                            removeLastInputs.Add(last_input_key);
                        }
                    }
                    foreach (string remove_last_input_key in removeLastInputs)
                    {
                        lastInputs.Remove(remove_last_input_key);
                    }
                    removeLastInputs.Clear();
                    foreach (KeyValuePair<string, object> input in inputs)
                    {
                        if (lastInputs.ContainsKey(input.Key))
                        {
                            if (lastInputs[input.Key] != input.Value)
                            {
                                deltaInputs.Add(input.Key, input.Value);
                                lastInputs[input.Key] = input.Value;
                            }
                        }
                        else
                        {
                            deltaInputs.Add(input.Key, input.Value);
                            lastInputs.Add(input.Key, input.Value);
                        }
                    }
                    if (deltaInputs.Count > 0)
                    {
                        StringBuilder input_string_builder = new StringBuilder();
                        foreach (string key in deltaInputs.Keys)
                        {
                            if (characterInputDictionary.ContainsKey(key))
                            {
                                input_string_builder.Append(characterInputDictionary[key]);
                            }
                        }
                        ret = input_string_builder.ToString();
                        input_string_builder.Clear();
                    }
                }
                return ret;
            }
        }

        private Console console;

        private Input input;

        public EngineersOS()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        private void Main(string argument, UpdateType updateSource)
        {
            if (console == null)
            {
                console = new Console(GridTerminalSystem, Storage);
            }
            if (input == null)
            {
                input = new Input(Me);
            }
            console.Update();
            console.ReadInput(input.GetKeyboardInput());
        }

        private bool ShouldPause() => (Runtime.CurrentInstructionCount > 1000);
    }
}
