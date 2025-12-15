using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace thecalcify.Shared
{
    public class SharedMemoryQueue : IDisposable
    {
        private const int HEADER_SIZE = 8;

        private readonly int SLOT_SIZE;
        private readonly int SLOT_COUNT;
        private readonly int TOTAL_SIZE;

        private readonly string _backingPath;

        private MemoryMappedFile _mmf;
        private MemoryMappedViewAccessor _accessor;
        private bool _disposed;

        public SharedMemoryQueue(string name, int slotSize = 2048, int slotCount = 4096)
        {
            SLOT_SIZE = slotSize;
            SLOT_COUNT = slotCount;
            TOTAL_SIZE = HEADER_SIZE + SLOT_SIZE * SLOT_COUNT;

            _backingPath = @"C:\Users\Public\thecalcify\rtwqueue.bin";

            EnsureBackingFile();
            OpenMappedFile();
        }

        private void EnsureBackingFile()
        {
            try
            {
                var dir = Path.GetDirectoryName(_backingPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (!File.Exists(_backingPath) ||
                    new FileInfo(_backingPath).Length != TOTAL_SIZE)
                {
                    using (var fs = new FileStream(
                        _backingPath,
                        FileMode.Create,
                        FileAccess.ReadWrite,
                        FileShare.ReadWrite))
                    {
                        fs.SetLength(TOTAL_SIZE);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create backing file: " + ex.Message, ex);
            }
        }

        private void OpenMappedFile()
        {
            try
            {
                var fs = new FileStream(
                    _backingPath,
                    FileMode.Open,
                    FileAccess.ReadWrite,
                    FileShare.ReadWrite);

                _mmf = MemoryMappedFile.CreateFromFile(
                    fs,
                    null,              
                    TOTAL_SIZE,
                    MemoryMappedFileAccess.ReadWrite,
                    HandleInheritability.None,
                    false);

                _accessor = _mmf.CreateViewAccessor(0, TOTAL_SIZE, MemoryMappedFileAccess.ReadWrite);
            }
            catch (Exception ex)
            {
                throw new Exception("MMF Create Failed: " + ex.Message, ex);
            }
        }

        public void Reset()
        {
            EnsureNotDisposed();

            // Only one writer and one reader, so no lock is needed:
            _accessor.Write(0, 0); // head
            _accessor.Write(4, 0); // tail
        }


        public bool Write(byte[] data)
        {
            EnsureNotDisposed();

            if (data == null || data.Length == 0)
                return false;

            if (data.Length > SLOT_SIZE - 4)
                return false; // too large for slot

            int head = _accessor.ReadInt32(0);
            int tail = _accessor.ReadInt32(4);

            int nextTail = (tail + 1) % SLOT_COUNT;

            // ------------------------------------------
            // 💥 DROP OLDEST IF QUEUE IS FULL
            // ------------------------------------------
            if (nextTail == head)
            {
                // Queue full → advance head (drop oldest)
                head = (head + 1) % SLOT_COUNT;
                _accessor.Write(0, head);
            }

            // Write data to slot
            long offset = HEADER_SIZE + (long)tail * SLOT_SIZE;

            _accessor.Write(offset, data.Length);
            _accessor.WriteArray(offset + 4, data, 0, data.Length);

            // Publish new tail
            _accessor.Write(4, nextTail);

            return true;
        }

        public bool Read(int timeoutMs, out byte[] buffer)
        {
            EnsureNotDisposed();
            buffer = null;

            int head = _accessor.ReadInt32(0);
            int tail = _accessor.ReadInt32(4);

            if (head == tail)
                return false; // empty

            long offset = HEADER_SIZE + (long)head * SLOT_SIZE;

            int len = _accessor.ReadInt32(offset);
            if (len <= 0 || len > SLOT_SIZE - 4)
            {
                int badNext = (head + 1) % SLOT_COUNT;
                _accessor.Write(0, badNext);
                return false;
            }

            buffer = new byte[len];
            _accessor.ReadArray(offset + 4, buffer, 0, len);

            int nextHead = (head + 1) % SLOT_COUNT;
            _accessor.Write(0, nextHead);

            return true;
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SharedMemoryQueue));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try { _accessor?.Dispose(); } catch { }
            try { _mmf?.Dispose(); } catch { }
        }
    }
}
