using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace AntiHarmonyHookDetector
{
    internal class Program
    {
        private static Harmony harmony = new Harmony("https://github.com/TheHellTower/AntiHarmonyHookDetector");

        private static bool isHooked(MethodBase method) => Marshal.ReadByte(method.MethodHandle.GetFunctionPointer()) == 0xE9; //If a method is Hooked it should be 0xE9

        private static void InstallAntiHookDetector()
        {
            MethodInfo antiDetection = typeof(Marshal).GetMethod("ReadByte", new[] { typeof(IntPtr) }); //Method to patch
            MethodInfo antiDetection2 = typeof(Program).GetMethod("PrefixM");
            MethodInfo antiDetection3 = typeof(Program).GetMethod("PostFixM");

            harmony.Patch(antiDetection, new HarmonyMethod(antiDetection2), new HarmonyMethod(antiDetection3), null, null);
        }

        private static void InstallHook(MethodInfo methodBase)
        {
            MethodInfo methodBase2 = typeof(Program).GetMethod("Prefix");
            MethodInfo methodBase3 = typeof(Program).GetMethod("PostFix");

            harmony.Patch(methodBase, new HarmonyMethod(methodBase2), new HarmonyMethod(methodBase3), null, null);
        }

        static void Main(string[] args)
        {
            Assembly assembly = Assembly.LoadFile(Path.GetFullPath(args[0])); //Loading the assembly
            if (assembly.GetName().ToString().Split(',')[0] != "Hooked")  //Verifying if the assembly is our assembly "Hooked"
            {
                //If not then...
                Console.WriteLine("This is a exemple only, it is not made to run over any file.\nIf you want to patch Harmony's Hooks detections, only take its match part.");
                Console.ReadKey();
                Environment.Exit(-1);
            }
            ParameterInfo[] parameters = assembly.EntryPoint.GetParameters(); //Getting the parameters of your assembly
            object[] array = new object[parameters.Length]; //Creating a object to pass it to our assembly with the length(number) of our parameters

            Console.WriteLine("Apply Anti Detection (Y for yes or any other key)?");
            ConsoleKeyInfo key = Console.ReadKey();
            if(key.Key == ConsoleKey.Y)
                InstallAntiHookDetector();

            Console.WriteLine();

            MethodInfo methodBase = typeof(Hooked.Program).GetMethod("isCracked"); //Our patched method
            InstallHook(methodBase);

            Console.WriteLine($"{methodBase.Name} Hooked Status: {isHooked(methodBase)}");
            assembly.EntryPoint.Invoke(null, array);
            Console.ReadLine();
        }

        [HarmonyPatch(typeof(Hooked.Program), "isCracked")]
        public static void Prefix() { }

        [HarmonyPatch(typeof(Hooked.Program), "isCracked")]
        public static void PostFix(ref bool __result) { //Harmony want a void method type for Prefix and Postif so you need to use a ref to modify the output of your targeted method.
            Console.WriteLine($"Original Method Result: {__result} | Patched Method Result: true");
            __result = true; //Here we are setting the returned value to be true whatever happen
        }

        [HarmonyPatch(typeof(Marshal), "ReadByte")]
        public static void PrefixM() { }

        [HarmonyPatch(typeof(Marshal), "ReadByte")]
        public static void PostFixM(IntPtr ptr, ref byte __result) //The targeted method take a IntPtr(a pointer)
        {
            if (__result == 0xE9)
                Console.WriteLine("Original Method Result: 0xE9 | Patched Method Result: 0xE8"); //If it is 0xE9 it mean detected so we set it to 0xE8 to remove this detection, "0" could break the application.
            __result = (byte)(__result == 0xE9 ? 0xE8 : __result);
        }
    }
}
