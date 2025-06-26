using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Synthesis.Settings;

namespace FixMergedOAR
{
    public class Settings
    {
        [SynthesisSettingName("Input Folder")]
        [SynthesisTooltip("This must be a valid path on your computer. Typically this points to the mod directory in your Mod Manager VFS, e.g. 'D:/ModdedSkyrim/mods'.")]
        [SynthesisDescription("Path to be scanned for merged plugins and DAR _conditions.txt files.")]
        //public string InputFolder { get; set; } = "";
        public string InputFolder { get; set; } = "J:\\SteamLibrary\\steamapps\\common\\Skyrim Special Edition\\Data";

        [SynthesisSettingName("Output Folder")]
        [SynthesisTooltip("This must be a valid path on your computer. Typically this points to a new mod directory in your Mod Manager VFS, e.g. 'D:/ModdedSkyrim/mods/FIx Merged DAR'.")]
        [SynthesisDescription("Path where remapped DAR _conditions.txt files are written.")]
        //public string OutputFolder { get; set; } = "";
        public string OutputFolder { get; set; } = "j:/OmegaLOTD/Tools/Mods/Fix Merged DAR";
    }
}
