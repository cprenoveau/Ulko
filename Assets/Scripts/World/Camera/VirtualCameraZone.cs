using Cinemachine;
using System;
using System.Collections;
using UnityEngine;

namespace Ulko.World
{
    public abstract class VirtualCameraZone : Trigger
    {
        public CinemachineVirtualCamera vCam;

        public static VirtualCameraZone CurrentZone { get; private set; }

        public delegate void VirtualCameraTrigger(VirtualCameraZone currentZone, VirtualCameraZone newZone);
        public static event VirtualCameraTrigger OnTrigger;

        public delegate void VirtualCameraInitialized(VirtualCameraZone zone);
        public static event VirtualCameraInitialized OnInitialized;
        public static VirtualCameraZone FindCurrentZone(Player player)
        {
            var vCams = GameObject.FindObjectsByType<VirtualCameraZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var vCam in vCams)
            {
                if (vCam != CurrentZone && vCam.GetComponent<BoxCollider>().bounds.Contains(player.transform.position + Vector3.up * 0.5f))
                {
                    return vCam;
                }
            }

            return CurrentZone;
        }

        public override void OnEnter(Player player)
        {
            if (this == CurrentZone)
                return;

            OnTrigger?.Invoke(CurrentZone, this);
        }

        public override void OnExit(Player player)
        {
            if (CurrentZone != this)
                return;

            var zone = FindCurrentZone(player);
            if(CurrentZone != zone && zone != null)
            {
                OnTrigger?.Invoke(CurrentZone, zone);
            }
        }

        public void Init(Camera cam, Transform playerTransform, Limits limits, bool areaChanged)
        {
            StartCoroutine(InitAsync(cam, playerTransform, limits, areaChanged));
        }

        public IEnumerator InitAsync(Camera cam, Transform playerTransform, Limits limits, bool areaChanged)
        {
            CurrentZone = this;

            yield return null;
            _Init(cam, playerTransform, limits);

            //switch to this vcam
            var currentVCam = cam.GetComponent<CinemachineBrain>().ActiveVirtualCamera;
            if (currentVCam != null) currentVCam.Priority = 0;

            vCam.Priority = 1000;
            vCam.PreviousStateIsValid = false;

            var composer = vCam.GetCinemachineComponent<CinemachineComposer>();

            float hDamping = composer.m_HorizontalDamping;
            float vDamping = composer.m_VerticalDamping;
            composer.m_HorizontalDamping = 0;
            composer.m_VerticalDamping = 0;

            _UpdateCamera();

            if (areaChanged)
            {
                yield return null;
            }

            _UpdateCamera();

            vCam.PreviousStateIsValid = false;

            yield return null;
            yield return null;

            composer.m_HorizontalDamping = hDamping;
            composer.m_VerticalDamping= vDamping;

            yield return null;
            OrientToCamera.RefreshAll();

            OnInitialized?.Invoke(this);
        }

        private void Update()
        {
            _UpdateCamera();
        }

        protected abstract void _Init(Camera cam, Transform playerTransform, Limits limits);
        protected abstract void _UpdateCamera();
    }
}
