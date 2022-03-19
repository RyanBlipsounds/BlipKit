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
            GlobalStopEvent,
            GlobalStopAllEvents,
            VolumeByDistance,
            SpatialBlendByDistance,
            HighPassFilter,
            LowPassFilter
        }

        public ActionType                    Type;
        public Action.Play                   OptionsPlay;
        public Action.PlayFromRandomSet      OptionsPlayFromRandomSet;
        public Action.GlobalStopEvent        OptionsGlobalStopEvent;
        public Action.GlobalStopAllEvents    OptionsGlobalStopAllEvents;
        public Action.VolumeByDistance       OptionsVolumeByDistance;
        public Action.SpatialBlendByDistance OptionsSpatialBlendByDistance;
        public Action.HighPassFilter         OptionsHighPassFilter;
        public Action.LowPassFilter          OptionsLowPassFilter;

        public void Apply(BlipEmitter[] emitters, ref int emitterIndex)
        {
            if (Type == ActionType.None) return;

            if (NeedsEmitter()) emitterIndex++;
            GetAction().Apply(emitters, emitterIndex);
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
                case ActionType.Play:                   return OptionsPlay;
                case ActionType.PlayFromRandomSet:      return OptionsPlayFromRandomSet;
                case ActionType.GlobalStopEvent:        return OptionsGlobalStopEvent;
                case ActionType.GlobalStopAllEvents:    return OptionsGlobalStopAllEvents;
                case ActionType.VolumeByDistance:       return OptionsVolumeByDistance;
                case ActionType.SpatialBlendByDistance: return OptionsSpatialBlendByDistance;
                case ActionType.HighPassFilter:         return OptionsHighPassFilter;
                case ActionType.LowPassFilter:          return OptionsLowPassFilter;
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