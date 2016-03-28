using System;
using System.Threading.Tasks;
using FileTape.PartitionMapping;

namespace FileTape.ReadCursors
{
    public interface IReadPartitionCursor : IDisposable
    {
        PartitionHeader GetHeader();
        IRecordsEnumerator Records{get;}
    }
}