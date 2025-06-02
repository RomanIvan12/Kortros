using SQLite;

namespace Kortros.ParamParser.Model
{
    public class ParamStack
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int CategoryItemId { get; set; }

        // Исходный параметр
        public bool IsBuiltInInit { get; set; }
        public string GuidInit { get; set; }
        public string NameInit { get; set; }
        public string DefinitionInit { get; set; }
        public string ParameterTypeInit { get; set; }
        public string StorageTypeInit { get; set; }
        public bool IsTypeInit { get; set; }

        // Целевой параметр
        public bool IsBuiltInTarg { get; set; }
        public string GuidTarg { get; set; }
        public string NameTarg { get; set; }
        public string DefinitionTarg { get; set; }
        public string ParameterTypeTarg { get; set; }
        public string StorageTypeTarg { get; set; }
        public bool IsTypeTarg { get; set; }

        public bool IsCorrect { get; set; }



        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
