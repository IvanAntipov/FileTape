using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileTape.ReadCursors;

namespace FileTape.PartitionsEnumeration
{
    public abstract class DirectoryPartitionsEnumeratorBase:IPartitionsEnumerator
    {
        private readonly string _path;

        protected DirectoryPartitionsEnumeratorBase(string path)
        {
            _path = path;
        }

        protected abstract string DataFileExtension { get; }


        private string DataFileMask
        {
            get
            {
                return "*." + DataFileExtension;
            }
        }

        public IEnumerable<IReadPartitionCursorProvider> GetCursors()
        {
            var files = new DirectoryInfo(_path).GetFiles(DataFileMask, SearchOption.TopDirectoryOnly);

            var orderedFiles = files.Select(
                f => new
                {
                    File = f,
                    Number = int.Parse(GetFileNameWithoutExtension(f))
                })
                .OrderBy(i => i.Number);

            return orderedFiles.Select(f => CreateCursor(f.File));
        }

        public string CreateFileNameForNextPartition()
        {
            var files = new DirectoryInfo(_path).GetFiles(DataFileMask, SearchOption.TopDirectoryOnly);

            var last = files.Select(
                f => new
                {
                    Number = int.Parse(GetFileNameWithoutExtension(f))
                })
                .OrderByDescending(i => i.Number).FirstOrDefault();

            var number = last == null
                ? 0
                : last.Number + 1;

            return string.Format( number.ToString("D8") + "." + DataFileExtension);
        }

        protected abstract IReadPartitionCursorProvider CreateCursor(FileInfo file);

        private string GetFileNameWithoutExtension(FileInfo file)
        {
            var fullName = file.Name;
            var index = fullName.LastIndexOf("."+DataFileExtension,StringComparison.InvariantCultureIgnoreCase);
            if (index < 0)
            {
                throw new Exception("Wrong file name format "+file.FullName);
            }
            return fullName.Substring(0, index);
        }
    }
}