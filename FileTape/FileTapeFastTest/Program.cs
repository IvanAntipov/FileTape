using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DeepEqual.Syntax;
using FileTape.Appenders;
using FileTape.PartitionHeaderFormatters;
using FileTape.PartitionMapping;
using FileTape.PartitionsEnumeration;
using ProtoBuf;

namespace FileTapeFastTest
{
    class Program
    {
        static void Main(string[] args)
        {

            var dTmpFiletape = @"D://tmp/FileTape";
            var sw = new Stopwatch();
            sw.Start();

//            var sizes = new[]
//            {
////                512,
////                1024,
////                1024*4,
////                1024*64,
////                1024*512,
////                1024*1024,
////                1024*1024*10
//                1024*1024*50,
//                1024*1024*100
//            };
//            foreach (var size in sizes)
//            {
//                WithTimeMeasurement(() =>ReadFiles(dTmpFiletape,size),size.ToString());                
//            }
//            Append(dTmpFiletape);
//            ReadFiles(dTmpFiletape, 1024 * 1024);                

//            Read(dTmpFiletape);
            CopyFilm(dTmpFiletape);
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
        }

        private static void CopyFilm(string fileTapePath)
        {
            var original = @"D://tmp/film/film.mkv";
            var dstFile = @"D://tmp/film/film2.mkv";
            WithTimeMeasurement(
                () =>
                {
                    
                    var buff = new byte[1024*1024];
                    var rnd = new Random();
                    var formatter = ProtoBufPartitionHeaderFormatter.Instance;
                    IPartitionsEnumerator enumerator = new DirectoryPartitionsEnumerator(fileTapePath, formatter);


                    IAppender appender = new DirectAppender(fileTapePath, enumerator, formatter);
                    Console.WriteLine(DateTime.Now + "Read orig");
                    using (var readStream = File.OpenRead(original))
                    {
                        var chunk = new List<byte[]>();
                        var maxChunkSize = 100;
                        while (true)
                        {

                            var readed = readStream.Read(buff, 0, rnd.Next(1024  * 512, buff.Length));
                            if (readed == 0)
                                break;


                            var readedArr = new byte[readed];
                            Array.Copy(buff, 0, readedArr, 0, readed);

                            chunk.Add(readedArr);

                            if (chunk.Count >= maxChunkSize)
                            {

                                appender.Append(chunk);
                                chunk.Clear();
                            }

                        }
                        if (chunk.Any())
                        {
                            appender.Append(chunk);
                        }
                    }
                    Console.WriteLine(DateTime.Now+"WriteDst");
                    var cursors = enumerator.GetCursors();

                    using (var dstStream = File.OpenWrite(dstFile))
                    {

//                        
//                        cursors
//                            .AsParallel()
//                            .ForAll(
//                                c =>
//                                {
//                                    using (var cursor = c.CreateCursor())
//                                    {
//
//                                        while (cursor.Records.Next())
//                                        {
//                                            var record = cursor.Records.Current;
//                                        }
//                                    }
//                                });


                        foreach (var readPartitionCursor in cursors)
                        {
                            using (var cursor = readPartitionCursor.CreateCursor())
                            {

                                while (cursor.Records.Next())
                                {
                                    var record = cursor.Records.Current;
                                    dstStream.Write(record, 0, record.Length);
                                }
                            }
                        }


                    }
                },
                "Copy Film");

            Console.WriteLine(DateTime.Now + "MD5");
            var originalMd5 = GetMD5(original);

            var dstMd5 = GetMD5(dstFile);

            var equals = originalMd5 == dstMd5;

            Console.WriteLine(equals);
        }

        private static string GetMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return Encoding.Default.GetString(md5.ComputeHash(stream));
                }
            }
        }

        private static void WithTimeMeasurement(Action a, string desc)
        {
            var sw = new Stopwatch();
            sw.Start();
            a();
            sw.Stop();
            Console.WriteLine(desc+" "+sw.ElapsedMilliseconds);
        }

        private static void ReadFiles(string path, int len)
        {
            var buf = new byte[len];
            var all = new DirectoryInfo(path).GetFiles();
            int c = 0;
            long totalSize = 0;

            foreach (var fileInfo in all)
            {
                using (var stream = new FileStream(fileInfo.FullName,FileMode.Open,FileAccess.Read,FileShare.Read))
                {
                    while (stream.Read(buf, 0, buf.Length)>0)
                    {
                        totalSize += buf.Length;
                        c++;
                        if (c%1000 == 0) ;
//                            Console.WriteLine(
//                                string.Format(
//                                    "{0} {1} {2}",
//                                    buf.Length,
//                                    buf.LastOrDefault(),
//                                    c));
                    }
                }
            }
//            Console.WriteLine("Total "+totalSize);
        }

        private static void Read(string dTmpFiletape)
        {
            var formatter = ProtoBufPartitionHeaderFormatter.Instance;
            IPartitionsEnumerator enumerator = new DirectoryPartitionsEnumerator(dTmpFiletape, formatter);


            var c = 0;
            long totalSize = 0;
            var cursorProviers = enumerator.GetCursors();
            foreach (var provider in cursorProviers)
            {
                using (var cursor = provider.CreateCursor())
                {
                    var h = cursor.GetHeader();
                    while (cursor.Records.Next())
                    {
                        totalSize += cursor.Records.Current.Length;
                        c++;
                        if (c%1000 == 0)
                            Console.WriteLine(
                                string.Format(
                                    "{0} {1} {2}",
                                    cursor.Records.Current.Length,
                                    cursor.Records.Current.LastOrDefault(),
                                    c));
                    }
                }
            }
            Console.WriteLine("Total length "+totalSize);
        }

        private static void Append(string dTmpFiletape)
        {
            var rnd = new Random();
            var xRange = Enumerable.Range(0, 10000)
                .Select(i => (byte) i)
                .ToArray();
            var yRange = Enumerable.Range(0, 100000);
            var partitionsRange = Enumerable.Range(0, 10);
            var formatter = ProtoBufPartitionHeaderFormatter.Instance;
            IPartitionsEnumerator enumerator = new DirectoryPartitionsEnumerator(dTmpFiletape, formatter);
            

            IAppender appender = new DirectAppender(dTmpFiletape, enumerator, formatter);

            foreach (var part in partitionsRange)
            {
                Console.WriteLine(part);
                appender.Append(
                    yRange.Select(
                        y => xRange.Take(rnd.Next(xRange.Length))
                            .ToArray())
                        .ToArray(),
                    new byte[0]);
            }
        }
    }
}
