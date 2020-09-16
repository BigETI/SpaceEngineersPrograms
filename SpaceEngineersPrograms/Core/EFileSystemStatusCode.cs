namespace SpaceEngineersPrograms
{
    public enum EFileSystemStatusCode
    {
        MissingArguments,

        InvalidArguments,

        IsFileMissing,

        IsDirectoryMissing,

        IsFile,

        IsNotFile,

        IsDirectory,

        IsNotDirectory,

        IsPath,

        IsNotPath,

        ChangedDirectory,

        FailedChangingDirectory,

        CreatedDirectory,

        FailedCreatingDirectory,

        CreatedFile,

        FailedCreatingFile,

        WrittenFileContents,

        FailedWritingFileContents,

        AppendedFileContents,

        FailedAppendingFileContents,

        DeletedPath,

        FailedDeletingPath,

        SavedState,

        ShownHelpTopic,

        CustomData,

        UnknownError
    }
}
