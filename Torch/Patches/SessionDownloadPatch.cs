﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NLog;
using Sandbox.Game.World;
using Torch.API.Session;
using Torch.API.Managers;
using Torch.Managers.PatchManager;
using Torch.Mod;
using VRage.Game;

namespace Torch.Patches
{
    [PatchShim]
    internal static class SessionDownloadPatch
    {
        private static ITorchSessionManager _sessionManager;
        private static ITorchSessionManager SessionManager => _sessionManager ?? (_sessionManager = TorchBase.Instance.Managers.GetManager<ITorchSessionManager>());


        internal static void Patch(PatchContext context)
        {
            context.GetPattern(typeof(MySession).GetMethod(nameof(MySession.GetWorld))).Suffixes.Add(typeof(SessionDownloadPatch).GetMethod(nameof(SuffixGetWorld), BindingFlags.Static | BindingFlags.NonPublic));
        }

        // ReSharper disable once InconsistentNaming
        private static void SuffixGetWorld(ref MyObjectBuilder_World __result)
        {
            //copy this list so mods added here don't propagate up to the real session
            __result.Checkpoint.Mods = __result.Checkpoint.Mods.ToList();
            
            __result.Checkpoint.Mods.AddRange(SessionManager.OverrideMods);

            var factionList = __result.Checkpoint.Factions.Factions;
            foreach(var faction in factionList) {
                
                //regex to see if any string follows format of \&.*?\;
                Regex regex = new Regex(@"\&.*?\;");
            
                //replace null strings with empty strings
                string privateInfo = faction.PrivateInfo ?? "";
                string description = faction.Description ?? "";
                string factionName = faction.Name ?? "";
                string factionTag = faction.Tag ?? "";
            
            
                if (regex.IsMatch(factionTag) || regex.IsMatch(factionName) || regex.IsMatch(description) || regex.IsMatch(privateInfo)) {
                    __result.Checkpoint.Factions.Factions.Remove(faction);
                    continue;
                }
                
                if (faction.Tag?.Length > 512 || faction.Name?.Length > 512 || faction.Description?.Length > 512 || faction.PrivateInfo?.Length > 512) {
                    __result.Checkpoint.Factions.Factions.Remove(faction);
                }
            }
        }
    }
}
