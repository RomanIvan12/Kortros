using System.Collections.Generic;

namespace RsnModelReloader
{
    public class RootContent
    {
        public string Path { get; set; }
        public long DriveFreeSpace { get; set; }
        public long DriveSpace { get; set; }
        public List<File> Files { get; set; }
        public List<Folder> Folders { get; set; }
        public object LockContext { get; set; }
        public int LockState { get; set; }
        public object ModelLocksInProgress { get; set; }
        public List<Model> Models { get; set; }

        // Добавляю FolderInfo
    }
    public class File
    {
        public bool IsText { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }
    }

    public class Folder // является root по умолчанию
    {
        public bool HasContents { get; set; }
        public object LockContext { get; set; }
        public int LockState { get; set; }
        public object ModelLocksInProgress { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
    }
    public class Model
    {
        public object LockContext { get; set; }
        public int LockState { get; set; }
        public object ModelLocksInProgress { get; set; }
        public int ModelSize { get; set; }
        public string Name { get; set; }
        public int ProductVersion { get; set; }
        public int SupportSize { get; set; }

        public HistoryResponse HistoryResponse { get; set;} 
    }

    public class HistoryResponse
    {
        public string Path { get; set; }
        public List<HistoryItem> Items { get; set; }
    }

    public class HistoryItem
    {
        public string Comment { get; set; }
        public string Date { get; set; }
        public long ModelSize { get; set; }
        public long SupportSize { get; set; }
        public string User { get; set; }
        public int VersionNumber { get; set; }
    }
}
