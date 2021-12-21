// <copyright file="ThirdPersonFollow.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Game.Camera.Hybrid
{
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    public struct ThirdPersonFollow : IComponentData
    {
        public float3 Damping;
        public float3 ShoulderOffset;
        public float VerticalArmLength;
        public float CameraSide;
        public float CameraDistance;
        public LayerMask CameraCollisionFilter;
        public float CameraRadius;
        public float DampingIntoCollision;
        public float DampingFromCollision;
    }
}
