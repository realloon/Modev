global using Verse;
using JetBrains.Annotations;
using HarmonyLib;

namespace Modev;

[UsedImplicitly]
[StaticConstructorOnStartup]
public class Modev {
    static Modev() {
        var harmony = new Harmony("Vortex.Modev");
        harmony.PatchAll();
    }
}