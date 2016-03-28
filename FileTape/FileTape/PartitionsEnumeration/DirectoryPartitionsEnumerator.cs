using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileTape.PartitionHeaderFormatters;
using FileTape.ReadCursors;

namespace FileTape.PartitionsEnumeration
{
    public class DirectoryPartitionsEnumerator : IPartitionsEnumerator
    {
        private const string DataFileExtension = "ftd";
        private const string DataFileMask = "*."+DataFileExtension;
        private readonly string _path;
        private readonly IPartitionHeaderFormatter _partitionHeaderFormatter;

        public DirectoryPartitionsEnumerator(string path, IPartitionHeaderFormatter partitionHeaderFormatter)
        {
            _path = path;
            _partitionHeaderFormatter = partitionHeaderFormatter;
        }

        public IEnumerable<IReadPartitionCursorProvider> GetCursors()
        {
            var files = new DirectoryInfo(_path).GetFiles(DataFileMask, SearchOption.TopDirectoryOnly);

            var orderedFiles = files.Select(
                f => new
                {
                    File = f,
                    Number = int.Parse(Path.GetFileNameWithoutExtension(f.FullName))
                })
                .OrderBy(i => i.Number);

            return orderedFiles.Select(f => new ReadPartitionCursorProvider(f.File.OpenRead, _partitionHeaderFormatter));
        }

        public string CreateFileNameForNextPartition()
        {
            var files = new DirectoryInfo(_path).GetFiles(DataFileMask, SearchOption.TopDirectoryOnly);

            var last = files.Select(
                f => new
                {
                    Number = int.Parse(Path.GetFileNameWithoutExtension(f.FullName))
                })
                .OrderByDescending(i => i.Number).FirstOrDefault();

            var number = last == null
                ? 0
                : last.Number + 1;

            return number + "." + DataFileExtension;
        }
    }
}