using System.Threading.Tasks;

namespace FileTape.ReadCursors
{
    public interface IRecordsEnumerator
    {
        bool Next();
        byte[] Current { get; }
    }
}