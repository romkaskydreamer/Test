using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AMX101.Dto.Enitites;
using AMX101.Dto.Models;
using Newtonsoft.Json;

namespace AMX101.LocalData
{
    public class JsonRepository: IDataRepository
    {
        IConfig localConfig;
        private readonly Dictionary<string, RegionRepository> regionRepos = new Dictionary<string, RegionRepository>();

        // the parameterless constructor will be used mostly for DI
        public JsonRepository(IConfig config)
        {
            localConfig = config;
            InitRepository();
        }

        public JsonRepository(string region, string  dataFolder)
        {
            localConfig = new LocalConfig
            {
                Region = region,
                LocalDataFolder = dataFolder
            };
            InitRepository();
        }

        public ICollection<ClaimValue> GetClaimValues(string postcode, string region)
        {
            var repo = GetRegionRepo(region);
            return repo.GetClaimValues(postcode);
        }

        public ICollection<Source> GetSources(string region)
        {
            var repo = GetRegionRepo(region);
            return repo.Sources;
        }

        public ICollection<StaticClaim> GetStaticClaims(string region)
        {
            var repo = GetRegionRepo(region);
            return repo.StaticClaims;
        }

        public ICollection<StaticClaim> GetStaticClaims(IEnumerable<int> ids, string region)
        {
            var staticClaims = GetStaticClaims(region);
            return staticClaims.Where(staticClaim => ids.Any(i => staticClaim.Id == i)).ToList();
        }

        public ICollection<Claim> GetClaims(string region)
        {
            var repo = GetRegionRepo(region);
            return repo.Claims;
        }

        public ICollection<Claim> GetClaims(IEnumerable<int> ids, string region)
        {
            var claims = GetClaims(region);
            return claims.Where(claim => ids.Any(i => claim.Id == i)).ToList();
        }

        public ICollection<PostCode> GetPostCodes(string region)
        {
            var repo = GetRegionRepo(region);
            return repo.PostCodes;
        }

        public void Save(object data, string resource)
        {
            string str = JsonConvert.SerializeObject(data);
            File.WriteAllText(GetFileName(resource), str);
        }

        public ICollection<ClaimValue> GetClaimValues(string region)
        {
            // we do not need this one ...
            // if we do then will think how to do it efficiently
            throw new NotImplementedException();
        }

        private string GetFileName(string recource, string region = "")
        {
            if (string.IsNullOrEmpty(region))
            {
                region = localConfig.Region;
            }
            return Path.ChangeExtension(Path.Combine(localConfig.LocalDataFolder, recource + "_" + region), "json");
        }

        private RegionRepository GetRegionRepo(string region)
        {
            if (regionRepos.ContainsKey(region))
            {
               return regionRepos[region];
            }
            throw new Exception($"No local repository for {region}");
        }

        private void InitRepository()
        {
            foreach (var region in localConfig.Regions)
            {
                var repo = new RegionRepository();
                regionRepos.Add(region, repo);
                ReadClaims(repo);
                ReadPostCodes(repo);
                ReadSources(repo);
                ReadStaicClaims(repo);
                ReadClaimValues(repo);
            }
        }


        private void ReadPostCodes(RegionRepository repo)
        {
            repo.PostCodes = GetValuesFromJson<PostCode>(Consts.PostCodes);
        }

        private void ReadClaims(RegionRepository repo)
        {
            repo.Claims = GetValuesFromJson<Claim>(Consts.Claims);
        }

        private void ReadStaicClaims(RegionRepository repo)
        {
            repo.StaticClaims = GetValuesFromJson<StaticClaim>(Consts.StaticClaims);
        }

        public void ReadSources(RegionRepository repo)
        {
            repo.Sources = GetValuesFromJson<Source>(Consts.Sources);
        }

        private void ReadClaimValues(RegionRepository repo)
        {
            var allValues = GetValuesFromJson<ClaimValue>(Consts.ClaimValues);
            var values = new Dictionary<string, ICollection<ClaimValue>>();

            foreach (var v in allValues)
            {
                if (!values.ContainsKey(v.Postcode))
                {
                    values[v.Postcode] = new List<ClaimValue>();
                }
                values[v.Postcode].Add(v);
            }
            repo.Values = values;
        }

        private ICollection<T> GetValuesFromJson<T>(string resource)
        {
            string fullPath = GetFileName(resource);
            if (!File.Exists(fullPath))
            {
                return new List<T>();
            }
            string json = File.ReadAllText(fullPath);
            var data = JsonConvert.DeserializeObject<ICollection<T>>(json);
            return data;
        }

        public ICollection<ClaimValue> GetClaimValuesByPrefix(string prefix, string region)
        {
            var repo = GetRegionRepo(region);
            var values = new List<ClaimValue>();
            foreach (var dct in repo.Values)
            {
                if (dct.Key.Substring(0, 2) == prefix)
                {
                    values.AddRange(dct.Value);
                }
            }
            return values;
        }

        public Claim GetClaim(int id, string region)
        {
            var repo = GetRegionRepo(region);
            return repo.Claims.FirstOrDefault(c => c.Id == id);
        }

        public ICollection<PostCode> SearchPostCodes(string query, string region)
        {
            var repo = GetRegionRepo(region);
            var lowercaseQuery = query.ToLower();
            return repo.PostCodes.Where(x => x.Postcode.Contains(lowercaseQuery) ||
                            x.Suburb.ToLower().Contains(lowercaseQuery)).ToList();
        }

        public bool IsValidPostcode(string query, string region)
        {
            var repo = GetRegionRepo(region);
            if (region == "sng")
            {
                return repo.Values.Where(dct => dct.Key.Substring(0, 2).Contains(query)).Any(dct => dct.Value.Any());
            }
            return repo.GetClaimValues(query).Any();
                
        }

        public void SaveClaims(IEnumerable<Claim> claims)
        {
            throw new NotImplementedException();
        }

        public void SaveClaimValues(IEnumerable<ClaimValue> claimValues)
        {
            throw new NotImplementedException();
        }

        public void SaveStaticClaims(IEnumerable<StaticClaim> staticClaims)
        {
            throw new NotImplementedException();
        }

        public Source GetOrAddSource(string text)
        {
            throw new NotImplementedException();
        }
    }
}
