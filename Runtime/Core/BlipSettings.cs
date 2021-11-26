using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blip
{    
    [CreateAssetMenu(fileName = "BlipSettings", menuName = "BlipKit/Global Settings", order = 0)]
    public class BlipSettings : ScriptableObject 
    {
        // The emitter count. Currently, initializing spawns all emitters which are used
        // as needed or remain idle when unused. This is better than risking latency when playing
        // a new clip due to spawning new emitters at run-time.
        public int MaxVoices = 3;
        public bool InvisibleEmitters = true;
    }
}
