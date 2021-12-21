// <copyright file="CinemachineSyncSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Game.Camera.Hybrid
{
    using Cinemachine;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Physics.Systems;
    using UnityEngine;

    [UpdateInGroup(typeof(CameraSystemGroup))]
    public class CinemachineSyncSystem : SystemBase
    {
        private EntityQuery virtualCameraQuery;
        private EntityQuery transposerQuery;
        private EntityQuery composerQuery;
        private EntityQuery thirdPersonFollowQuery;

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            this.VirtualCameraChanged();
            this.TransposerChanged();
            this.ComposerChanged();
            this.ThirdPersonCameraChanged();
        }

        private void VirtualCameraChanged()
        {
            if (this.virtualCameraQuery.IsEmpty)
            {
                return;
            }

            this.Entities
                .WithChangeFilter<CMVirtualCamera>()
                .ForEach((CinemachineVirtualCamera cam, in CMVirtualCamera component) =>
                {
                    cam.enabled = component.Enabled;

                    cam.Follow = this.EntityManager.HasComponent<Transform>(component.Follow)
                        ? this.EntityManager.GetComponentObject<Transform>(component.Follow)
                        : null;

                    cam.LookAt = this.EntityManager.HasComponent<Transform>(component.LookAt)
                        ? this.EntityManager.GetComponentObject<Transform>(component.LookAt)
                        : null;

                    var lens = cam.m_Lens;
                    lens.FieldOfView = math.clamp(component.FieldOfView, 1, 179);
                    lens.NearClipPlane = component.NearClipPlane;
                    lens.FarClipPlane = component.FarClipPlane;
                    cam.m_Lens = lens;
                })
                .WithStoreEntityQueryInField(ref this.virtualCameraQuery)
                .WithoutBurst()
                .Run();
        }

        private void TransposerChanged()
        {
            if (this.transposerQuery.IsEmpty)
            {
                return;
            }

            this.Entities
                .WithChangeFilter<Transposer>()
                .ForEach((CinemachineTransposer transposer, in Transposer component) =>
                {
                    transposer.m_BindingMode = component.BindingMode;
                    transposer.m_FollowOffset = component.FollowOffset;
                    transposer.m_XDamping = math.clamp(component.XDamping, 0, 20);
                    transposer.m_YDamping = math.clamp(component.YDamping, 0, 20);
                    transposer.m_ZDamping = math.clamp(component.ZDamping, 0, 20);
                    transposer.m_AngularDampingMode = component.AngularDampingMode;
                    transposer.m_PitchDamping = math.clamp(component.PitchDamping, 0, 20);
                    transposer.m_YawDamping = math.clamp(component.YawDamping, 0, 20);
                    transposer.m_RollDamping = math.clamp(component.RollDamping, 0, 20);
                    transposer.m_AngularDamping = math.clamp(component.AngularDamping, 0, 20);
                })
                .WithStoreEntityQueryInField(ref this.transposerQuery)
                .WithoutBurst()
                .Run();
        }

        private void ComposerChanged()
        {
            if (this.composerQuery.IsEmpty)
            {
                return;
            }

            this.Entities
                .WithChangeFilter<Composer>()
                .ForEach((CinemachineComposer composer, in Composer component) =>
                {
                    composer.m_TrackedObjectOffset = component.TrackedObjectOffset;
                    composer.m_LookaheadTime = component.LookaheadTime;
                    composer.m_LookaheadSmoothing = component.LookaheadSmoothing;
                    composer.m_LookaheadIgnoreY = component.LookaheadIgnoreY;
                    composer.m_HorizontalDamping = component.HorizontalDamping;
                    composer.m_VerticalDamping = component.VerticalDamping;
                    composer.m_ScreenX = component.ScreenX;
                    composer.m_ScreenY = component.ScreenY;
                    composer.m_DeadZoneWidth = component.DeadZoneWidth;
                    composer.m_DeadZoneHeight = component.DeadZoneHeight;
                    composer.m_SoftZoneWidth = component.SoftZoneWidth;
                    composer.m_SoftZoneHeight = component.SoftZoneHeight;
                    composer.m_BiasX = component.BiasX;
                    composer.m_BiasY = component.BiasY;
                    composer.m_CenterOnActivate = component.CenterOnActivate;
                })
                .WithStoreEntityQueryInField(ref this.composerQuery)
                .WithoutBurst()
                .Run();
        }

        private void ThirdPersonCameraChanged()
        {
            if (this.thirdPersonFollowQuery.IsEmpty)
            {
                return;
            }

            var physicsWorld = this.World.GetExistingSystem<BuildPhysicsWorld>();

            this.Entities
                .WithChangeFilter<ThirdPersonFollow>()
                .ForEach((Cinemachine3rdPersonFollowDots cam, in ThirdPersonFollow component) =>
                {
                    cam.Damping = component.Damping;
                    cam.ShoulderOffset = component.ShoulderOffset;
                    cam.VerticalArmLength = component.VerticalArmLength;
                    cam.CameraSide = math.clamp(component.CameraSide, 0, 1);
                    cam.CameraDistance = component.CameraDistance;
                    cam.CameraCollisionFilter = component.CameraCollisionFilter;
                    cam.CameraRadius = component.CameraRadius;
                    cam.DampingIntoCollision = component.DampingIntoCollision;
                    cam.DampingFromCollision = component.DampingFromCollision;
                    cam.PhysicsWorld = physicsWorld;
                })
                .WithStoreEntityQueryInField(ref this.thirdPersonFollowQuery)
                .WithoutBurst()
                .Run();
        }
    }
}
