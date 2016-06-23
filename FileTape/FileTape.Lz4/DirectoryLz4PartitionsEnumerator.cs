using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileTape.PartitionHeaderFormatters;
using FileTape.PartitionsEnumeration;
using FileTape.ReadCursors;

namespace FileTape.Lz4
{
    public class DirectoryLz4PartitionsEnumerator : DirectoryPartitionsEnumeratorBase
    {
        private readonly IPartitionHeaderFormatter _partitionHeaderFormatter;

        public DirectoryLz4PartitionsEnumerator(string path, IPartitionHeaderFormatter partitionHeaderFormatter)
            : base(path)
        {
            _partitionHeaderFormatter = partitionHeaderFormatter;
        }

        protected override string DataFileExtension
        {
            get { return "ftd.lz4"; }
        }

        protected override IReadPartitionCursorProvider CreateCursor(FileInfo file)
        {
            return 
                new ReadPartitionCursorProvider(
                    () =>
                        new Lz4Net.Lz4DecompressionStream(
                            file.OpenRead(),
                            true),
                        _partitionHeaderFormatter);
        }
    }            
}
