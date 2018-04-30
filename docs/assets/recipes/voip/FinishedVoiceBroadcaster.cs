<!-- using directive to import the configuration parameters from step 3 -->
using Improbable.Audio;
using Improbable.Unity;
using Improbable.Worker;
using Improbable.Unity.Visualizer;
using UnityEngine;

namespace Assets.Gamelogic.Audio
{
    [RequireComponent(typeof(AudioSource))]

    // Only enable on clients
    [WorkerType(WorkerPlatform.UnityClient)]
    public class VoiceBroadcaster : MonoBehaviour
    {
        // This script will read from the AudioTransmission component
        [Require]
        private AudioTransmission.Reader AudioTransmissionReader;

        // An AudioSource that we'll play audio from
        [SerializeField] private AudioSource audioSource;
        // Keep track of the last sample in the audio played
        private int lastSamplePlayed;

        void OnEnable()
        {
            audioSource = gameObject.GetComponent<AudioSource>();

            // Register callbacks for `SendAudio` and `StopSendingAudio` events
            AudioTransmissionReader.SendAudioTriggered.Add(UpdateSoundSamples);
            AudioTransmissionReader.StopSendingAudioTriggered.Add(ResetSoundSamples);
        }

        void OnDisable()
        {
            // De-register callbacks for `SendAudio` and `StopSendingAudio` events
            AudioTransmissionReader.SendAudioTriggered.Remove(UpdateSoundSamples);
            AudioTransmissionReader.StopSendingAudioTriggered.Remove(ResetSoundSamples);
        }

        void UpdateSoundSamples(SendAudio sound)
        {
            // Don't do anything if this is on the current player (which we have write access to)
            if (AudioTransmissionReader.Authority == Authority.Authoritative)
            {
                return;
            }

            // If the clip isn't playing, it needs initialising
            if (!audioSource.isPlaying)
            {
                InitialiseAudioSource();
            }

            // Add the new audio data at the set offset
            audioSource.clip.SetData(sound.samples.ToArray(), lastSamplePlayed);

            // If the clip isn't already playing, set it off
            if (!audioSource.isPlaying)
            {
                audioSource.PlayDelayed(0.1f);
            }

            // If audio clip buffer overflowed, we wrap around from the start
            lastSamplePlayed = (lastSamplePlayed + sound.samples.Count) % SimulationSettings.MaxAudioClipSamples;
        }

        private void InitialiseAudioSource()
        {
            // Reset the lastSample tracker
            lastSamplePlayed = 0;
            // Create a new, blank audio clip for the voice input to be written to
            audioSource.clip = AudioClip.Create("TransmittedAudio", SimulationSettings.MaxAudioClipSamples, SimulationSettings.AudioTransmissionChannels, SimulationSettings.Frequency, false);
        }

        private void ResetSoundSamples(StopSendingAudio obj)
        {
            // When the StopSendingAudio event comes through, stop playing the clip, and delete it
            audioSource.Stop();
            audioSource.clip = null;
        }
    }
}
