using SQLite;


namespace Kortros.ParamParser.Model
{
    public class Config
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
