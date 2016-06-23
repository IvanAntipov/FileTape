using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileTape.Appenders;
using FileTape.PartitionHeaderFormatters;
using FileTape.PartitionMapping;
using FileTape.PartitionsEnumeration;
using lz4;

namespace FileTape.Lz4
{

    public class DirectLz4Appender : DirectAppenderBase
    {
        
        public DirectLz4Appender(
            string path, 
            IPartitionsEnumerator partitionsEnumerator, 
            IPartitionHeaderFormatter formatter) 
            : base(
                path,
                partitionsEnumerator,
                formatter)
        {

        }

        protected override string TmpFileName
        {
            get { return ".tmplz4.partition"; }
        }

        protected override Stream CreateWriteStream(string tmpFileName)
        {
            return LZ4Stream.CreateCompressor(File.OpenWrite(tmpFileName),LZ4StreamMode.Write);
        }
    }
}