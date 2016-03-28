using System;
using System.IO;
using System.Threading.Tasks;
using FileTape.PartitionMapping;

namespace FileTape.ReadCursors
{
    class RecordsEnumerator : IRecordsEnumerator
    {
        private readonly Lazy<PartitionHeader> _partitionHeader;
        private readonly Stream _stream;
        private bool _currentIsValid = false;
        private byte[] _current = null;
        private int _currentPosition = -1;

        public RecordsEnumerator(Lazy<PartitionHeader> partitionHeader, Stream stream)
        {
            _partitionHeader = partitionHeader;
            _stream = stream;
        }

        public bool Next()
        {
            _currentPosition += 1;

            bool res;
            var partitionHeader = _partitionHeader.Value;
            if (_currentPosition >= partitionHeader.PartitionMap.Length)
            {
                res = false;
            }
            else
            {
                var mapItem = partitionHeader.PartitionMap[_currentPosition];


                
                var data = new byte[mapItem.Length];
                var readed = _stream.Read(data,0,mapItem.Length);// TODO use look ahead cursor for small records
                if (readed != mapItem.Length)
                {
                    throw new Exception("Partition is broken");
                }

                _current = data;
                res = true;
            }

            _currentIsValid = res;
            return res;
        }

        public byte[] Current
        {
            get
            {
                if (!_currentIsValid)
                {
                    throw new InvalidOperationException();
                }

                return _current;
            }
        }
    }
}