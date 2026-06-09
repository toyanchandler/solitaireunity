using System;
using System.Collections;
using System.Collections.Generic;
using _Game.Scripts.Helper.Extensions.System;
using Cinemachine;
using UnityEngine;

namespace Handler.Extensions
{
    public static class CinemachineExtensions
    {
        public static void SetCameraActive(this Cinemachine.CinemachineVirtualCameraBase self, bool active) => self.Priority = active ? 1 : 0;
        
        public static void SetFollowTarget(this Cinemachine.CinemachineVirtualCameraBase self, Transform target) => self.Follow = target;
        
        public static void SetLookAtTarget(this Cinemachine.CinemachineVirtualCameraBase self, Transform target) => self.LookAt = target;

        public static void SetFov(this Cinemachine.CinemachineVirtualCameraBase self, float fov)
        {
            var vm = self.GetComponent<Cinemachine.CinemachineVirtualCamera>();
            vm.m_Lens.FieldOfView = fov;
        }
        
        public static void BodyFollowOffset(this Cinemachine.CinemachineVirtualCameraBase self, Vector3 offset)
        {
            var body = self.GetComponent<Cinemachine.CinemachineTransposer>();
            body.m_FollowOffset = offset;
        }
        
        public static void SetXDamper(this Cinemachine.CinemachineVirtualCameraBase self, float damper)
        {
            var body = self.GetComponent<Cinemachine.CinemachineTransposer>();
            body.m_XDamping = damper;
        }
        
        public static void SetYDamper(this Cinemachine.CinemachineVirtualCameraBase self, float damper)
        {
            var body = self.GetComponent<Cinemachine.CinemachineTransposer>();
            body.m_YDamping = damper;
        }
        
        public static void SetZDamper(this Cinemachine.CinemachineVirtualCameraBase self, float damper)
        {
            var body = self.GetComponent<Cinemachine.CinemachineTransposer>();
            body.m_ZDamping = damper;
        }
        
        public static void SetScreenX(this Cinemachine.CinemachineVirtualCameraBase self, float x)
        {
            var body = self.GetComponent<Cinemachine.CinemachineComposer>();
            body.m_ScreenX = x;
        }
        
        public static void SetScreenY(this Cinemachine.CinemachineVirtualCameraBase self, float y)
        {
            var body = self.GetComponent<Cinemachine.CinemachineComposer>();
            body.m_ScreenY = y;
        }
        
        public static void SetDeadZoneWidth(this Cinemachine.CinemachineVirtualCameraBase self, float width)
        {
            var body = self.GetComponent<Cinemachine.CinemachineComposer>();
            body.m_DeadZoneWidth = width;
        }
        
        public static void SetDeadZoneHeight(this Cinemachine.CinemachineVirtualCameraBase self, float height)
        {
            var body = self.GetComponent<Cinemachine.CinemachineComposer>();
            body.m_DeadZoneHeight = height;
        }
        
        public static void SetSoftZoneWidth(this Cinemachine.CinemachineVirtualCameraBase self, float width)
        {
            var body = self.GetComponent<Cinemachine.CinemachineComposer>();
            body.m_SoftZoneWidth = width;
        }
        
        public static void SetSoftZoneHeight(this Cinemachine.CinemachineVirtualCameraBase self, float height)
        {
            var body = self.GetComponent<Cinemachine.CinemachineComposer>();
            body.m_SoftZoneHeight = height;
        }
        
        public static void SetNoiseAmplitude(this Cinemachine.CinemachineVirtualCameraBase self, float amplitude)
        {
            var body = self.GetComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
            body.m_AmplitudeGain = amplitude;
        }
        
        public static void SetNoiseFrequency(this Cinemachine.CinemachineVirtualCameraBase self, float frequency)
        {
            var body = self.GetComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
            body.m_FrequencyGain = frequency;
        }
        
        private static Coroutine currentCoroutine = null;

        public static void SafeStartNoise(this CinemachineVirtualCamera vcam, float amplitudeGain, float frequencyGain, float duration, MonoBehaviour coroutineOwner)
        {
            // Stop any existing noise coroutines
            if (currentCoroutine != null)
            {
                coroutineOwner.StopCoroutine(currentCoroutine);
            }

            // Start new noise coroutine
            currentCoroutine = coroutineOwner.StartCoroutine(ApplyNoise(vcam, amplitudeGain, frequencyGain, duration));
        }
        
        public static void SafeStopNoise(this CinemachineVirtualCamera vcam, MonoBehaviour coroutineOwner)
        {
            // Stop any existing noise coroutines
            if (currentCoroutine != null)
            {
                coroutineOwner.StopCoroutine(currentCoroutine);
            }
        }
        
        private static IEnumerator ApplyNoise(CinemachineVirtualCamera vcam, float amplitudeGain, float frequencyGain, float duration)
        {
            var noiseProfile = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            if (noiseProfile != null)
            {
                // Save initial noise settings
                float initialAmplitude = noiseProfile.m_AmplitudeGain;
                float initialFrequency = noiseProfile.m_FrequencyGain;

                // Set new noise settings
                noiseProfile.m_AmplitudeGain = amplitudeGain;
                noiseProfile.m_FrequencyGain = frequencyGain;

                // Wait for the specified duration
                yield return new WaitForSeconds(duration);

                // Revert to initial noise settings
                noiseProfile.m_AmplitudeGain = initialAmplitude;
                noiseProfile.m_FrequencyGain = initialFrequency;
            }
            else
            {
                TDebug.LogWarning("CinemachineBasicMultiChannelPerlin component not found on the virtual camera.");
            }

            currentCoroutine = null;
        }
    }
}