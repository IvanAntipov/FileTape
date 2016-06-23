using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileTape.Appenders;
using FileTape.PartitionHeaderFormatters;
using FileTape.PartitionMapping;
using FileTape.PartitionsEnumeration;
using Lz4Net;

namespace FileTape.Lz4
{

    public class DirectLz4Appender : DirectAppenderBase
    {
        private readonly Lz4Mode _mode;
        public DirectLz4Appender(
            string path, 
            IPartitionsEnumerator partitionsEnumerator, 
            IPartitionHeaderFormatter formatter,
            Lz4Net.Lz4Mode mode = Lz4Mode.Fast) 
            : base(
                path,
                partitionsEnumerator,
                formatter)
        {
            _mode = mode;

        }

        protected override string TmpFileName
        {
            get { return ".tmplz4.partition"; }
        }

        protected override Stream CreateWriteStream(string tmpFileName)
        {
            return new Lz4Net.Lz4CompressionStream(File.OpenWrite(tmpFileName), mode: _mode, closeStream: true);
        }
    }
}