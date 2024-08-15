using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection.Emit;

namespace OpenPDAWhilePilotingCyclops;

[HarmonyPatch]
public static class Patches
{
    [HarmonyPatch(typeof(PDA), nameof(PDA.Open))]
    public static class Patch_MainCameraControl_OnUpdate
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            /*

            */

            CodeMatcher cm = new(instructions);

            // Find:
            // if (!flag || main.cinematicModeActive)
            // {
            //     return false;
            // }
            cm.MatchForward(true, // false = move at the start of the match, true = move at the end of the match
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Inventory), nameof(Inventory.ReturnHeld))),
                new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(Player), nameof(Player.main))),
                new CodeMatch(OpCodes.Stloc_0),
                new CodeMatch(OpCodes.Brfalse),
                new CodeMatch(OpCodes.Ldloc_0),
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Player), nameof(Player.cinematicModeActive))),
                new CodeMatch(OpCodes.Brfalse));

            if (cm.IsValid)
            {
                CodeInstruction br = new CodeInstruction(OpCodes.Br, cm.Operand);
                cm.Advance(-2); // Ldloc_0
                br.MoveLabelsFrom(cm.Instruction);
                cm.Insert(br);
            }
            else
            {
                Plugin.Logger.LogError("Unable to patch PDA.Open().");
            }

            return cm.InstructionEnumeration();
        }
    }
}
