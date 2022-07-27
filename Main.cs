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
            // simple rundown of how notorious does some checks

            // step 1: notorious checks if you have 2 things: the notorious folder in your appdata and the auth.gg (contains your key) in that folder
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

            // step 2: notorious split their dependencies into a seperate dll and looks for it when it's running, without it you won't have the proper resources.
            // no idea if notorious actually checks if the loader is loaded into the program or not, but we do it for safe measure :)
            MelonLogger.Msg(ConsoleColor.Blue, "copying resources to userdata");
            r2f("cracktool.n-dependencies.dll", MelonUtils.UserDataDirectory + "\\n-dependencies.dll");
            r2f("cracktool.n-dummy.dll", MelonUtils.UserDataDirectory + "\\n-dummy.dll");
            r2f("cracktool.n.dll", MelonUtils.UserDataDirectory + "\\n.dll");
            r2f("cracktool.payload.txt", MelonUtils.UserDataDirectory + "\\payload.txt");

            // step 3: the notorious core dll is actually a melonloader mod, meaning that it won't work with conventional means of just invoking OnApplicationStart as if it was a static method
            // melonloader gives us the ability to load a mod from an assembly, so we just do this.
            Assembly asm = Assembly.LoadFrom(MelonUtils.UserDataDirectory + "\\n.dll");
            MelonHandler.LoadFromAssembly(asm, MelonUtils.UserDataDirectory + "\\n.dll");
            Assembly.LoadFrom(MelonUtils.UserDataDirectory + "\\n-dependencies.dll");
            Assembly.LoadFrom(MelonUtils.UserDataDirectory + "\\n-dummy.dll");
            MelonLogger.Msg(ConsoleColor.Blue, "loading assemblies...");
            MelonLogger.Msg(ConsoleColor.Green, "successfully loaded all assemblies (main, dummy, dependencies)");

            // here is where the magic comes in.
            // if you don't have a basic understanding of reflection, decide to read up on it as it's pretty interesting.

            // GetTypes gets all the types within our Notorious core assembly, allowing us to see all the types that are inside of the assembly.
            Type[] types = asm.GetTypes();
            foreach (Type type in types)
            {
                // foreaching through these types allows us to step through each type one at a time
                // we do this metadatatoken search because of notorious's naming convenction on types not being the most friendly
                if (type.MetadataToken != 0x020003C0)
                {
                    continue;
                }

                // once we find the proper type, we can find all the methods (u can also call them functions) inside of the type, this gives us a lot of power.
                try
                {
                    MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
                    foreach (MethodInfo methodInfo in methods)
                    {
                        // the method in this instance is HUDMessage, which shows the little "Welcome to Notorious" message. seemed kind of funny to patch so i did it.
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

            // same principle here
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
                        // this is one of the auth checks inside of notorious, due to no really preliminary checks to see if this ever gets ran/set we can just make this not run.
                        if (methodInfo.Name == "Setup")
                        {
                            Instance.Patch(methodInfo,
                                new HarmonyLib.HarmonyMethod(typeof(cracktool), "ReturnFalse"));
                            MelonLogger.Msg(ConsoleColor.Blue, "patched Setup");
                        }

                        // this is also another one of the auth checks inside of notorious, once again due to no really preliminary checks to see if it ever gets ran/set we can make it not run.
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

            // we are done!
            MelonLogger.Msg(ConsoleColor.Green, "finished. client will load when OnApplicationStart is hit");

            // not needed sleep here, just so user can read stuff that was printed ig
            Thread.Sleep(2000);
        }


        public static bool UploadPatch(ref byte[] __result, ref string address, ref NameValueCollection data)
        {
            // this is retrived from Meap's cloudflare list on his github.
            string[] addresses = { "104.26.8", "172.67.70", "104.26.9", "172.67.172", "104.22.42", "104.16.133" };
            if (addresses.Contains(address) == true)
            {
                MelonLogger.Msg(ConsoleColor.Blue, "redirecting call to UploadValues");

                // when you make a request to meap's servers, it returns a couple of things:
                // your key,
                // your avatar favorites,
                // all the nametags (userids are encrypted, don't ask me to decrypt)
                // your settings for notorious
                // some more
                __result = Encoding.UTF8.GetBytes($"{File.ReadAllText($"{MelonUtils.UserDataDirectory}\\payload.txt")}");
                return false;
            }
            return true;
        }

        public static bool RewritePatch(ref string Message, ref bool ForceMessage)
        {
            // funny patch to say notorious cracked by unixian ig
            if (Message.Contains("<color=#D781E7>Notorious</color>"))
            {
                Message = "<color=#D781E7>Notorious</color> cracked by <color=#00FF00>Unixian</color>, enjoy!";
            }
            return true;
        }

        public static bool ReturnFalse()
        {
            // patch to just make something not run cause harmony is cool
            return false;
        }
    }
}