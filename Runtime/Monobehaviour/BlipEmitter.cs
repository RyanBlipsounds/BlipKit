using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blip
{
    public class BlipEmitter : MonoBehaviour
    {
        private AudioSource audioSource;
        private Transform attachParent;
        private bool isActive = false;
        private bool isPaused = false;
        private bool isRequested = false;
        private int activePriority = -1;
        private float listenerDistance = 0f;
        private string currentEventName = null;

        // Action-specfic.
        private bool hasDistanceSpatial = false;
        private float spatialMax;
        private float distanceSpatialMin;
        private float distanceSpatialMax;
        private bool hasDistanceVolume = false;
        private float volumeMax;
        private float distanceVolumeMin;
        private float distanceVolumeMax;

        [HideInInspector]
        public AudioHighPassFilter HighPassFilter;

        [HideInInspector]
        public AudioLowPassFilter LowPassFilter;

        private void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            HighPassFilter = gameObject.AddComponent<AudioHighPassFilter>();
            LowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
        
        }

        private void LateUpdate()
        {
            if (attachParent)
            {
                // Follow an attached parent transform without actually attaching as a child, if
                // one has been set.
                transform.position = attachParent.position;
                transform.localRotation = attachParent.rotation;
            }

            if (isActive && !audioSource.isPlaying && !isPaused)
            {
                // Inactive emitters are eligable to be reused.
                isActive = false;
                isRequested = false;
                isPaused = false;
                hasDistanceSpatial = false;
                attachParent = null;
                activePriority = 0;

                return;
            }

            // Respond to listener distance, if necessary.
            if (hasDistanceSpatial || hasDistanceVolume)
            {
                listenerDistance = Vector3.Distance
                (
                    transform.position, 
                    Statics.GetListenerGameobject().transform.position
                );

                if (hasDistanceSpatial)
                {
                    UpdateDistanceSpatial();
                }

                if (hasDistanceVolume)
                {
                    UpdateDistanceVolume();
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

            if (hasDistanceSpatial)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, distanceSpatialMin);

                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, distanceSpatialMax);
            }

            if (hasDistanceVolume)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, distanceVolumeMin);

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, distanceVolumeMax);
            }
        }

        private void UpdateDistanceVolume()
        {
            audioSource.volume = Mathf.Clamp
            (
                Mathf.Clamp01
                (
                    1f - 
                    (listenerDistance - distanceVolumeMin) / 
                    (distanceVolumeMax - distanceVolumeMin)
                ),
                0f, 
                1f
            ) * volumeMax;

        }

        private void UpdateDistanceSpatial()
        {
            audioSource.spatialBlend = Mathf.Clamp
            (
                Mathf.Clamp01
                (
                    (listenerDistance - distanceSpatialMin) / 
                    (distanceSpatialMax - distanceSpatialMin)
                ),
                0f, 
                1f
            ) * spatialMax;
        }

        public bool Request(int priority)
        {
            if (!isRequested)
            {
                isRequested = true;
                activePriority = priority;
                return true;
            }

            return false;
        }

        public void GoToPosition(Vector3 position)
        {
            transform.position = position;
            attachParent = null;
        }

        public void AttachTo(GameObject attachTarget)
        {
            transform.position = attachTarget.transform.position;
            attachParent = attachTarget.transform;
        }

        public bool IsActive()
        {
            return isActive;
        }

        public bool IsRequested()
        {
            return isRequested;
        }

        public int GetActivePriority()
        {
            return activePriority;
        }

        public void PlayClip(AudioClip clip)
        {
            Reset();

            audioSource.clip = clip;
            audioSource.Play();
            // Note: We may want this split up for more control from Actions.

            isRequested = false;
            isActive = true;
        }

        public void Stop()
        {
            // TODO: Support fading.
            audioSource.Stop();
            isActive = false;
        }

        public void Pause()
        {
            // TODO: Support fading.
            audioSource.Pause();
            isPaused = true;
        }

        public void Unpause()
        {
            // TODO: Support fading.
            audioSource.UnPause();
            isPaused = false;
        }

        public void Mute()
        {
            // TODO: Support fading.
            audioSource.mute = true;
        }

        public void Unmute()
        {
            // TODO: Support fading.
            audioSource.mute = false;
        }

        public AudioSource GetSource()
        {
            return audioSource;
        }

        public void Reset()
        {
            LowPassFilter.enabled = false;
            HighPassFilter.enabled = false;
            hasDistanceSpatial = false;
            isPaused = false;
        }

        public void SetVolume(float volume)
        {
            audioSource.volume = volume;
        }

        public void SetPitchUnity(float pitch)
        {
            audioSource.pitch = pitch;
        }

        // Sets pitch by converting cents of semitone to Unity pitch.
        public void SetPitch(float pitchCentsOfSemitone)
        {
            audioSource.pitch = (float)Statics.PitchAsUnityRange(pitchCentsOfSemitone);
        }

        public void SetLowPassFilter(float cutoffFrequency, float resonanceQ)
        {            
            LowPassFilter.cutoffFrequency = cutoffFrequency; 
            LowPassFilter.lowpassResonanceQ = resonanceQ;

            LowPassFilter.enabled = true;
        }
        
        public void SetHighPassFilter(float cutoffFrequency, float resonanceQ)
        {
            HighPassFilter.cutoffFrequency = cutoffFrequency;
            HighPassFilter.highpassResonanceQ = resonanceQ;

            HighPassFilter.enabled = true;
        }

        public void SetVolumeByDistance(float minRange, float maxRange)
        {
            hasDistanceVolume = true;

            volumeMax = audioSource.volume;
            distanceVolumeMin = minRange;
            distanceVolumeMax = maxRange;
        }

        public void SetSpatialBlendByDistance(float minRange, float maxRange)
        {
            hasDistanceSpatial = true;

            spatialMax = audioSource.spatialBlend;
            distanceSpatialMin = minRange;
            distanceSpatialMax = maxRange;
        }

        // Called from statics when an emitter plays anything and contains the relevent event name. 
        // Used to search for emitters.
        public void SetCurrentEventName(string eventName)
        {
            currentEventName = eventName;
        }

        public string GetCurrentEventName()
        {
            if (!isActive)
            {
                return null;
            }

            return currentEventName;
        }

        public void DestroySelf()
        {
            Destroy(this);
        }
    }
}
