using System.Collections.Generic;

namespace FileTape.Appenders
{
    public interface IAppender
    {
        void Append(IReadOnlyCollection<byte[]> records, byte[] customProperties = null);
    }
}
