using AMX101.Data;
using AMX101.LocalData;
using AMX101.Dto.Models;

namespace AMX101.JsonExport
{
    public class ExportJson : IExportJson
    {
        private string folder;
        private IDataRepository source;
        private JsonRepository target;
        private string region;

        public ExportJson(string srvr, string rgn, string fldr)
        {
            folder = fldr;
            region = rgn;
            source = new DataRepository(region, srvr);
            target = new JsonRepository(region, fldr);
        }

        public void ExportClaimsToJson()
        {
            var claims = source.GetClaims(region);
            target.Save(claims, Consts.Claims);
        }

        public void ExportStaticClaimsToJson()
        {
            var staticClaims = source.GetStaticClaims(region);
            target.Save(staticClaims, Consts.StaticClaims);
        }

        public void ExportClaimValuesToJson()
        {
            var claimValues = source.GetClaimValues(region);
            target.Save(claimValues, Consts.ClaimValues);
        }

        public void ExportSourcesToJson()
        {
            var sources = source.GetSources(region);
            target.Save(sources, Consts.Sources);
        }

        public void ExportPostCodesToJson()
        {
            var postCodes = source.GetPostCodes(region);
            target.Save(postCodes, Consts.PostCodes);
        }
    }
}


