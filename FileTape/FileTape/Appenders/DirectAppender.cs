using System.ComponentModel;
using System.IO;
using FileTape.PartitionHeaderFormatters;
using FileTape.PartitionsEnumeration;

namespace FileTape.Appenders
{
    public class DirectAppender : DirectAppenderBase
    {
        public DirectAppender(string path,
            IPartitionsEnumerator partitionsEnumerator,
            IPartitionHeaderFormatter formatter) 
            : base(path, partitionsEnumerator, formatter)
        {
        }

        protected override Stream CreateWriteStream(string tmpFileName)
        {
            return File.Create(tmpFileName);
        }

        protected override string TmpFileName
        {
            get { return ".tmp.partition"; }
        }

    }
}