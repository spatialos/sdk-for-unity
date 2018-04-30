<!-- using directive to import the configuration parameters from step 3 -->
using Improbable.Audio;
using Improbable.Collections;
using Improbable.Unity;
using Improbable.Unity.Visualizer;
using UnityEngine;

namespace Assets.Gamelogic.Audio
{
    // Only enable on clients
    [WorkerType(WorkerPlatform.UnityClient)]
    public class VoiceInputHandler : MonoBehaviour
    {
        // This script will write to the AudioTransmission component
        [Require]
        private AudioTransmission.Writer AudioTransmissionWriter;

        // An empty AudioClip that we'll write to when the player triggers it
        private AudioClip clipToTransmit;
        // Set as the last point in the AudioClip that we sent data for
        private int lastSampleOffset;

        private bool isMicOn;
        private bool keyPressed;

        private void Update(){
            if (Input.GetKeyDown(KeyCode.Space) && !keyPressed) {
                TransmitAudio();
                keyPressed = true;
            }
            if (Input.GetKeyUp(KeyCode.Space) && keyPressed) {
                StopTransmittingAudio();
                keyPressed = false;
            }
        }

        private void FixedUpdate()
        {
            if (isMicOn)
            {
                // Check where the microphone is at in the sample list
                int currentMicSamplePosition = Microphone.GetPosition(null);
                // Find out how many samples we need to send in this update
                int samplesToTransmit = CalculateSampleTransmissionCount(currentMicSamplePosition);
                if (samplesToTransmit > 0)
                {
                    // Send the samples, and update the offset
                    TransmitSamples(samplesToTransmit);
                    lastSampleOffset = currentMicSamplePosition;
                }
            }
        }

        private void TransmitAudio()
        {
            if (!isMicOn)
            {
                // Start recording from the default microphone
                clipToTransmit = Microphone.Start(null, true, 10, SimulationSettings.Frequency);
                // Reset the offset to 0, as we're starting the clip again
                lastSampleOffset = 0;
                isMicOn = true;
            }
        }

        private void StopTransmittingAudio()
        {
            if (isMicOn)
            {
                isMicOn = false;
                // Stop recording from the default microphone
                Microphone.End(null);
                AudioTransmissionWriter.Send(new AudioTransmission.Update().AddStopSendingAudio(new StopSendingAudio()));
            }
        }

        private int CalculateSampleTransmissionCount(int currentMicrophoneSample)
        {
            // Work out how many samples to send in this transmission
            int sampleCountToTransmit = currentMicrophoneSample - lastSampleOffset;
            if (sampleCountToTransmit < 0) // lastSampleOffset overflew the microphone buffer.
            {
                sampleCountToTransmit = (clipToTransmit.samples - lastSampleOffset) + currentMicrophoneSample;
            }
            return sampleCountToTransmit;
        }

        private void TransmitSamples(int sampleCountToTransmit)
        {
            float[] samplesToTransmit = new float[sampleCountToTransmit * clipToTransmit.channels];
            clipToTransmit.GetData(samplesToTransmit, lastSampleOffset);
            AudioTransmissionWriter.Send(new AudioTransmission.Update().AddSendAudio(new SendAudio(new List<float>(samplesToTransmit))));
        }
    }
}