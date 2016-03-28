using System;
using System.IO;
using FileTape.PartitionHeaderFormatters;

namespace FileTape.ReadCursors
{
    public class ReadPartitionCursorProvider:IReadPartitionCursorProvider
    {
        private readonly Func<Stream> _createStream;
        private readonly IPartitionHeaderFormatter _formatter;

        public ReadPartitionCursorProvider(Func<Stream> createStream, IPartitionHeaderFormatter formatter)
        {
            _createStream = createStream;
            _formatter = formatter;
        }

        public IReadPartitionCursor CreateCursor()
        {
            return new ReadPartitionCursor(_createStream(), _formatter);   
        }
    }
}