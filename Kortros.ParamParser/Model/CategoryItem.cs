using SQLite;

namespace Kortros.ParamParser.Model
{
    public class CategoryItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int ConfigId { get; set; }
        public string Name { get; set; } // Содержит имя вида "OST_{category}"
        public bool IsChecked { get; set; }
        public bool IsEnabled { get; set; } = true; // На случай если общего параметра нет
    }
}
