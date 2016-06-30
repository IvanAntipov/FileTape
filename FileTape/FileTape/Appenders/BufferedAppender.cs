using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;

namespace FileTape.Appenders
{
    public class BufferedAppender: IAppender,IDisposable
    {
        private readonly IAppender _underlingAppender;
        private readonly int? _bufferByteSizeLimit;
        private readonly int? _bufferRecordCountLimit;
        private readonly int? _bufferChunkCountLimit;
        private readonly Action<Exception> _onError;
        private readonly bool _backGround;
        private readonly ConcurrentQueue<BufferedItem> _bufferedItems = new ConcurrentQueue<BufferedItem>();
        private int _isRunned = 0;
        private long _stamp = 0;
        public BufferedAppender(            
            IAppender underlingAppender,
            int? bufferByteSizeLimit,
            int? bufferRecordCountLimit,
            int? bufferChunkCountLimit,
            Action<Exception> onError, 
            bool backGround=true)
        {
            _underlingAppender = underlingAppender;
            _bufferByteSizeLimit = bufferByteSizeLimit;
            _bufferRecordCountLimit = bufferRecordCountLimit;
            _bufferChunkCountLimit = bufferChunkCountLimit;
            _onError = onError;
            _backGround = backGround;

            if ((bufferByteSizeLimit ==null)&& (bufferRecordCountLimit ==null)&& (bufferChunkCountLimit == null))
            {
                throw new ArgumentException("You should specify at least one limit");
            }
        }

        public void Flush()
        {
            var spin = new SpinWait();
            while (true)
            {

                var allocated = WithStampGuard(TryAllocateWriteLoop);
                if (allocated.Result)
                {
                    StartWriteLoopSync(true);
                    return;
                }
                spin.SpinOnce();
            }
        }

        public void Append(IReadOnlyCollection<byte[]> records, byte[] customProperties = null)
        {
            var spin = new SpinWait();
            _bufferedItems.Enqueue(new BufferedItem(records,customProperties));
            while (true)
            {
                
                var allocated = WithStampGuard(TryAllocateWriteLoop);
                if (allocated.Result)
                {
                    if (_backGround)
                    {
                        StartWriteLoopAsync();
                    }
                    else
                    {
                        StartWriteLoopSync(false);                        
                    }
                    return;
                }
                if (allocated.StapmNotChanged)
                {
                    return;
                }
                spin.SpinOnce();
            }

        }

        public void Dispose()
        {
            Flush();
        }

        public static byte[] SerializeCustomPropertiesArray(byte[][] customProperties)
        {
            using (var memoryStream = new MemoryStream())
            {
                Serializer.Serialize(memoryStream, customProperties);
                return memoryStream.ToArray();
            }
        }

        public static byte[][] DeserializeCutomPropertiesArray(byte[] customProperties)
        {
            using (var s = new MemoryStream(customProperties))
            {
                return Serializer.Deserialize<byte[][]>(s);
            }
        }

        private GuardResult<T> WithStampGuard<T>(Func<T> a)
        {
            var t1 = Interlocked.Read(ref _stamp);
            T res;
            long t2;
            try
            {
                res = a();
            }
            finally
            {
                t2 = Interlocked.CompareExchange(ref _stamp,t1+1,t1);                
            }

            return new GuardResult<T>
            {
                Result = res,
                StapmNotChanged = t2 == t1
            };
        }

        private void StartWriteLoopAsync()
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        StartWriteLoopSync(false);
                    }
                    catch (Exception)
                    {
                    }
                });
        }
        private void StartWriteLoopSync(bool flush)
        {
            var spinWait = new SpinWait();
            while (true)
            {
                if (NeedWrite(flush))
                {
                    try
                    {
                        WriteBuffer();
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            _onError(ex);
                        }
                        catch (Exception)
                        {
                            ReleaseWriteLoop();
                        }
                        throw;
                    }
                }
                else
                {
                    var res = WithStampGuard<object>(
                        () =>
                        {
                            ReleaseWriteLoop();
                            return null;
                        }
                        );
                    if (res.StapmNotChanged)
                    {
                        return;
                    }
                    var loopRealocated = TryAllocateWriteLoop();
                    if (!loopRealocated)
                    {
                        return;
                    }
                    spinWait.SpinOnce();
                }
            }
            
        }

        private bool NeedWrite(bool flush)
        {
            if (flush && _bufferedItems.Count > 0)
            {
                return true;
            }
            if (_bufferChunkCountLimit.HasValue && _bufferChunkCountLimit <= _bufferedItems.Count)
            {
                return true;
            }
            if (_bufferRecordCountLimit.HasValue && _bufferRecordCountLimit <= _bufferedItems.Sum(i => i.Records.Count))
            {
                return true;
            }
            if (_bufferByteSizeLimit.HasValue && _bufferByteSizeLimit <= _bufferedItems.Sum(i => i.TotalLength))
            {
                return true;
            }
            return false;

        }
        private bool TryAllocateWriteLoop()
        {
            return Interlocked.CompareExchange(ref _isRunned, 1, 0) == 0;
        }

        private void ReleaseWriteLoop()
        {
            Interlocked.Exchange(ref _isRunned, 0);
        }
        private struct GuardResult<T>
        {
            public bool StapmNotChanged;
            public T Result;
        }
        private void WriteBuffer()
        {
            List<BufferedItem> items =new List<BufferedItem>();
            BufferedItem item;
            while (_bufferedItems.TryDequeue(out item))
            {
                items.Add(item);
            }
            if (items.Any())
            {
                var customProperties = items
                    .Select(i => i.CustomProperties)
                    .Where(i => i !=null)
                    .ToArray();
                var records = items.SelectMany(i => i.Records)
                    .ToArray();
                _underlingAppender.Append(records, SerializeCustomPropertiesArray(customProperties));
            }
        }

        private class BufferedItem
        {
            public IReadOnlyCollection<byte[]> Records { get; private set; }
            public byte[] CustomProperties { get; private set; }
            public long TotalLength { get; private set; }
            public BufferedItem(IReadOnlyCollection<byte[]> records, byte[] customProperties)
            {
                Records = records;
                CustomProperties = customProperties;
                TotalLength =records.Sum(i => (long)i.Length) + ((customProperties!=null)?(long)customProperties.Length:0);
            }
        }
    }
}