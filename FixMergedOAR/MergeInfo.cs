using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FixMergedOAR
{
    public class MergeInfo
    {
        public ISet<string> MergePaths { get; } = new HashSet<string>();
        public IDictionary<string, string> MergedPlugins { get; } = new Dictionary<string, string>( 
            StringComparer.InvariantCultureIgnoreCase);
        public IDictionary<string, IDictionary<string, string>> MappedFormIds { get; } = new Dictionary<string, IDictionary<string, string>>(
            StringComparer.InvariantCultureIgnoreCase);

        public MergeInfo(string inputFolder)
        {
            // inventory merge candidates in the current directory
            EnumerationOptions options = new EnumerationOptions { RecurseSubdirectories = true };
            foreach (string mergePath in Directory.GetDirectories(inputFolder, "merge - *", options))
            {
                // check for plugin list and formid mapping
                using (StreamReader reader = File.OpenText(mergePath + "/merge.json"))
                {
                    JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                    if (!o.ContainsKey("filename"))
                        continue;
                    JValue mergeResult = (JValue)o["filename"]!;
                    if (!o.ContainsKey("plugins"))
                        continue;
                    foreach (JObject plugin in (JArray)o["plugins"]!)
                    {
                        if (!plugin.ContainsKey("filename"))
                            continue;
                        JValue mergeInput = (JValue)plugin["filename"]!;
                        MergedPlugins.Add(mergeInput.Value<string>()!, (string)mergeResult!);
                    }
                }
                MergePaths.Add(mergePath);
                Console.WriteLine("Found possible merged plugin {0}", mergePath);
                using (StreamReader reader = File.OpenText(mergePath + "/map.json"))
                {
                    JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                    foreach (var mod in o)
                    {
                        // Key is a plugin name
                        string plugin = mod.Key;
                        // Value is a map of formid mappings. Form IDs are normalized as six hexadecimals chars.
                        var formMappings = ((JObject)(mod.Value!)).ToObject<Dictionary<string, string>>();
                        if (formMappings!.Count == 0)
                            continue;
                        MappedFormIds.Add(plugin, formMappings);
                    }
                }
            }
        }
    }
}
