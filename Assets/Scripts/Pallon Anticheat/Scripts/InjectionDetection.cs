namespace PallonAnticheat
{
    using PlayFab;
    using PlayFab.ClientModels;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Security.Cryptography;
    using UnityEngine;
    using System.Linq;

    public static class InjectionDetection
    {
        static List<string> banRequests;

        readonly static string[] bannedDlls = new string[]
        {
            "lemon",
            "harmony",
            "melonloader",
            "lemonloader",
            "mod"
        };

        public static List<string> LoadedAssemblies
        {
            get => AppDomain.CurrentDomain.GetAssemblies().ToList().Select(s => s.GetName().FullName.ToLower()).ToList();
        }

        public static void CheckMods()
        {
            foreach (var banned in bannedDlls)
            {
                foreach (var mod in LoadedAssemblies)
                {
                    if (mod.ToLower().Contains(banned))
                    {
                        
                    }
                }
            }
        }
        // honestly theres nothing you can do in unity to reliably detect dll injection unless you do a deepdive into kernel anticheat
        // hackers/modders will ALWAYS be able to patch methods and string lists, including private, internal, etc. nothing is safe from being patched.
        // :)
    }
}