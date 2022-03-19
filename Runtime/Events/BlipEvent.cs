using UnityEngine;

namespace Blip
{
    /**
     * A BlipEvent is a group of IBlipActionables and built-in modifiers, which when the BlipEvent 
     * is activated, all component IBlipActionable actions in the event are applied to the target 
     * BlipEmitter in order.
     */ 
    [CreateAssetMenu(fileName = "BlipEvent", menuName = "BlipKit/Event", order = 0)]
    public class BlipEvent : ScriptableObject
    {
        [System.Serializable]
        public class AttenuationSettings
        {
            [Range(0f, 1f)]
            public float AttenuationAmount = 0f;
            public float MinDistance = 1f;
            public float MaxDistance = 300f;
        };

        public int priority = 1;

        [Space(10)]

        public BlipActionContainer[] Actions;

        [Space(10)]
        [Header("Modifiers")]

        [Range(0f, 1f)]
        public float Volume = 1f;

        [Tooltip("Pitch adjustment in cents of a semitone.")]
        [Range(-2400, 2400)]
        public int PitchAdjust = 0;

        [Space(10)]

        public AttenuationSettings attenuationSettings;

        #region Public Methods

        public BlipEmitter[] Play2D()
        {
            return Play2D(Volume);
        }
        
        public BlipEmitter[] Play2D(float volume)
        {
            BlipEmitter[] emitters = Statics.RequestEmitters(GetEmitterCount(), priority);

            foreach (BlipEmitter emitter in emitters)
            {
                emitter.Reset();
                emitter.AttachTo(Statics.GetListenerGameobject());
                emitter.SetCurrentEventName(name);
            }

            Statics.SetEmitterVolume(emitters, volume);
            Statics.SetEmitterPitch(emitters, PitchAdjust);

            for (int i=0; i<emitters.Length; i++)
            {
                ApplySpatial2DToAudioSource(emitters[i].GetSource());
            }

            int emitterIndex = -1;
            foreach (BlipActionContainer action in Actions)
            {
                action.Apply(emitters, ref emitterIndex);
            }

            return emitters;
        }

        public BlipEmitter[] PlayAtPosition(Vector3 position)
        {            
            return PlayAtPosition(position, Volume);
        }

        public BlipEmitter[] PlayAtPosition(Vector3 position, float volume)
        {
            BlipEmitter[] emitters = Statics.RequestEmitters(GetEmitterCount(), priority);

            foreach (BlipEmitter emitter in emitters)
            {
                if (emitter == null) continue;
                
                emitter.Reset();
                emitter.GoToPosition(position);
                emitter.SetCurrentEventName(name);
            }

            Statics.SetEmitterVolume(emitters, volume);
            Statics.SetEmitterPitch(emitters, PitchAdjust);

            for (int i=0; i<emitters.Length; i++)
            {
                ApplySpatialToAudioSource(emitters[i].GetSource());
            }

            int emitterIndex = -1;
            foreach (BlipActionContainer action in Actions)
            {
                action.Apply(emitters, ref emitterIndex);
            }

            return emitters;
        }

        public BlipEmitter[] PlayAttached(GameObject objectToAttach)
        {
            return PlayAttached(objectToAttach, Volume);
        }

        public BlipEmitter[] PlayAttached(GameObject objectToAttach, float volume)
        {
            BlipEmitter[] emitters = Statics.RequestEmitters(GetEmitterCount(), priority);

            foreach (BlipEmitter emitter in emitters)
            {
                emitter.Reset();
                emitter.AttachTo(objectToAttach);
                emitter.SetCurrentEventName(name);
            }

            Statics.SetEmitterVolume(emitters, volume);
            Statics.SetEmitterPitch(emitters, PitchAdjust);

            for (int i=0; i<emitters.Length; i++)
            {
                ApplySpatialToAudioSource(emitters[i].GetSource());
            }

            int emitterIndex = -1;
            foreach (BlipActionContainer action in Actions)
            {
                action.Apply(emitters, ref emitterIndex);
            }

            return emitters;
        }

        public void PlayOnSingleSource(AudioSource audioSource)
        {
            PlayOnSingleSource(audioSource, Volume);
        }

        public void PlayOnSingleSource(AudioSource audioSource, float volume)
        {
            audioSource.volume = volume;
            audioSource.pitch = (float)Statics.PitchAsUnityRange(PitchAdjust);

            ApplySpatial2DToAudioSource(audioSource);

            foreach (BlipActionContainer action in Actions)
            {
                action.ApplyToSingleAudioSource(audioSource);
            }
        }

        #endregion

        #region Private Methods

        private void ApplySpatialToAudioSource(AudioSource audioSource)
        {
            audioSource.rolloffMode = AudioRolloffMode.Custom;
            audioSource.spatialize = attenuationSettings.AttenuationAmount <= 0f;
            audioSource.spatialBlend = attenuationSettings.AttenuationAmount;
            audioSource.minDistance = attenuationSettings.MinDistance;
            audioSource.maxDistance = attenuationSettings.MaxDistance;
        }

        private void ApplySpatial2DToAudioSource(AudioSource audioSource)
        {
            audioSource.rolloffMode = AudioRolloffMode.Custom;
            audioSource.spatialBlend = 0f;
            audioSource.spatialize = false;
        }

        private int GetEmitterCount()
        {
            int emittersNeeded = 0;

            foreach(BlipActionContainer actionContainer in Actions)
            {
                if (actionContainer.NeedsEmitter())
                {
                    emittersNeeded++;
                }
            }

            return emittersNeeded;
        }

        #endregion
    }
}
