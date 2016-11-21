using System;
using System.IO;
using System.Threading.Tasks;
using FileTape.PartitionMapping;
using ProtoBuf;

namespace FileTape.PartitionHeaderFormatters
{
    public class ProtoBufPartitionHeaderFormatter : IPartitionHeaderFormatter
    {
        static ProtoBufPartitionHeaderFormatter()
        {
            Instance = new ProtoBufPartitionHeaderFormatter();
        }

        public PartitionHeader Deserialize(Stream stream, int headerSize)
        {
            var data = new byte[headerSize];

            var readed = stream.Read(data, 0, headerSize);
            if (readed != headerSize)
            {
                throw new Exception("Wrong partition format");
            }

            using (var memoryStream = new MemoryStream(data))
            {
                var res = Serializer.Deserialize<PartitionHeader>(memoryStream);
                if (res.PartitionMap == null)
                {
                    return new PartitionHeader(res.PartitionId,new PartitionMapItem[0],res.CustomProperties);// Протобуф десериализует пустые коллекции в null
                }
                return res;
            }
        }

        public byte[] Serialize(PartitionHeader header)
        {
            using (var memoryStream = new MemoryStream())
            {
                Serializer.Serialize(memoryStream, header);
                return memoryStream.ToArray();
            }
        }

        public static ProtoBufPartitionHeaderFormatter Instance { get; private set; }
    }
}