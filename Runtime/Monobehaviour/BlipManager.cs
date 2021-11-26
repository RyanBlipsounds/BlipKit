using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blip
{
    public class BlipManager : MonoBehaviour
    {
        private static BlipManager main = null;

        public BlipSettings Settings;
        public bool InitializeOnAwake = true;

        private void Awake()
        {
            if (main != null)
            {
                Debug.LogError("[BlipKit] Multiple BlipManagers detected.");
                Destroy(this);
                return;
            }

            main = this;

            if (InitializeOnAwake)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            AudioListener activeListener = FindObjectOfType<AudioListener>();

            Statics.Initialize(Settings, activeListener);

            // There might need to be some other initialization here eventually.
        }

        public static BlipManager Get()
        {
            return main;
        }
    }
}
