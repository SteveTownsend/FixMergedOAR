using System;
using System.IO;
using System.Collections.Generic;

namespace FixMergedOAR
{
    public class DARConditions
    {
        public ISet<string> ConditionsPaths { get; } = new HashSet<string>();
        static readonly string PluginTag = ".esp";
        static readonly string FormIDStart = "0x";
        static readonly char[] FormIDEnd = "),".ToCharArray();

        public DARConditions(string inputFolder, MergeInfo mergeInfo)
        {
            // inventory merge candidates in the current directory
            EnumerationOptions options = new EnumerationOptions { RecurseSubdirectories = true };
            foreach (string conditionsFile in Directory.GetFiles(inputFolder, "_conditions.txt", options))
            {
                // read the contexts of the DAR conditions file and convert any merged plugin names and formids
                ConditionsPaths.Add(conditionsFile);
                Console.WriteLine("---- DAR Conditions file {0}", conditionsFile);
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
                        bool updatedLine = false;
                        int consumed = 0;
                        while (true)
                        {
                            // find next possible plugin - .esm and .esl unmergable
                            int offset = line.IndexOf(PluginTag, consumed);
                            if (offset == -1)
                            {
                                // flush text since final update to the line
                                if (updatedLine)
                                {
                                    newLine += line.Substring(consumed, line.Length - consumed);
                                }
                                break;
                            }
                            // read back to previous quote, should delimit the plugin
                            int quote = line.LastIndexOf('"', offset, offset - consumed);
                            if (quote == -1)
                                continue;
                            string pluginName = line.Substring(quote + 1, offset + PluginTag.Length - (quote + 1));
                            // check if this plugin has been merged - we need to rip and replace plugin and maybe formid if so
                            string? mappedESP;
                            bool isMapped = false;
                            if (mergeInfo.MergedPlugins.TryGetValue(pluginName, out mappedESP) && mappedESP is not null)
                            {
                                updatedLine = true;
                                updatedFile = true;
                                isMapped = true;
                                newLine += line.Substring(consumed, quote - consumed + 1);
                                newLine += mappedESP;
                            }
                            consumed = offset + PluginTag.Length;
                            IDictionary<string, string>? formMappings;
                            if (isMapped && mergeInfo.MappedFormIds.TryGetValue(pluginName, out formMappings) && formMappings is not null)
                            {
                                // check if FormID needs mapping
                                offset = line.IndexOf(FormIDStart, consumed);
                                if (offset != -1)
                                {
                                    int start = offset + FormIDStart.Length;
                                    int end = line.IndexOfAny(FormIDEnd, start);
                                    if (end != -1)
                                    {
                                        string formID = line.Substring(start, end - start);
                                        int numericFormID = Int32.Parse(formID, System.Globalization.NumberStyles.HexNumber);
                                        formID = String.Format("{0:X6}", numericFormID);
                                        string? newFormID;
                                        if (formMappings.TryGetValue(formID, out newFormID) && newFormID is not null)
                                        {
                                            // output skipped text and the updated form ID
                                            newLine += line.Substring(consumed, start - consumed);
                                            newLine += newFormID;
                                            consumed = end;
                                        }
                                    }
                                }
                            }
                        }
                        if (updatedLine)
                        {
                            Console.WriteLine("{0,4} '{1}' converted to '{2}'", lineNumber, line, newLine);
                            buffered.Add(newLine);
                        }
                        else
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