using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blip
{
    [System.Serializable]
    public abstract class BlipAction
    {
        /**
         * Must be overwritten in a child BlipAction to return true.
         * If true, the event will need to request a dynamic emitter for this action to act on.
         */ 
        public virtual bool NeedsEmitter { get { return false; } }

        public abstract void Apply(BlipEmitter[] emitters, int emitterIndex);

        public abstract void ApplyToSingleAudioSource(AudioSource audioSource);
    }
}