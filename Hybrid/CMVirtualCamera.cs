// <copyright file="CMVirtualCamera.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Game.Camera.Hybrid
{
    using Unity.Entities;

    public struct CMVirtualCamera : IComponentData
    {
        public static readonly CMVirtualCamera Default = new()
        {
            FieldOfView = 90,
            NearClipPlane = 0.1f,
            FarClipPlane = 1000f,
        };

        public bool Enabled;

        public Entity Follow;
        public Entity LookAt;

        // Lens
        public float FieldOfView; // 1 to 179
        public float NearClipPlane;
        public float FarClipPlane;
    }
}
