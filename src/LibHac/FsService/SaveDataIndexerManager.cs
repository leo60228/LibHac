﻿using System.Threading;
using LibHac.Common;
using LibHac.Fs;

namespace LibHac.FsService
{
    internal class SaveDataIndexerManager
    {
        private FileSystemClient FsClient { get; }
        private ulong SaveDataId { get; }

        private IndexerHolder _bisIndexer = new IndexerHolder(new object());
        private IndexerHolder _sdCardIndexer = new IndexerHolder(new object());
        private IndexerHolder _tempIndexer = new IndexerHolder(new object());
        private IndexerHolder _safeIndexer = new IndexerHolder(new object());
        private IndexerHolder _properSystemIndexer = new IndexerHolder(new object());

        public SaveDataIndexerManager(FileSystemClient fsClient, ulong saveDataId)
        {
            FsClient = fsClient;
            SaveDataId = saveDataId;
        }

        public Result GetSaveDataIndexer(out SaveDataIndexerReader reader, SaveDataSpaceId spaceId)
        {
            switch (spaceId)
            {
                case SaveDataSpaceId.System:
                case SaveDataSpaceId.User:
                    Monitor.Enter(_bisIndexer.Locker);

                    if (!_bisIndexer.IsInitialized)
                    {
                        _bisIndexer.Indexer = new SaveDataIndexer(FsClient, "saveDataIxrDb".ToU8Span(), SaveDataSpaceId.System, SaveDataId);
                    }

                    reader = new SaveDataIndexerReader(_bisIndexer.Indexer, _bisIndexer.Locker);
                    return Result.Success;

                case SaveDataSpaceId.SdSystem:
                case SaveDataSpaceId.SdCache:
                    Monitor.Enter(_sdCardIndexer.Locker);

                    // todo: Missing reinitialize if SD handle is old

                    if (!_sdCardIndexer.IsInitialized)
                    {
                        _sdCardIndexer.Indexer = new SaveDataIndexer(FsClient, "saveDataIxrDbSd".ToU8Span(), SaveDataSpaceId.SdSystem, SaveDataId);
                    }

                    reader = new SaveDataIndexerReader(_sdCardIndexer.Indexer, _sdCardIndexer.Locker);
                    return Result.Success;

                case SaveDataSpaceId.Temporary:
                    Monitor.Enter(_tempIndexer.Locker);

                    if (!_tempIndexer.IsInitialized)
                    {
                        _tempIndexer.Indexer = new SaveDataIndexerLite();
                    }

                    reader = new SaveDataIndexerReader(_tempIndexer.Indexer, _tempIndexer.Locker);
                    return Result.Success;

                case SaveDataSpaceId.ProperSystem:
                    Monitor.Enter(_safeIndexer.Locker);

                    if (!_safeIndexer.IsInitialized)
                    {
                        _safeIndexer.Indexer = new SaveDataIndexer(FsClient, "saveDataIxrDbPr".ToU8Span(), SaveDataSpaceId.ProperSystem, SaveDataId);
                    }

                    reader = new SaveDataIndexerReader(_safeIndexer.Indexer, _safeIndexer.Locker);
                    return Result.Success;

                case SaveDataSpaceId.SafeMode:
                    Monitor.Enter(_properSystemIndexer.Locker);

                    if (!_properSystemIndexer.IsInitialized)
                    {
                        _properSystemIndexer.Indexer = new SaveDataIndexer(FsClient, "saveDataIxrDbSf".ToU8Span(), SaveDataSpaceId.SafeMode, SaveDataId);
                    }

                    reader = new SaveDataIndexerReader(_properSystemIndexer.Indexer, _properSystemIndexer.Locker);
                    return Result.Success;

                default:
                    reader = default;
                    return ResultFs.InvalidArgument.Log();
            }
        }

        internal void ResetSdCardIndexer()
        {
            lock (_sdCardIndexer.Locker)
            {
                _sdCardIndexer.Indexer = null;
            }
        }

        private struct IndexerHolder
        {
            public object Locker { get; }
            public ISaveDataIndexer Indexer { get; set; }

            public IndexerHolder(object locker)
            {
                Locker = locker;
                Indexer = null;
            }

            public bool IsInitialized => Indexer != null;
        }
    }

    public ref struct SaveDataIndexerReader
    {
        private bool _isInitialized;
        private object _locker;

        public ISaveDataIndexer Indexer;
        public bool IsInitialized => _isInitialized;

        internal SaveDataIndexerReader(ISaveDataIndexer indexer, object locker)
        {
            _isInitialized = true;
            _locker = locker;
            Indexer = indexer;
        }

        public void Dispose()
        {
            if (_isInitialized)
            {
                Monitor.Exit(_locker);

                _isInitialized = false;
            }
        }
    }
}
