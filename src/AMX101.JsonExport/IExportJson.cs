namespace AMX101.JsonExport
{
    public interface IExportJson
    {
        void ExportClaimsToJson();
        void ExportStaticClaimsToJson();
        void ExportClaimValuesToJson();
        void ExportSourcesToJson();
        void ExportPostCodesToJson();
    }
}