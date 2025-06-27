# FixMergedOAR
Regenerates OAR and DAR Conditions files for plugins that have been merged using ZMerge.

ZMerge always alters the plugin name containing formIDs referenced, and depending on the merge may also alter the Form ID. This patcher generates a new set of _conditions.txt files for affected plugins and forms. The output files should be loaded using a separate mod that overrides the default files.

To be clear, this patcher fixes up your DAR _conditions.txt and OAR config.json files if you have ZMerge plugins in your LO. It does not allow you to combine conflicting animations that have the same priority - that's a manual task depending on your preference.

The affected files may or may not be associated with the plugins that you have merged : the issue is that plugin:formid references to merged plugins in any conditions files are incorrect after the merge.

For example: I have merged Animated Armoury into a large ZMerge called Weapons And Armour Merged.esp. That breaks its own conditions files, and also any other file from an unmerged mod that references Animated Armoury. The patcher corrects all of these references to Animated Armoury forms.
