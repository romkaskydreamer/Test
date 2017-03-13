using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using AMX101.Dto.Enitites;
using AMX101.Dto.Models;
using Microsoft.EntityFrameworkCore;

namespace AMX101.Data
{
    public class DataContext : DbContext
    {
        string ConnectionString => string.Format(conn, server, region);

        //ToDO I want to figure out how to get these in app settings 
        // we have left as is so far until discuss it, just a small refactoring to get it easear to swith servers

        string conn = "Server={0};Database=amx101_{1}_db;User Id=dev;Password=dev;MultipleActiveResultSets=true";
        //static string conn = "Server={0};Database=amx101_{1}_db;User Id=sa;Password=!QAZ2wsx;MultipleActiveResultSets=true";
        
        //string server = @"air.set.local\inst6";
        string server = @"STUGA-7";
        // string server = "192.168.1.153";
        string region; // perhaps we don't need it anymore and have to replace with Config.Region?

        IConfig config;
        IConfig Config => config ?? (config = new LocalConfig());

        public DataContext(IConfig cnf)
        {
            config = cnf;
            region = Config.Regions[0];
        }

        public DataContext(string rgn = "")
        {
            if (string.IsNullOrEmpty(rgn))
            {
                rgn = Config.Regions[0]; // the default region is not gonna change;
            }
            if (Config.Regions.Contains(rgn))
            {
                region = rgn;
            }
        }
        public DataContext(string rgn, string srv):this(rgn)
        {
            if (!string.IsNullOrEmpty(srv))
            {
                server = srv;
            }
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString);

        }

        public virtual DbSet<PostCode> PostCode {get; set;}
        public virtual DbSet<ClaimValue> ClaimValue { get; set; }
        public virtual DbSet<Claim> Claim { get; set; }
        public virtual DbSet<StaticClaim> StaticClaim { get; set; }
        public virtual DbSet<Source> Source { get; set; }
    }
    /*
    [Table("PostCode")]
    public class PostCodeDb
    {

        public int Id { get; set; }
        [NotMapped]
        public int Postcode { get; set; }

        [Column("Postcode")]
        public string PostcodeC { get; set; }

        public string Suburb { get; set; }
        public string State { get; set; }
    }
    
     
             public PostCode(PostCodeInt pci)
        {
            Id = pci.Id;
            Postcode = pci.Postcode.ToString();
            State = pci.State;
            Suburb = pci.Suburb;
        }

     */
}