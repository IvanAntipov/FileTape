using System;
using System.IO;
using System.Threading.Tasks;
using FileTape.PartitionHeaderFormatters;
using FileTape.PartitionMapping;

namespace FileTape.ReadCursors
{
    public class ReadPartitionCursor : IReadPartitionCursor
    {
        /// <summary>
        /// Кол-во байт в начале файла которые определяют длинну Header
        /// </summary>
        private const int HeaderLengthBytesCount = 4;
        private readonly Stream _partitionStream;
        private readonly IPartitionHeaderFormatter _formatter;
        private bool _headerIsReaded = false;
        private PartitionHeader _header = null;

        /// <summary>
        /// Создает курсор по партиции.
        /// При унитожении курсора используемый поток закрывается.
        /// </summary>
        /// <param name="partitionStream"></param>
        /// <param name="formatter"></param>
        public ReadPartitionCursor(Stream partitionStream, IPartitionHeaderFormatter formatter)
        {
            _partitionStream = partitionStream;
            _formatter = formatter;
            Records = new RecordsEnumerator(new Lazy<PartitionHeader>(GetHeader),partitionStream);// TODO dispose cursor on exception
        }

        public void Dispose()
        {
            _partitionStream.Close();
        }

        public PartitionHeader GetHeader()
        {
            try
            {

                if (_headerIsReaded)
                {
                    return _header;
                }
                else
                {
                    var buff = new byte[HeaderLengthBytesCount];
                    
                    var headerSise = _partitionStream.Read(buff, 0, HeaderLengthBytesCount);
                    if (headerSise != HeaderLengthBytesCount)
                    {
                        throw new Exception("Wrong partition format");
                    }                   
                        
                    var headerLength = (buff[0] << 24) + (buff[1] << 16) + (buff[2] << 8) + buff[3];

                    var header = _formatter.Deserialize(_partitionStream,headerLength);

                    _header = header;
                    _headerIsReaded = true;
                    return header;

                }
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }


        public IRecordsEnumerator Records { get; private set; }
    }
}