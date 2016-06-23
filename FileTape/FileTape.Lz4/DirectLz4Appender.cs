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
        private readonly bool _verifyCheckSum;

        public DirectLz4Appender(
            string path, 
            IPartitionsEnumerator partitionsEnumerator, 
            IPartitionHeaderFormatter formatter,
            bool verifyCheckSum=true) 
            : base(
                path,
                partitionsEnumerator,
                formatter)
        {
            _verifyCheckSum = verifyCheckSum;
        }

        protected override string TmpFileName
        {
            get { return ".tmplz4.partition"; }
        }

        protected override Stream CreateWriteStream(string tmpFileName)
        {
            return LZ4Stream.CreateCompressor(File.OpenWrite(tmpFileName),LZ4StreamMode.Write);
        }

        protected override void Check(string tmpFileName)
        {
            if (_verifyCheckSum)
            {
                using (var stream = LZ4Stream.CreateDecompressor(File.OpenRead(tmpFileName), LZ4StreamMode.Read))
                {
                    var dummy = new byte[16000000];
                    while (stream.Read(dummy,0,dummy.Length) >0)
                    {
                    }
                }
            }
        }
    }
}