using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blip
{
    public static class Statics
    {
        private static List<BlipEmitter> emitters = new List<BlipEmitter>();

        private static bool isInitialized = false;
        private static BlipSettings activeGlobalSettings;
        private static AudioListener activeListener;

        public static void Initialize(BlipSettings settings, AudioListener listener)
        {
            activeGlobalSettings = settings;
            activeListener = listener;
            isInitialized = true;

            InitializeEmitters();

            // Any other future initialization steps go here.

            Debug.Log("[BlipKit] Initialized.");
        }

        public static void PlayEvent2D(string eventName)
        {
            if (!isInitialized)
            {
                // Needs to be initialized with global settings. Exits silently, but you may
                // want a warning here.
                return;
            }

            BlipEvent targetEvent = Bank.FindEvent(eventName);
            if (targetEvent != null)
            {
                targetEvent.Play2D();
            }
            else
            {
                Debug.LogWarning("[BlipKit.Statics] Event \"" + eventName + "\" not found.");
            }
        }

        public static void PlayEvent2D(BlipEvent eventReference)
        {
            if (!isInitialized)
            {
                // Needs to be initialized with global settings. Exits silently, but you may
                // want a warning here.
                return;
            }

            eventReference.Play2D();
        }

        public static void PlayEventAtPosition(string eventName, Vector3 position)
        {
            if (!isInitialized)
            {
                // Needs to be initialized with global settings. Exits silently, but you may
                // want a warning here.
                return;
            }

            BlipEvent targetEvent = Bank.FindEvent(eventName);
            if (targetEvent != null)
            {
                PlayEventAtPosition(targetEvent, position);
            }
            else
            {
                Debug.LogWarning("[BlipKit.Statics] Event \"" + eventName + "\" not found.");
            }
        }

        public static void PlayEventAtPosition(BlipEvent eventReference, Vector3 position)
        {
            if (!isInitialized)
            {
                // Needs to be initialized with global settings. Exits silently, but you may
                // want a warning here.
                return;
            }

            eventReference.PlayAtPosition(position);
        }

        public static void PlayEventAttached(string eventName, GameObject objectToAttach)
        {
            if (!isInitialized)
            {
                // Needs to be initialized with global settings. Exits silently, but you may
                // want a warning here.
                return;
            }

            BlipEvent targetEvent = Bank.FindEvent(eventName);
            if (targetEvent != null)
            {
                PlayEventAttached(targetEvent, objectToAttach);
            }
            else
            {
                Debug.LogWarning("[BlipKit.Statics] Event \"" + eventName + "\" not found.");
            }
        }

        public static void PlayEventAttached(BlipEvent eventReference, GameObject objectToAttach)
        {
            if (!isInitialized)
            {
                // Needs to be initialized with global settings. Exits silently, but you may
                // want a warning here.
                return;
            }

            eventReference.PlayAttached(objectToAttach);
        }

        public static void SetEmitterVolume(BlipEmitter[] emitters, float volume)
        {
            foreach (BlipEmitter emitter in emitters)
            {
                emitter.GetSource().volume = volume;
            }
        }

        public static void SetEmitterPitch(BlipEmitter[] emitters, double pitchAdjust)
        {
            foreach (BlipEmitter emitter in emitters)
            {
                // Possibly truncates some accuracy here. The + 1.0 is because Unity's pitch is
                // a multiplicative state, not just an adjustment. A pitch of 0 cents (no change)
                // must be a pitch of 1.0 in a Unity audio source (* 1 = no change).
                emitter.GetSource().pitch = (float)PitchAsUnityRange(pitchAdjust);   
            }
        }

        public static double PitchAsUnityRange(double pitchInCentsOfSemitone)
        {
            // Notes (Angel: 2022.01.07):
            // A cent of a semitone corresponds to multiplying the frequency by 
            // 0.000594630943592952645618252949463... (or 2^(1/12)/100).
            // Tested with a tuning note (A, 440Hz) and a microphone-based guitar tuner on a pair
            // of headphones. The first few semitones in either direction are exactly on tune but 
            // subsequent semitones (3+, or a value of 300+ cents of a semitone) the physical tuner 
            // began to see the target note as increasingly off key. This is a subtle effect, and 
            // trying to tune for a further note then detunes the closer notes. I'm not sure if  
            // this is an effect of multication present in either this algorithm or Unity's, or  
            // perhaps it's an accuracy or rounding issue, or perhaps it's an effect of the tuner 
            // or speakers or sample I used. In any case, the value being used here is the 
            // mathematically "correct" value that's supposed to work, and does in tests.
            // This likely won't need to be revisted but I wanted to note it here in case.

            // Clamping pitch to -2400f to 2400f would go here, to match Wwise, but unnecessary 
            // since values outside the range are still handled correctly. Currently only enforced
            // by the range in the BlipEvent inspector slider for PitchAdjustment.

            // Modify pitch between -2400f and 2400f (cents of a semitone). Remaps this to Unity's 
            // multiplicative frequency (0 to 3f, meaning a max where the frequency is multiplied 
            // by 3). 1 is therefore added to this adjustment value when applied to the audio 
            // source, as an adjustment of 0 here should result in a value of 1 in the AudioSource
            // (no change).
            if (pitchInCentsOfSemitone > 0.0)
            {
                // A max value of 2400 cents becomes 2.427114.
                return (pitchInCentsOfSemitone * 0.000594630943592952645618252949463) + 1.0; 
            }
            else if (pitchInCentsOfSemitone < 0.0)
            {
                // A min value of -2400 cents becomes 0.4120119.
                return 1.0 / (1.0 + -pitchInCentsOfSemitone * 0.000594630943592952645618252949463);
            }

            // Pitch adjustment of 0 is no change (1.0 in Unity audio source).
            return 1.0;
        }
        public static AudioListener GetListener()
        {
            if (activeListener == null) return null;

            return activeListener;
        }

        public static GameObject GetListenerGameobject()
        {
            if (activeListener == null) return null;

            return activeListener.gameObject;
        }

        public static BlipEmitter RequestEmitter(int requestPriority)
        {
            if (!isInitialized)
            {
                // Needs to be initialized with global settings. Exits silently, but you may
                // want a warning here.
                return null;
            }

            if (emitters.Count <= 0)
            {
                // Can't play audio if there's no emitters. This shouldn't ever happen; maybe
                // the emitter count from the loaded GlobalSettings (if there is one) is zero,
                // or something went wrong with initialization.
                return null;
            }

            // Notes: Could be upgraded with more intelligent criteria such as a reference position,
            // to better inform a priority decision at maximum voice count.

            BlipEmitter targetEmitter = null;

            // Look for first inactive emitter.
            foreach (BlipEmitter emitter in emitters)
            {
                // Setting the emitter requested means it won't be targetted during additional 
                // emitter requests.
                if (!emitter.IsActive() && emitter.Request(requestPriority))
                {
                    targetEmitter = emitter;

                    break;
                }
            }

            // Use an in-use emitter, instantly interupting it. Makes this decision using the
            // priority value.
            if (targetEmitter == null)
            {
                int lowestPriority = requestPriority + 1;   // This means one wasn't found.

                foreach (BlipEmitter emitter in emitters)
                {
                    if (emitter.GetActivePriority() <= requestPriority && 
                        emitter.GetActivePriority() < lowestPriority &&
                        emitter.Request(requestPriority))
                    {
                        lowestPriority = emitter.GetActivePriority();
                        targetEmitter = emitter;
                    }
                }
            }

            // This might still be null if no inactive emitters were found at or below the request
            // priority. This should be checked by the reciever of this method, and likely means
            // the event shouldn't play.
            return targetEmitter;
        }

        /**
         * Requests an array of emitters of the specified size. May return partial if there aren't
         * enough emitters available of equal or lower priority. In that case, some returned 
         * emitters will be null. The receiving function should check for this.
         * 
         * NOTE: Currently, request priority is event-wide. Would we want this to be something
         *       individual to each emitter request? For example, an event could combine high-
         *       priority and low-priority requests.
         */
        public static BlipEmitter[] RequestEmitters(int emitterCount, int requestPriority)
        {
            BlipEmitter[] outEmitters = new BlipEmitter[emitterCount];

            for (int i=0; i<emitterCount; i++)
            {
                outEmitters[i] = RequestEmitter(requestPriority);
            }

            return outEmitters;
        }

        private static void InitializeEmitters()
        {
            if (emitters.Count > 0)
            {
                foreach (BlipEmitter emitter in emitters)
                {
                    emitter.DestroySelf();
                }
            }

            emitters = new List<BlipEmitter>();

            for (int i=0; i<activeGlobalSettings.MaxVoices; i++)
            {
                GameObject newEmitterObject = new GameObject("Blip Emitter " + (i+1));
                newEmitterObject.transform.parent = BlipManager.Get().transform;
                
                if (activeGlobalSettings.InvisibleEmitters)
                {
                    newEmitterObject.hideFlags = HideFlags.HideAndDontSave;
                }
                else
                {
                    newEmitterObject.hideFlags = HideFlags.DontSave;
                }

                BlipEmitter newEmitter = newEmitterObject.AddComponent<BlipEmitter>();
                emitters.Add(newEmitter);
            }
        }
    }
}
