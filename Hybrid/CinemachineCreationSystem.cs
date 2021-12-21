// <copyright file="CinemachineCreationSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Game.Camera.Hybrid
{
    using Cinemachine;
    using Unity.Entities;
    using Unity.Transforms;
    using UnityEngine;

    [UpdateInGroup(typeof(CameraSystemGroup))]
    [UpdateBefore(typeof(CinemachineSyncSystem))]
    public class CinemachineCreationSystem : SystemBase
    {
        private const HideFlags HideFlags = UnityEngine.HideFlags.HideInHierarchy | UnityEngine.HideFlags.NotEditable | UnityEngine.HideFlags.DontSaveInBuild |
                                            UnityEngine.HideFlags.DontSaveInEditor | UnityEngine.HideFlags.DontUnloadUnusedAsset;

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            this.CreateBrainObjects();
            this.CreateCameraObjects();
            this.DeleteOldBrains();
            this.DeleteOldCameras();
        }

        private void CreateBrainObjects()
        {
            this.Entities
                .WithNone<CinemachineBrain>()
                .WithAll<CMBrain>()
                .ForEach((Entity entity, Camera camera) =>
                {
                    var brain = camera.gameObject.AddComponent<CinemachineBrain>();

                    this.EntityManager.AddComponents(entity, new ComponentTypes(
                        typeof(CinemachineBrain),
                        typeof(CMBrainReference),
                        typeof(Transform),
                        typeof(CopyTransformFromGameObject)));

                    this.EntityManager.AddComponentObject(entity, brain);
                    this.EntityManager.AddComponentObject(entity, new CMBrainReference { Value = brain });
                    this.EntityManager.AddComponentObject(entity, brain.transform);
                })
                .WithStructuralChanges().Run();
        }

        private void CreateCameraObjects()
        {
            this.Entities
                .WithNone<CinemachineVirtualCamera>()
                .ForEach((Entity entity, in CMVirtualCamera vcam) =>
                {
                    var name = $"CinemachineVirtualCamera {entity}";
                    var go = new GameObject(name);
                    go.hideFlags |= HideFlags;

                    if (this.HasComponent<CopyTransformToGameObject>(entity) || this.HasComponent<CopyTransformFromGameObject>(entity))
                    {
                        this.EntityManager.AddComponentObject(entity, go.transform);
                    }

                    var virtualCamera = go.AddComponent<CinemachineVirtualCamera>();
                    virtualCamera.enabled = false;
                    this.EntityManager.AddComponentObject(entity, virtualCamera);

                    var reference = new CMVirtualCameraReference
                    {
                        Value = virtualCamera,
                    };

                    if (vcam.Follow != Entity.Null)
                    {
                        if (GetOrCreateTransform(this.EntityManager, vcam.Follow, out var tr))
                        {
                            reference.Follow = tr;
                        }

                        virtualCamera.Follow = tr;
                    }

                    if (vcam.LookAt != Entity.Null)
                    {
                        if (GetOrCreateTransform(this.EntityManager, vcam.LookAt, out var tr))
                        {
                            reference.LookAt = tr;
                        }

                        virtualCamera.LookAt = tr;
                    }

                    if (this.HasComponent<Transposer>(entity))
                    {
                        var component = virtualCamera.AddCinemachineComponent<CinemachineTransposer>();
                        this.EntityManager.AddComponentObject(entity, component);
                    }
                    else if (this.HasComponent<ThirdPersonFollow>(entity))
                    {
                        var component = virtualCamera.AddCinemachineComponent<Cinemachine3rdPersonFollowDots>();
                        this.EntityManager.AddComponentObject(entity, component);
                    }

                    if (this.HasComponent<Composer>(entity))
                    {
                        var component = virtualCamera.AddCinemachineComponent<CinemachineComposer>();
                        this.EntityManager.AddComponentObject(entity, component);
                    }

                    this.EntityManager.AddComponentData(entity, new CMVirtualCameraReference
                    {
                        Value = virtualCamera,
                        Follow = virtualCamera.Follow,
                        LookAt = virtualCamera.LookAt,
                    });

                    virtualCamera.InvalidateComponentPipeline();

                    this.EntityManager.AddComponentData(entity, reference);
                })
                .WithStructuralChanges().Run();
        }

        private void DeleteOldBrains()
        {
            // Delete old managed objects
            this.Entities.WithNone<CMBrain>()
                .ForEach((Entity entity, CMBrainReference brainReference) =>
                {
                    if (brainReference.Value != null)
                    {
                        Object.Destroy(brainReference.Value.gameObject);
                    }

                    this.EntityManager.RemoveComponent<CMBrainReference>(entity);
                })
                .WithStructuralChanges().Run();
        }

        private void DeleteOldCameras()
        {
            // Delete old managed objects
            this.Entities.WithNone<CMVirtualCamera>()
                .ForEach((Entity entity, CMVirtualCameraReference cameraReference) =>
                {
                    if (cameraReference.Value != null)
                    {
                        Object.Destroy(cameraReference.Value.gameObject);
                    }

                    if (cameraReference.Follow != null)
                    {
                        Object.Destroy(cameraReference.Follow.gameObject);
                    }

                    if (cameraReference.LookAt != null)
                    {
                        Object.Destroy(cameraReference.LookAt.gameObject);
                    }

                    this.EntityManager.RemoveComponent<CMVirtualCameraReference>(entity);
                })
                .WithStructuralChanges().Run();
        }

        /// <returns> False if a transform already exists or true if we've created a new one. </returns>
        private static bool GetOrCreateTransform(EntityManager em, Entity entity, out Transform transform)
        {
            em.AddComponent<CopyTransformToGameObject>(entity);

            if (em.HasComponent<Transform>(entity))
            {
                transform = em.GetComponentObject<Transform>(entity);
                return false;
            }

            var go = new GameObject(entity.ToString());
            go.hideFlags |= HideFlags;
            em.AddComponentObject(entity, go.transform);
            transform = go.transform;
            return true;
        }
    }
}
