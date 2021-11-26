using UnityEngine;

namespace Blip
{
    /**
     * A BlipEvent is a group of IBlipActionable classes which when the BlipEvent is activated, all
     * component IBlipActionable actions in the event are applied to the target BlipEmitter in 
     * order.
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

        public AttenuationSettings attenuationSettings;

        public void Play2D(float volume = 1f)
        {
            BlipEmitter[] emitters = Statics.RequestEmitters(GetEmitterCount(), priority);

            foreach (BlipEmitter emitter in emitters)
            {
                emitter.AttachTo(Statics.GetListenerGameobject());
            }

            int emitterIndex = 0;
            foreach (BlipActionContainer action in Actions)
            {
                action.Apply(emitters, ref emitterIndex);
            }

            Statics.SetEmitterVolume(emitters, volume);
            ApplySpatial2D(emitters);
        }

        public void PlayAtPosition(Vector3 position, float volume = 1f)
        {            
            BlipEmitter[] emitters = Statics.RequestEmitters(GetEmitterCount(), priority);

            foreach (BlipEmitter emitter in emitters)
            {
                emitter.GoToPosition(position);
            }

            int emitterIndex = 0;
            foreach (BlipActionContainer action in Actions)
            {
                action.Apply(emitters, ref emitterIndex);
            }

            Statics.SetEmitterVolume(emitters, volume);
            ApplySpatial(emitters);
        }

        public void PlayAttached(GameObject objectToAttach, float volume = 1f)
        {
            BlipEmitter[] emitters = Statics.RequestEmitters(GetEmitterCount(), priority);

            foreach (BlipEmitter emitter in emitters)
            {
                emitter.AttachTo(objectToAttach);
            }

            int emitterIndex = 0;
            foreach (BlipActionContainer action in Actions)
            {
                action.Apply(emitters, ref emitterIndex);
            }

            Statics.SetEmitterVolume(emitters, volume);
            ApplySpatial(emitters);
        }

        private void ApplySpatial(BlipEmitter[] emitters)
        {
            for (int i=0; i<emitters.Length; i++)
            {
                AudioSource audioSource = emitters[i].GetSource();

                audioSource.spatialize = attenuationSettings.AttenuationAmount <= 0f;
                audioSource.spatialBlend = attenuationSettings.AttenuationAmount;
                audioSource.minDistance = attenuationSettings.MinDistance;
                audioSource.maxDistance = attenuationSettings.MaxDistance;
            }
        }

        private void ApplySpatial2D(BlipEmitter[] emitters)
        {
            for (int i=0; i<emitters.Length; i++)
            {
                AudioSource audioSource = emitters[i].GetSource();

                audioSource.spatialBlend = 0f;
                audioSource.spatialize = false;
            }
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
    }
}
