using System;
using System.CodeDom;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Lemons.Cryptography;
using System.Runtime.CompilerServices;
using System.Text;
using HarmonyLib.Tools;
using UnityEngine;
using Random = System.Random;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Security;
using Il2CppSystem.Threading;
using Mono.Cecil;
using Thread = System.Threading.Thread;

namespace cracktool
{
    public static class BuildInfo
    {
        public const string Name = "UnixSaysGoodbye"; // Name of the Plugin.  (MUST BE SET)
        public const string Description = null; // Description for the Plugin.  (Set as null if none)
        public const string Author = "unixian :)"; // Author of the Plugin.  (MUST BE SET)
        public const string Company = null; // Company that made the Plugin.  (Set as null if none)
        public const string Version = "1.0.0"; // Version of the Plugin.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Plugin.  (Set as null if none)
    }

    public class cracktool : MelonPlugin
    {
        private static readonly HarmonyLib.Harmony Instance = new HarmonyLib.Harmony("UnixSaysGoodbye");
        public override void OnPreInitialization() // Runs before Game Initialization.
        {
            Instance.Patch(typeof(System.Net.WebClient).GetMethod(nameof(System.Net.WebClient.UploadValues), new Type[] { typeof(string), typeof(NameValueCollection) }),
                new HarmonyLib.HarmonyMethod(typeof(cracktool), "UploadPatch"));
            Instance.Patch(typeof(System.Diagnostics.Process).GetMethod(nameof(System.Diagnostics.Process.Kill)), new HarmonyLib.HarmonyMethod(typeof(cracktool), "ReturnFalse"));
        }

        public void r2f(string resourceName, string fileName)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (FileStream destination = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(destination);
                }
            }
        }

        public override void
            OnApplicationEarlyStart() // Runs after Game Initialization, before OnApplicationStart and (on Il2Cpp games) before Unhollower.
        {
            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Notorious"))
            {
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Notorious");
                MelonLogger.Msg(ConsoleColor.Blue, "created notorious directory in AppData/Roaming");
            }

            if (!File.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Notorious\\Auth.gg"))
            {
                File.Create($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Notorious\\Auth.gg").Close();
                File.WriteAllLines($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Notorious\\Auth.gg", new string[] { "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" });
                MelonLogger.Msg(ConsoleColor.Blue, "created auth file and filled with garbage data");
            }

            MelonLogger.Msg(ConsoleColor.Blue, "copying resources to userdata");
            r2f("cracktool.n-dependencies.dll", MelonUtils.UserDataDirectory + "\\n-dependencies.dll");
            r2f("cracktool.n-dummy.dll", MelonUtils.UserDataDirectory + "\\n-dummy.dll");
            r2f("cracktool.n.dll", MelonUtils.UserDataDirectory + "\\n.dll");
            r2f("cracktool.payload.txt", MelonUtils.UserDataDirectory + "\\payload.txt");

            Assembly asm = Assembly.LoadFrom(MelonUtils.UserDataDirectory + "\\n.dll");
            MelonHandler.LoadFromAssembly(asm, MelonUtils.UserDataDirectory + "\\n.dll");
            Assembly.LoadFrom(MelonUtils.UserDataDirectory + "\\n-dependencies.dll");
            Assembly.LoadFrom(MelonUtils.UserDataDirectory + "\\n-dummy.dll");
            MelonLogger.Msg(ConsoleColor.Blue, "loading assemblies...");
            MelonLogger.Msg(ConsoleColor.Green, "successfully loaded all assemblies (main, dummy, dependencies)");

            Type[] types = asm.GetTypes();
            foreach (Type type in types)
            {
                if (type.MetadataToken != 0x020003C0)
                {
                    continue;
                }
                try
                {
                    MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
                    foreach (MethodInfo methodInfo in methods)
                    {
                        if (methodInfo.Name == "HUDMessage")
                        {
                            Instance.Patch(methodInfo,
                                new HarmonyLib.HarmonyMethod(typeof(cracktool), "RewritePatch"));
                            MelonLogger.Msg(ConsoleColor.Blue, "patched HUDMessage");
                        }
                    }
                }
                catch (Exception e)
                {
                    MelonLogger.Msg($"error in methodinfo foreach, exception: \n {e}");
                }
            }

            foreach (Type type in types)
            {
                if (type.MetadataToken != 0x020004F2)
                {
                    continue;
                }
                try
                {
                    MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
                    foreach (MethodInfo methodInfo in methods)
                    {
                        if (methodInfo.Name == "Setup")
                        {
                            Instance.Patch(methodInfo,
                                new HarmonyLib.HarmonyMethod(typeof(cracktool), "ReturnFalse"));
                            MelonLogger.Msg(ConsoleColor.Blue, "patched Setup");
                        }

                        if (methodInfo.Name == "RCT")
                        {
                            Instance.Patch(methodInfo,
                                new HarmonyLib.HarmonyMethod(typeof(cracktool), "ReturnFalse"));
                            MelonLogger.Msg(ConsoleColor.Blue, "patched RCT");
                        }
                    }
                }
                catch (Exception e)
                {
                    MelonLogger.Msg($"error in methodinfo foreach, exception: \n {e}");
                }
            }

            MelonLogger.Msg(ConsoleColor.Green, "finished. client will load when OnApplicationStart is hit");

            // not needed sleep here, just so user can read stuff that was printed ig
            Thread.Sleep(2000);
        }


        public static bool UploadPatch(ref byte[] __result, ref string address, ref NameValueCollection data)
        {
            string[] addresses = { "104.26.8", "172.67.70", "104.26.9", "172.67.172", "104.22.42", "104.16.133" };
            if (addresses.Contains(address) == true)
            {
                MelonLogger.Msg(ConsoleColor.Blue, "redirecting call to UploadValues");
                __result = Encoding.UTF8.GetBytes($"{File.ReadAllText($"{MelonUtils.UserDataDirectory}\\payload.txt")}");
                return false;
            }
            return true;
        }

        public static bool RewritePatch(ref string Message, ref bool ForceMessage)
        {
            if (Message.Contains("<color=#D781E7>Notorious</color>"))
            {
                Message = "<color=#D781E7>Notorious</color> cracked by <color=#00FF00>Unixian</color>, enjoy!";
            }
            return true;
        }

        public static bool ReturnFalse()
        {
            return false;
        }
    }
}