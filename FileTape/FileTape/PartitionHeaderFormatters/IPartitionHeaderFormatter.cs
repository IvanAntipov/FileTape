using System.IO;
using System.Threading.Tasks;
using FileTape.PartitionMapping;

namespace FileTape.PartitionHeaderFormatters
{

    public interface IPartitionHeaderFormatter
    {
        PartitionHeader Deserialize(Stream stream, int headerSize);
        byte[] Serialize(PartitionHeader header);

    }
}