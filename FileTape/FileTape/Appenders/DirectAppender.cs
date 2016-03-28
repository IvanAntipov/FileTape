using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using FileTape.PartitionHeaderFormatters;
using FileTape.PartitionMapping;
using FileTape.PartitionsEnumeration;

namespace FileTape.Appenders
{
    /// <summary>
    /// Store records as single partition
    /// 
    /// TODDO save data in back ground
    /// TODO use lock file to prevent multi thread writes
    /// </summary>
    public class DirectAppender : IAppender
    {
        private readonly string _path;
        private readonly IPartitionsEnumerator _partitionsEnumerator;
        private readonly IPartitionHeaderFormatter _formatter;
        private const string TempFileName = ".tmp.partition";
            
        public DirectAppender(string path, IPartitionsEnumerator partitionsEnumerator, IPartitionHeaderFormatter formatter)
        {
            _path = path;
            _partitionsEnumerator = partitionsEnumerator;
            _formatter = formatter;
        }

        public void Append(IReadOnlyCollection<byte[]> records, byte[] customProperties=null)
        {
            
            var tmpFile = Path.Combine(_path, TempFileName);
            using (var file = File.OpenWrite(tmpFile))
            {
                var map = records.Select(r => new PartitionMapItem(r.Length)).ToArray();
                var header = new PartitionHeader(Guid.NewGuid(), map,customProperties==null?new List<byte[]>() : new List<byte[]>{customProperties});
                var serializedHeader = _formatter.Serialize(header);
                var headerLength = serializedHeader.Length;
                var headerLengthArr = new byte[]
                {
                    (byte) (headerLength >> 24),
                    (byte) (headerLength >> 16),
                    (byte) (headerLength >> 8),
                    (byte) headerLength
                };


                file.Write(headerLengthArr,0,headerLengthArr.Length);
                file.Write(serializedHeader,0,serializedHeader.Length);

                foreach (var record in records)
                {
                    file.Write(record,0,record.Length);
                }
            }

            var newPartitionName = _partitionsEnumerator.CreateFileNameForNextPartition();
            File.Move(tmpFile,Path.Combine(_path,newPartitionName));
        }
    }
}