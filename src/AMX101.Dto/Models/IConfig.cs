namespace AMX101.Dto.Models
{
    public interface IConfig
    {
        string Region { get; set; }
        string LocalDataFolder { get; set; }
        string[] Regions { get; set; }

    }
}
