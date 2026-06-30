using System;
using System.IO;
using System.Collections.Generic;

namespace FixMergedOAR
{
    public class OARConditions
    {
        public ISet<string> ConditionsPaths { get; } = new HashSet<string>();
        static readonly string PluginTag = ".esp";
        static readonly string FormIDStart = "\"formID\": \"";
        static readonly string FormIDEnd = "\"";

        public OARConditions(string inputFolder, MergeInfo mergeInfo)
        {
            // inventory merge candidates in the current directory
            EnumerationOptions options = new EnumerationOptions { RecurseSubdirectories = true };
            foreach (string conditionsFile in Directory.GetFiles(inputFolder, "config.json", options))
            {
                // read the contexts of the DAR conditions file and convert any merged plugin names and formids
                ConditionsPaths.Add(conditionsFile);
                Console.WriteLine("---- OAR Conditions file {0}", conditionsFile);
                using (StreamReader reader = File.OpenText(conditionsFile))
                {
                    IList<string> buffered = new List<string>();
                    int lineNumber = 0;
                    bool updatedFile = false;
                    while (!reader.EndOfStream)
                    {
                        ++lineNumber;
                        string line = reader.ReadLine()!;
                        string newLine = String.Empty;
                        // find next possible plugin - .esm and .esl unmergable
                        int offset = line.IndexOf(PluginTag);
                        if (offset == -1)
                        {
                            // flush unaltered line
                            buffered.Add(line);
                            continue;
                        }
                        // read back to previous quote, should delimit the plugin
                        int quote = line.LastIndexOf('"', offset);
                        if (quote == -1)
                        {
                            // flush unaltered line
                            buffered.Add(line);
                            continue;
                        }
                        string pluginName = line.Substring(quote + 1, offset + PluginTag.Length - (quote + 1));
                        // check if this plugin has been merged - we need to rip and replace plugin and maybe formid if so
                        string? mappedESP;
                        bool isMapped = false;
                        if (mergeInfo.MergedPlugins.TryGetValue(pluginName, out mappedESP) && mappedESP is not null)
                        {
                            updatedFile = true;
                            isMapped = true;
                            newLine += line.Substring(0, quote + 1);
                            newLine += mappedESP;
                            newLine += line.Substring(offset + PluginTag.Length);
                            buffered.Add(newLine);
                            Console.WriteLine("{0,4} '{1}' converted to '{2}'", lineNumber, line, newLine);
                        }
                        if (String.IsNullOrEmpty(newLine))
                        {
                            buffered.Add(line);
                            continue;
                        }
                        IDictionary<string, string>? formMappings;
                        newLine = String.Empty;
                        if (isMapped && mergeInfo.MappedFormIds.TryGetValue(pluginName, out formMappings) && formMappings is not null)
                        {
                            // check if FormID needs mapping - it is on the next line in OAR config.json files
                            ++lineNumber;
                            line = reader.ReadLine()!;
                            offset = line.IndexOf(FormIDStart);
                            if (offset != -1)
                            {
                                int start = offset + FormIDStart.Length;
                                int end = line.IndexOf(FormIDEnd, start);
                                if (end != -1)
                                {
                                    string formID = line.Substring(start, end - start);
                                    int numericFormID = Int32.Parse(formID, System.Globalization.NumberStyles.HexNumber);
                                    formID = String.Format("{0:X6}", numericFormID);
                                    string? newFormID;
                                    if (formMappings.TryGetValue(formID, out newFormID) && newFormID is not null)
                                    {
                                        // output skipped text and the updated form ID
                                        newLine += line.Substring(0, start);
                                        newLine += String.Format("{0:X}", Int32.Parse(newFormID, System.Globalization.NumberStyles.HexNumber));
                                        newLine += line.Substring(end);
                                        buffered.Add(newLine);
                                        Console.WriteLine("{0,4} '{1}' converted to '{2}'", lineNumber, line, newLine);
                                    }
                                }
                            }
                        }
                        if (String.IsNullOrEmpty(newLine))
                        {
                            buffered.Add(line);
                        }
                    }
                    // If this file required updates, output the new lines to the patch location
                    if (updatedFile)
                    {
                        string updatedPath = Path.GetRelativePath(inputFolder, conditionsFile);
                        updatedPath = Program.settings.OutputFolder + updatedPath.Substring(updatedPath.IndexOf("\\"));
                        Directory.CreateDirectory(Path.GetDirectoryName(updatedPath)!);
                        File.WriteAllLines(updatedPath, buffered);
                        Console.WriteLine("---- {0} written", updatedPath);
                    }
                }
            }
        }
    }
}