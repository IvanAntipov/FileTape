using System.IO;
using FileTape.PartitionHeaderFormatters;
using FileTape.ReadCursors;

namespace FileTape.PartitionsEnumeration
{
    public class DirectoryPartitionsEnumerator : DirectoryPartitionsEnumeratorBase
    {
        private readonly IPartitionHeaderFormatter _partitionHeaderFormatter;

        public DirectoryPartitionsEnumerator(string path, IPartitionHeaderFormatter partitionHeaderFormatter)
            :base(path)
        {
            _partitionHeaderFormatter = partitionHeaderFormatter;
        }

        protected override string DataFileExtension
        {
            get { return "ftd"; }
        }

        protected override IReadPartitionCursorProvider CreateCursor(FileInfo file)
        {
            return new ReadPartitionCursorProvider(file.OpenRead, _partitionHeaderFormatter);
        }
    }
}