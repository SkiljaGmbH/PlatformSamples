using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace SampleEventDrivenActivity
{
    public interface IEntryHelper
    {
        string Extension { get; set; }
        string ExtensionWithoutDot { get; }
        string FileName { get; set; }
        string DirectoryName { get; set; }
        Stream Open();
    }

    [DebuggerDisplay("{DirectoryName} - {FileName}")]
    public class EntryHelper : IEntryHelper
    {
        public EntryHelper(ZipArchiveEntry archiveEntry)
        {
            ArchiveEntry = archiveEntry;

            DirectoryName = Path.GetDirectoryName(archiveEntry.FullName);
            FileName = Path.GetFileName(archiveEntry.Name);
            Extension = Path.GetExtension(archiveEntry.Name);
        }

        public string Extension { get; set; }
        public string ExtensionWithoutDot => Extension?.Substring(1) ?? "";

        public string FileName { get; set; }

        public string DirectoryName { get; set; }

        public ZipArchiveEntry ArchiveEntry { get; private set; }

        public Stream Open()
        {
            return ArchiveEntry.Open();
        }
    }
}
