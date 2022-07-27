using System.Reflection;
using MelonLoader;

[assembly: AssemblyTitle(cracktool.BuildInfo.Description)]
[assembly: AssemblyDescription(cracktool.BuildInfo.Description)]
[assembly: AssemblyCompany(cracktool.BuildInfo.Company)]
[assembly: AssemblyProduct(cracktool.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + cracktool.BuildInfo.Author)]
[assembly: AssemblyTrademark(cracktool.BuildInfo.Company)]
[assembly: AssemblyVersion(cracktool.BuildInfo.Version)]
[assembly: AssemblyFileVersion(cracktool.BuildInfo.Version)]
[assembly: MelonInfo(typeof(cracktool.cracktool), cracktool.BuildInfo.Name, cracktool.BuildInfo.Version, cracktool.BuildInfo.Author, cracktool.BuildInfo.DownloadLink)]
[assembly: MelonColor(System.ConsoleColor.DarkRed)]

// Create and Setup a MelonGame Attribute to mark a Melon as Universal or Compatible with specific Games.
// If no MelonGame Attribute is found or any of the Values for any MelonGame Attribute on the Melon is null or empty it will be assumed the Melon is Universal.
// Values for MelonGame Attribute can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame(null, null)]