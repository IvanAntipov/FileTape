using System.Collections.Generic;
using FileTape.ReadCursors;

namespace FileTape.PartitionsEnumeration
{
    public interface IPartitionsEnumerator
    {
        IEnumerable<IReadPartitionCursorProvider> GetCursors();
        string CreateFileNameForNextPartition();
    }
}
