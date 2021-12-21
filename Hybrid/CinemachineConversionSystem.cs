// <copyright file="CinemachineConversionSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Game.Camera.Hybrid
{
    using Cinemachine;
    using Unity.Entities;

    public class CinemachineConversionSystem : GameObjectConversionSystem
    {
        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            this.Entities.ForEach((CinemachineBrain brain) =>
            {
                var entity = this.GetPrimaryEntity(brain);
                this.DstEntityManager.AddComponent<CMBrain>(entity);
            });

            this.Entities.ForEach((CinemachineVirtualCamera camera) =>
            {
                var entity = this.GetPrimaryEntity(camera);

                var follow = camera.Follow != null ? this.GetPrimaryEntity(camera.Follow) : Entity.Null;
                var lookAt = camera.LookAt != null ? this.GetPrimaryEntity(camera.LookAt) : Entity.Null;

                var virtualCamera = new CMVirtualCamera
                {
                    Enabled = false, // Annoyingly entities ignores disabled component so they must all start enabled before conversion
                    Follow = follow,
                    LookAt = lookAt,
                    FieldOfView = camera.m_Lens.FieldOfView,
                    NearClipPlane = camera.m_Lens.NearClipPlane,
                    FarClipPlane = camera.m_Lens.FarClipPlane,
                };

                this.DstEntityManager.AddComponentData(entity, virtualCamera);

                var transposer = camera.GetCinemachineComponent<CinemachineTransposer>();
                if (transposer != null)
                {
                    this.DstEntityManager.AddComponentData(entity, new Transposer
                    {
                        BindingMode = transposer.m_BindingMode,
                        FollowOffset = transposer.m_FollowOffset,
                        XDamping = transposer.m_XDamping,
                        YDamping = transposer.m_YDamping,
                        ZDamping = transposer.m_ZDamping,
                        AngularDampingMode = transposer.m_AngularDampingMode,
                        PitchDamping = transposer.m_PitchDamping,
                        YawDamping = transposer.m_YawDamping,
                        RollDamping = transposer.m_RollDamping,
                        AngularDamping = transposer.m_AngularDamping,
                    });
                }

                var composer = camera.GetCinemachineComponent<CinemachineComposer>();
                if (composer != null)
                {
                    this.DstEntityManager.AddComponentData(entity, new Composer
                    {
                        TrackedObjectOffset = composer.m_TrackedObjectOffset,
                        LookaheadTime = composer.m_LookaheadTime,
                        LookaheadSmoothing = composer.m_LookaheadSmoothing,
                        LookaheadIgnoreY = composer.m_LookaheadIgnoreY,
                        HorizontalDamping = composer.m_HorizontalDamping,
                        VerticalDamping = composer.m_VerticalDamping,
                        ScreenX = composer.m_ScreenX,
                        ScreenY = composer.m_ScreenY,
                        DeadZoneWidth = composer.m_DeadZoneWidth,
                        DeadZoneHeight = composer.m_DeadZoneHeight,
                        SoftZoneWidth = composer.m_SoftZoneWidth,
                        SoftZoneHeight = composer.m_SoftZoneHeight,
                        BiasX = composer.m_BiasX,
                        BiasY = composer.m_BiasY,
                        CenterOnActivate = composer.m_CenterOnActivate,
                    });
                }

                var thirdPerson = camera.GetCinemachineComponent<Cinemachine3rdPersonFollowDots>();
                if (thirdPerson != null)
                {
                    this.DstEntityManager.AddComponentData(entity, new ThirdPersonFollow
                    {
                        Damping = thirdPerson.Damping,
                        ShoulderOffset = thirdPerson.ShoulderOffset,
                        VerticalArmLength = thirdPerson.VerticalArmLength,
                        CameraSide = thirdPerson.CameraSide,
                        CameraDistance = thirdPerson.CameraDistance,
                        CameraCollisionFilter = thirdPerson.CameraCollisionFilter,
                        CameraRadius = thirdPerson.CameraRadius,
                        DampingIntoCollision = thirdPerson.DampingIntoCollision,
                        DampingFromCollision = thirdPerson.DampingFromCollision,
                    });
                }
            });
        }
    }
}
