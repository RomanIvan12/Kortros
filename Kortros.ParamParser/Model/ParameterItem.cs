using SQLite;

namespace Kortros.ParamParser.Model
{
    public class ParameterItemCommon
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string Category { get; set; }
        public bool IsBuiltIn { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public string Definition { get; set; }
        public string ParameterType { get; set; }
        public string StorageType { get; set; }
        public bool IsType { get; set; }
        public bool IsReadOnly { get; set; }
    }

    public class ParameterItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string Category { get; set; }
        public bool IsBuiltIn { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public string Definition { get; set; }
        public string ParameterType { get; set; }
        public string StorageType { get; set; }
        public bool IsType { get; set; }
        public bool IsReadOnly { get; set; }
    }
}
