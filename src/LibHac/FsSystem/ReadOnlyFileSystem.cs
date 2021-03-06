﻿using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;

namespace LibHac.FsSystem
{
    public class ReadOnlyFileSystem : IFileSystem
    {
        private IFileSystem BaseFs { get; }

        public ReadOnlyFileSystem(IFileSystem baseFileSystem)
        {
            BaseFs = baseFileSystem;
        }

        protected override Result DoOpenDirectory(out IDirectory directory, U8Span path, OpenDirectoryMode mode)
        {
            return BaseFs.OpenDirectory(out directory, path, mode);
        }

        protected override Result DoOpenFile(out IFile file, U8Span path, OpenMode mode)
        {
            file = default;

            Result rc = BaseFs.OpenFile(out IFile baseFile, path, mode);
            if (rc.IsFailure()) return rc;

            file = new ReadOnlyFile(baseFile);
            return Result.Success;
        }

        protected override Result DoGetEntryType(out DirectoryEntryType entryType, U8Span path)
        {
            return BaseFs.GetEntryType(out entryType, path);
        }

        protected override Result DoGetFreeSpaceSize(out long freeSpace, U8Span path)
        {
            freeSpace = 0;
            return Result.Success;

            // FS does:
            // return ResultFs.UnsupportedOperationReadOnlyFileSystemGetSpace.Log();
        }

        protected override Result DoGetTotalSpaceSize(out long totalSpace, U8Span path)
        {
            return BaseFs.GetTotalSpaceSize(out totalSpace, path);

            // FS does:
            // return ResultFs.UnsupportedOperationReadOnlyFileSystemGetSpace.Log();
        }

        protected override Result DoGetFileTimeStampRaw(out FileTimeStampRaw timeStamp, U8Span path)
        {
            return BaseFs.GetFileTimeStampRaw(out timeStamp, path);

            // FS does:
            // return ResultFs.NotImplemented.Log();
        }

        protected override Result DoCommit()
        {
            return Result.Success;
        }

        protected override Result DoCreateDirectory(U8Span path) => ResultFs.UnsupportedOperationModifyReadOnlyFileSystem.Log();

        protected override Result DoCreateFile(U8Span path, long size, CreateFileOptions options) => ResultFs.UnsupportedOperationModifyReadOnlyFileSystem.Log();

        protected override Result DoDeleteDirectory(U8Span path) => ResultFs.UnsupportedOperationModifyReadOnlyFileSystem.Log();

        protected override Result DoDeleteDirectoryRecursively(U8Span path) => ResultFs.UnsupportedOperationModifyReadOnlyFileSystem.Log();

        protected override Result DoCleanDirectoryRecursively(U8Span path) => ResultFs.UnsupportedOperationModifyReadOnlyFileSystem.Log();

        protected override Result DoDeleteFile(U8Span path) => ResultFs.UnsupportedOperationModifyReadOnlyFileSystem.Log();

        protected override Result DoRenameDirectory(U8Span oldPath, U8Span newPath) => ResultFs.UnsupportedOperationModifyReadOnlyFileSystem.Log();

        protected override Result DoRenameFile(U8Span oldPath, U8Span newPath) => ResultFs.UnsupportedOperationModifyReadOnlyFileSystem.Log();
    }
}
