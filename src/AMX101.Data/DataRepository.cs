using System;
using System.Collections.Generic;
using System.Linq;
using AMX101.Dto.Enitites;
using AMX101.Dto.Models;

namespace AMX101.Data
{

    /// <summary>
    /// A temporary solution before we do the data access refactoring
    /// </summary>

    public class DataRepository : IDataRepository
    {
        private readonly string server;
        private readonly string region;

        public DataRepository(string rgn, string srvr = "")
        {
            server = srvr;
            region = rgn;
        }

        public ICollection<Source> GetSources(string region)
        {
            using (var context = new DataContext(region, server))
            {
                return context.Source.ToList();
            }
        }

        public ICollection<Claim> GetClaims(string region)
        {
            using (var context = new DataContext(region, server))
            {
                return context.Claim.ToList();
            }
        }

        public ICollection<StaticClaim> GetStaticClaims(string region)
        {
            using (var context = new DataContext(region, server))
            {
                return context.StaticClaim.ToList();
            }
        }

        public ICollection<ClaimValue> GetClaimValues(string region)
        {
            using (var context = new DataContext(region, server))
            {
                return context.ClaimValue.ToList();
            }
        }

        public ICollection<ClaimValue> GetClaimValues(string postcode, string region)
        {
            using (var context = new DataContext(region))
            {
                return context.ClaimValue.Where(x => x.Postcode == postcode).ToList();
            }
        }

        public ICollection<StaticClaim> GetStaticClaims(IEnumerable<int> ids, string region)
        {
            using (var context = new DataContext(region))
            {
                return context.StaticClaim.Where(x => ids.Any(y => y == x.Id)).ToList();
            }
        }

        public ICollection<Claim> GetClaims(IEnumerable<int> ids, string region)
        {
            using (var context = new DataContext(region))
            {
                return context.Claim.Where(x => ids.Any(y => y == x.Id)).ToList();
            }
        }

        public ICollection<PostCode> GetPostCodes(string region)
        {
            using (var context = new DataContext(region))
            {
                return context.PostCode.ToList();
            }
        }

        public Claim GetClaim(int id, string region)
        {
            using (var context = new DataContext(region))
            {
                return context.Claim.FirstOrDefault(x => x.Id == id);
            }
        }

        public ICollection<ClaimValue> GetClaimValuesByPrefix(string prefix, string region)
        {
            using (var context = new DataContext(region))
            {
                return context.ClaimValue.Where(x => x.Postcode.ToString().Substring(0, 2) == prefix).ToList();
            }
        }

        public ICollection<PostCode> SearchPostCodes(string query, string region)
        {
            using (var context = new DataContext(region))
            {
                var lowercaseQuery = query.ToLower();
                return context.PostCode
                    .Where(x => x.Postcode.Contains(lowercaseQuery) ||
                                x.Suburb.ToLower().Contains(lowercaseQuery)).ToList();

            }
        }

        public bool IsValidPostcode(string query, string region)
        {
            using (var context = new DataContext(region))
            {
                if (region == "sng")
                {
                    return context.ClaimValue.Any(x => x.Postcode.Substring(0, 2).Contains(query));
                }
                return context.ClaimValue.Any(x => x.Postcode == query);
            }
        }

        public void SaveClaims(IEnumerable<Claim> claims)
        {
            using (var context = new DataContext(region))
            {
                //first thing we need to do is wipe the old claims as they may have been updated
                context.ClaimValue.RemoveRange(context.ClaimValue);
                context.SaveChanges();
                context.StaticClaim.RemoveRange(context.StaticClaim);
                context.SaveChanges();

                foreach (var sc in claims)
                {
                    context.Claim.Add(sc);
                }
                context.SaveChanges();
            }
        }

        public void SaveClaimValues(IEnumerable<ClaimValue> claimValues)
        {
            using (var context = new DataContext(region))
            {
                //first thing we need to do is wipe the old claims as they may have been updated
                context.StaticClaim.RemoveRange(context.StaticClaim);
                context.SaveChanges();

                foreach (var cv in claimValues)
                {
                    context.ClaimValue.Add(cv);
                }
                context.SaveChanges();
            }
        }

        public void SaveStaticClaims(IEnumerable<StaticClaim> staticClaims)
        {
            using (var context = new DataContext(region))
            {                        
                //first thing we need to do is wipe the old claims as they may have been updated
                context.StaticClaim.RemoveRange(context.StaticClaim);
                context.SaveChanges();
                foreach (var sc in staticClaims)
                {
                    context.StaticClaim.Add(sc);
                }

            }
        }

        public Source GetOrAddSource(string text)
        {
            Source source = null;
            using (var context = new DataContext())
            {
                // add source, in case it's new one
                if (!string.IsNullOrEmpty(text))
                {
                    source = context.Source.FirstOrDefault(s => s.Text == text);
                    if (source == null)
                    {
                        var s = new Source
                        {
                            Text = text
                        };
                        context.Source.Add(s);
                        context.SaveChanges();

                        source = s;
                    }
                }
            }
            return source;
        }
    }
}
