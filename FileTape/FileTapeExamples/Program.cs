using System;
using System.IO;
using System.Linq;
using System.Text;
using FileTape.Appenders;
using FileTape.PartitionHeaderFormatters;
using FileTape.PartitionsEnumeration;

namespace FileTapeExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileTapePath = "FileTapeExample";
            if (!Directory.Exists(fileTapePath))
            {
                Directory.CreateDirectory(fileTapePath);
            }

            Write(fileTapePath);
            Read(fileTapePath);
        }

        private static void Write(string fileTapePath)
        {
            
            var formatter = ProtoBufPartitionHeaderFormatter.Instance;
            IPartitionsEnumerator enumerator = new DirectoryPartitionsEnumerator(fileTapePath, formatter);


            IAppender appender = new DirectAppender(fileTapePath, enumerator, formatter);

            var data = Enumerable.Range(0, 100)
                .Select(i => string.Format("Data #{0}", i))
                .Select(Encoding.UTF8.GetBytes)
                .ToArray();

            appender.Append(data);
        }

        private static void Read(string fileTapePath)
        {
            var formatter = ProtoBufPartitionHeaderFormatter.Instance;
            IPartitionsEnumerator enumerator = new DirectoryPartitionsEnumerator(fileTapePath, formatter);

            var cursorProviers = enumerator.GetCursors();
            foreach (var provider in cursorProviers)
            {
                using (var cursor = provider.CreateCursor())
                {
                    var h = cursor.GetHeader();
                    Console.WriteLine("Count "+h.PartitionMap.Length);
                    while (cursor.Records.Next())
                    {                      
                        Console.WriteLine(Encoding.UTF8.GetString(cursor.Records.Current));
                    }
                }
            }

        }
    }
}
