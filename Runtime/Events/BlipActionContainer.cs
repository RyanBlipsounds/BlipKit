using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blip
{
    /** 
     * An action can 'Apply' an effect onto an BlipEmitter (AudioSource), such as playing a clip, 
     * adding an effect component, and so on. BlipEvents are made up of a list of Actions which 
     * execute in order.
     */
    [System.Serializable]
    public class BlipActionContainer
    {
        public enum ActionType
        {
            None,
            Play,
            PlayFromRandomSet,
            Stop,
            StopAllInEvent,
            HighPassFilter,
            LowPassFilter
        }

        public ActionType Type;
        public Action.Play OptionsPlay;
        public Action.PlayFromRandomSet OptionsPlayFromRandomSet;
        public Action.Stop OptionsStop;
        public Action.StopAllInEvent OptionsStopAllInEvent;
        public Action.HighPassFilter OptionsHighPassFilter;
        public Action.LowPassFilter OptionsLowPassFilter;

        public void Apply(BlipEmitter[] emitters, ref int emitterIndex)
        {
            if (Type == ActionType.None) return;

            GetAction().Apply(emitters, emitterIndex);

            if (NeedsEmitter()) emitterIndex++;
        }

        public void ApplyToSingleAudioSource(AudioSource audioSource)
        {
            if (Type == ActionType.None) return;

            GetAction().ApplyToSingleAudioSource(audioSource);
        }

        public BlipAction GetAction()
        {
            switch (Type)
            {
                case ActionType.Play: return OptionsPlay;
                case ActionType.PlayFromRandomSet: return OptionsPlayFromRandomSet;
                case ActionType.Stop: return OptionsStop;
                case ActionType.StopAllInEvent: return OptionsStopAllInEvent;
                case ActionType.HighPassFilter: return OptionsHighPassFilter;
                case ActionType.LowPassFilter: return OptionsLowPassFilter;
            }

            return null;
        }

        public bool NeedsEmitter()
        {
            if (Type == ActionType.None) return false;

            return GetAction().NeedsEmitter;
        }
    }
}