using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AMX101.Dto.Enitites;
using AMX101.Dto.Models;
using AMX101.Site.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace AMX101.Site.Services
{
    public interface IImportService
    {
        List<string> ImportStaticClaimsData(string filename);
        List<string> ImportClaimsData(string claimsFilename, string postcodesFilename);
    }

    public class ImportService : IImportService
    {
        //private readonly DataContext _context;
        private readonly IHostingEnvironment _environment;
        private readonly IDataRepository _repository;
        public ImportService(IHostingEnvironment environment, IDataRepository repository)
        {
            _repository = repository;
            // _context = context;
            _environment = environment;
        }
        public List<string> ImportStaticClaimsData(string filename)
        {
            var errors = new List<string>();
            try
            {
                var path = Path.Combine(_environment.ContentRootPath, "importData", filename);
                var staticClaims = new List<StaticClaim>();
                using (TextReader reader = File.OpenText(path))
                {
                    var csv = new CsvReader(reader, new CsvConfiguration() { Encoding = Encoding.UTF8 });
                    List<StaticClaimRecord> records = csv.GetRecords<StaticClaimRecord>().ToList();

                    //now we go through and add them back into the database
                    foreach (var rec in records)
                    {
                        if (!IsStaticClaimValid(rec))
                        {
                            errors.Add($"ERROR: record is not valid row {records.IndexOf(rec)}");
                            continue;
                        }
                        var sc = new StaticClaim
                        {
                            Heading = rec.Heading,
                            Value = rec.Value,
                            ClaimText = rec.Text,
                            SourceId = GetOrAddSource(rec.Source),
                            Category = (Category)rec.Category,
                            ImagePath = rec.Imagepath
                        };

                        if (string.IsNullOrEmpty(sc.ClaimText)) sc.ClaimText = null;

                        if (string.IsNullOrEmpty(sc.ImagePath)) sc.ImagePath = null;
                        staticClaims.Add(sc);
                    }
                    _repository.SaveStaticClaims(staticClaims);
                }
            }
            catch (Exception ex)
            {
                errors.Add("FATAL ERROR:" + ex.Message);
            }

            return errors;
        }

        public List<string> ImportClaimsData(string claimsFilename, string postcodesFilename)
        {
            var errors = new List<string>();
            try
            {
                var claims = new List<Claim>();
                var values = new List<ClaimValue>();
                var claimsPath = Path.Combine(_environment.ContentRootPath, "ImportData", claimsFilename);
                var postcodesPath = Path.Combine(_environment.ContentRootPath, "ImportData", postcodesFilename);
                using (TextReader claimsReader = File.OpenText(claimsPath))
                using (TextReader postcodeReader = File.OpenText(postcodesPath))
                {
                    var claimsCsv = new CsvReader(claimsReader);
                    var postcodeCsv = new CsvReader(postcodeReader);
                    var claimRecords = claimsCsv.GetRecords<ImportClaim>().ToList();
                    var postcodeRecords = postcodeCsv.GetRecords<ImportPostcode>().ToList();

                    //now we go through and add the claims into the database
                    foreach (var rec in claimRecords)
                    {
                        if (!IsClaimValid(rec))
                        {
                            errors.Add($"ERROR: claim is not a valid row {claimRecords.IndexOf(rec)}");
                            continue;
                        }
                        var cl = new Claim
                        {
                            SourceId = GetOrAddSource(rec.Source),
                            Heading = rec.Heading,
                            ClaimText = rec.Text,
                            Industry = (Industry)rec.Industry,
                            Type = (Dto.Models.Type)rec.Type,
                            ImagePath = rec.Imagepath
                        };

                        if (string.IsNullOrEmpty(cl.ClaimText)) cl.ClaimText = null;

                        if (string.IsNullOrEmpty(cl.ImagePath)) cl.ImagePath = null;

                        claims.Add(cl);
                    }

                    _repository.SaveClaims(claims);

                    // NOTE: This is different in the other versions!!!
                    foreach (var rec in postcodeRecords)
                    {
                        var threshold = 0;
                        var headings = typeof(ImportPostcode).GetTypeInfo();
                        foreach (var heading in headings.DeclaredProperties)
                        {
                            try
                            {
                                var loweredHeading = heading.Name.ToLower();
                                if (loweredHeading.Contains("threshold"))
                                {
                                    PropertyInfo pinfo = rec.GetType().GetProperty(heading.Name);
                                    var val = (string)pinfo.GetValue(rec, null);
                                    if (val == "x")
                                    {
                                        switch (loweredHeading)
                                        {
                                            case "threshold1":
                                                threshold = 1;
                                                break;
                                            case "threshold2":
                                                threshold = 2;
                                                break;
                                            case "threshold3":
                                                threshold = 3;
                                                break;
                                            case "threshold4":
                                                threshold = 4;
                                                break;
                                        }
                                    }
                                }
                                else if (loweredHeading.Contains("cardsinforce"))
                                {
                                    var claim = 
                                        claims.FirstOrDefault(
                                            x => x.Heading.Replace(" ", "").ToLower() == "cardsinforce");
                                    if (claim != null)
                                    {
                                        var order =
                                            Convert.ToInt32(
                                                loweredHeading.Substring(loweredHeading.ToCharArray().Length - 1));

                                        var claimValue = new ClaimValue()
                                        {
                                            Postcode = rec.Postcode,
                                            Value = GetClaimValue(rec, heading.Name),
                                            ClaimId = claim.Id,
                                            Order = order,
                                            Threshold = threshold
                                        };

                                        values.Add(claimValue);
                                    }
                                }
                                else
                                {
                                    var claim =
                                        claims.FirstOrDefault(
                                            x => x.Heading.Replace(" ", "").ToLower() == loweredHeading);
                                    if (claim != null)
                                    {
                                        var claimValue = new ClaimValue()
                                        {
                                            Postcode = rec.Postcode,
                                            Value = GetClaimValue(rec, heading.Name),
                                            ClaimId = claim.Id,
                                            Order = 0,
                                            Threshold = threshold
                                        };

                                        values.Add(claimValue);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                errors.Add(
                                    $"Error trying to import heading {heading.Name} for postcode {rec.Postcode}: {ex.Message}");
                            }
                        }

                    }
                    // hopefully there is still enough memory left ;)
                    _repository.SaveClaimValues(values);
                }
            }
            catch (Exception ex)
            {
                errors.Add("FATAL ERROR:" + ex.Message);
            }
            return errors;
        }

        private int? GetOrAddSource(string text)
        {
            Source source = _repository.GetOrAddSource(text);
            return source?.Id;
        }

        private bool IsClaimValid(ImportClaim rec)
        {
            return !string.IsNullOrEmpty(rec.Heading);
        }

        private bool IsStaticClaimValid(StaticClaimRecord rec)
        {
            return !(string.IsNullOrEmpty(rec.Heading) ||
                     string.IsNullOrEmpty(rec.Value));
        }

        private long? GetClaimValue(ImportPostcode rec, string fieldName)
        {
            long val;
            PropertyInfo pinfo = rec.GetType().GetProperty(fieldName);
            var strVal = (string)pinfo.GetValue(rec, null);
            var trimmedStrVal = TrimAlpha(strVal);

            var attempt = long.TryParse(trimmedStrVal, out val);
            if (attempt)
            {
                return val;
            }
            return null;
        }

        private string TrimAlpha(string s)
        {
            var noDecimals = s.Split('.').ToArray().FirstOrDefault();
            return new string(noDecimals.Where(char.IsDigit).ToArray());
        }
    }
}