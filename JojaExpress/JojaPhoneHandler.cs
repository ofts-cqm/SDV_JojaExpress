﻿using StardewValley.Objects;

namespace JojaExpress
{
    public class JojaPhoneHandler : IPhoneHandler
    {
        public string CheckForIncomingCall(Random random)
        {
            return "";
        }

        public IEnumerable<KeyValuePair<string, string>> GetOutgoingNumbers()
        {
            if(ModEntry.config != null && !ModEntry.config.OpenByPhone) 
                return new Dictionary<string, string>(); 
            return new Dictionary<string, string>() { { "ofts.JojaExp.shop", ModEntry._Helper.Translation.Get("phone") } };
        }

        public bool TryHandleIncomingCall(string callId, out Action showDialogue)
        {
            showDialogue = ()=>{ };
            return false;
        }

        public bool TryHandleOutgoingCall(string callId)
        {
            if (callId != "ofts.JojaExp.shop") return false;
            PlayerInteractionHandler.openMenu();
            return true;
        }
    }
}
