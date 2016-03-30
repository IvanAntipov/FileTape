using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileTape.ReadCursors;

namespace FileTape.Helpers
{
    public static class ReadCursorExtensions
    {
        public static IReadOnlyCollection<byte[]> ReadAll(this IReadPartitionCursor cursor)
        {
            var result = new List<byte[]>();
            while (cursor.Records.Next())
            {
                result.Add(cursor.Records.Current);
            }

            return result;
        }
        
    }
}
