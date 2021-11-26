using UnityEngine;

namespace Blip
{
    public static class EditorStatics
    {
        private static AudioSource editorPreviewAudioSource = null;

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

        public static void PlayClipInPreviewAudioSource
        (
            AudioClip clip, 
            float volume=1f, 
            float pitch =1f,
            bool isLooping = false
        )
        {
            AudioSource audioSource = GetPreviewAudioSource();

            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.loop = isLooping;
            audioSource.Play();
        }

        public static void StopPreviewAudioSource()
        {
            AudioSource audioSource = GetPreviewAudioSource();

            audioSource.Stop();
        }
    }
}
