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

        public static void PlayEvent2D(string eventName, float volume=1f)
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
                targetEvent.Play2D(volume);
            }
            else
            {
                Debug.LogWarning("[BlipKit.Statics] Event \"" + eventName + "\" not found.");
            }
        }

        public static void PlayEvent2D(BlipEvent eventReference, float volume=1f)
        {
            if (!isInitialized)
            {
                // Needs to be initialized with global settings. Exits silently, but you may
                // want a warning here.
                return;
            }

            eventReference.Play2D(volume);
        }

        public static void PlayEventAtPosition(string eventName, Vector3 position, float volume=1f)
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
                PlayEventAtPosition(targetEvent, position, volume);
            }
            else
            {
                Debug.LogWarning("[BlipKit.Statics] Event \"" + eventName + "\" not found.");
            }
        }

        public static void PlayEventAtPosition(BlipEvent eventReference, Vector3 position, float volume=1f)
        {
            if (!isInitialized)
            {
                // Needs to be initialized with global settings. Exits silently, but you may
                // want a warning here.
                return;
            }

            eventReference.PlayAtPosition(position, volume);
        }

        public static void PlayEventAttached(string eventName, GameObject objectToAttach, float volume=1f)
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
                PlayEventAttached(targetEvent, objectToAttach, volume);
            }
            else
            {
                Debug.LogWarning("[BlipKit.Statics] Event \"" + eventName + "\" not found.");
            }
        }

        public static void PlayEventAttached(BlipEvent eventReference, GameObject objectToAttach, float volume=1f)
        {
            if (!isInitialized)
            {
                // Needs to be initialized with global settings. Exits silently, but you may
                // want a warning here.
                return;
            }

            eventReference.PlayAttached(objectToAttach, volume);
        }

        public static void SetEmitterVolume(BlipEmitter[] emitters, float volume)
        {
            foreach (BlipEmitter emitter in emitters)
            {
                emitter.GetSource().volume = volume;
            }
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
