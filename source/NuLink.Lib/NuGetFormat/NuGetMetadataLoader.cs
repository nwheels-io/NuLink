using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuLink.Lib.Abstractions;
using NuLink.Lib.Workspaces;

namespace NuLink.Lib.NuGetFormat
{
    public class NuGetMetadataLoader
    {
        private readonly IUserInterface _ui;
        private readonly IImmutableEnvironment _environment;

        public NuGetMetadataLoader(IUserInterface ui, IImmutableEnvironment environment)
        {
            _ui = ui;
            _environment = environment;
        }

        public async Task<PackageMetadata> DownloadMetadata(PackageReference reference)
        {
            var url = $"https://api.nuget.org/v3/registration3/{reference.Id.ToLower()}/index.json";
            var json = await _environment.DownloadUrlAsText(url);
            var parsed = JObject.Parse(json);
            var catalogEntry = FindCatalogEntry();

            var metadata = new PackageMetadata(
                reference: reference,
                sourceRepoType: "git",
                sourceRepoUrl: catalogEntry?["projectUrl"]?.Value<string>()
            );

            return metadata;

            JObject FindCatalogEntry()
            {
                var entry = parsed.SelectToken(
                    $"$..[?(@['@type']=='PackageDetails' && @.version=='{reference.Version}')]"
                );

                return entry as JObject;
            }
        }
    }
}