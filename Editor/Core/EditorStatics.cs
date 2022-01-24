using UnityEngine;

namespace Blip
{
    public static class EditorStatics
    {
        private static AudioSource editorPreviewAudioSource = null;

        public static bool IsPlaying = false;

        private static AudioSource CreatePreviewAudioSource()
        {
            GameObject PreviewAudioSourceObject = new GameObject("Preview Emitter [BlipKitEditor]");

            // "HideAndDontSave" means this doesn't appear in the scene hierarchy and wont save if
            // the user changes scenes or hits Play. However, the speaker icon still shows.
            // In the future, changing this to just 'DontSave' and letting the user manipulate the 
            // temporary emitter in the scene might be best, especially if testing attenuation in 
            // editor. In that case, the listener is likely the Editor camera, which isnt completely
            // intuitive.
            PreviewAudioSourceObject.hideFlags = HideFlags.HideAndDontSave;

            editorPreviewAudioSource = PreviewAudioSourceObject.AddComponent(typeof(AudioSource)) as AudioSource;
            return editorPreviewAudioSource;
        }

        public static AudioSource GetPreviewAudioSource()
        {
            if (editorPreviewAudioSource == null)
            {
                CreatePreviewAudioSource();
            }

            return editorPreviewAudioSource;
        }

        public static void PlayEventInPreviewAudioSource(BlipEvent eventToPlay)
        {
            AudioSource audioSource = GetPreviewAudioSource();
            
            // This is handled mare gracefully by the emitters in their Reset() called before an event plays.
            var lowPassFilterComponent = audioSource.gameObject.GetComponent<AudioLowPassFilter>();
            var highPassFilterComponent = audioSource.gameObject.GetComponent<AudioHighPassFilter>();

            if (!lowPassFilterComponent)
            {
                lowPassFilterComponent = audioSource.gameObject.AddComponent<AudioLowPassFilter>();
            }
            if (!highPassFilterComponent)
            {
                highPassFilterComponent = audioSource.gameObject.AddComponent<AudioHighPassFilter>();
            }

            lowPassFilterComponent.enabled = false;
            highPassFilterComponent.enabled = false;

            eventToPlay.PlayOnSingleSource(audioSource);

            IsPlaying = true;
        }

        public static void StopPreviewAudioSource()
        {
            AudioSource audioSource = GetPreviewAudioSource();

            audioSource.Stop();
            
            IsPlaying = false;
        }
    }
}
