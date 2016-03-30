using System.Collections.Generic;
using FileTape.ReadCursors;

namespace FileTape.Helpers
{
    public static class ReadCursorProviderExtensions
    {
        public static IReadOnlyCollection<byte[]> ReadAll(this IReadPartitionCursorProvider provider)
        {
            using (var cursor = provider.CreateCursor())
            {
                return cursor.ReadAll();
            }
        }
    }
}