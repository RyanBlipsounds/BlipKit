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
            PlayClip,
            PlayClipFromRandomSet,
            GlobalStopEvent,
            GlobalStopAllEvents,
            HighPassFilter,
            LowPassFilter
        }

        public ActionType Type;
        public Action.PlayClip OptionsPlayClip;
        public Action.PlayClipFromRandomSet OptionsPlayClipFromRandomSet;
        public Action.GlobalStopEvent OptionsGlobalStopEvent;
        public Action.GlobalStopAllEvents OptionsGlobalStopAllEvents;
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
                case ActionType.PlayClip: return OptionsPlayClip;
                case ActionType.PlayClipFromRandomSet: return OptionsPlayClipFromRandomSet;
                case ActionType.GlobalStopEvent: return OptionsGlobalStopEvent;
                case ActionType.GlobalStopAllEvents: return OptionsGlobalStopAllEvents;
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