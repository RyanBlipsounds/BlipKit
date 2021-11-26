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

        private void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
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
            audioSource.clip = clip;
            audioSource.Play();
            // Note: We may want this split up for more control from Actions.

            isRequested = false;
            isActive = true;
        }

        public AudioSource GetSource()
        {
            return audioSource;
        }

        public void DestroySelf()
        {
            Destroy(this);
        }
    }
}
