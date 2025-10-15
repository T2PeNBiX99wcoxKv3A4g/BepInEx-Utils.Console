using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UnityEngine;

namespace BepinExUtils.Console.Patches;

[HarmonyPatch(typeof(Cursor))]
internal class CursorPatches
{
    [HarmonyPatch(nameof(Cursor.visible), MethodType.Setter)]
    [HarmonyPrefix]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static bool visible(ref bool value)
    {
        if (Behaviour.Console.CurrentlySettingCursor) return true;
        Behaviour.Console.LastVisible = value;
        if (!Behaviour.Console.Active) return true;
        if (Cursor.visible) return false;
        value = true;
        return true;
    }

    [HarmonyPatch(nameof(Cursor.lockState), MethodType.Setter)]
    [HarmonyPrefix]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static bool lockState(ref CursorLockMode value)
    {
        if (Behaviour.Console.CurrentlySettingCursor) return true;
        Behaviour.Console.LastLockState = value;
        if (!Behaviour.Console.Active) return true;
        if (Cursor.lockState == CursorLockMode.None) return false;
        value = CursorLockMode.None;
        return true;
    }
}