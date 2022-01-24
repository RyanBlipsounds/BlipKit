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
        private bool isRequested = false;
        private int activePriority = -1;

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

            if (isActive && !audioSource.isPlaying)
            {
                // Inactive emitters are eligable to be reused.
                isActive = false;
                isRequested = false;
                attachParent = null;
                activePriority = 0;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 10f);
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
            audioSource.Stop();
            isActive = false;
        }

        public AudioSource GetSource()
        {
            return audioSource;
        }

        public void Reset()
        {
            LowPassFilter.enabled = false;
            HighPassFilter.enabled = false;
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

            Debug.Log("Cutoff is: " + LowPassFilter.cutoffFrequency);
        }
        public void SetHighPassFilter(float cutoffFrequency, float resonanceQ)
        {
            HighPassFilter.cutoffFrequency = cutoffFrequency;
            HighPassFilter.highpassResonanceQ = resonanceQ;

            HighPassFilter.enabled = true;

            Debug.Log("Cutoff is: " + HighPassFilter.cutoffFrequency);
        }

        public void DestroySelf()
        {
            Destroy(this);
        }
    }
}
