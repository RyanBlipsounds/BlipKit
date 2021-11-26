using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blip
{
    public static class Bank
    {
        private static BlipEvent[] blipEvents = null;

        public static void LoadAllEvents()
        {
            // Notes:
            // - Only works in Play. For an editor varient, use:
            //   AssetDatabase.FindAssets("t:BlipEvent", null);
            //   which returns a string[] of GUIDs that can be turned into asset references.
            // - To appear here, all BlipEvents need to be in the Assets/Resources folder.
            blipEvents = Resources.LoadAll<BlipEvent>("") as BlipEvent[];

            if (blipEvents.Length == 0)
            {
                Debug.LogWarning("[BlipKit.Bank] Can't find any BlipEvents (are they " +
                    "in the Resources folder?");
            }
        }

        public static BlipEvent FindEvent(string eventName)
        {
            if (blipEvents == null)
            {
                // If this behaviour is too automatic, could replace with return null and a warning.
                LoadAllEvents();
            }

            foreach (BlipEvent blipEvent in blipEvents)
            {
                if (blipEvent.name == eventName)
                {
                    return blipEvent;
                }
            }

            return null;
        }
    }
}
