// <copyright file="Composer.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Game.Camera.Hybrid
{
    using Unity.Entities;
    using Unity.Mathematics;

    public struct Composer : IComponentData
    {
        public float3 TrackedObjectOffset;
        public float LookaheadTime;
        public float LookaheadSmoothing;
        public bool LookaheadIgnoreY;
        public float HorizontalDamping;
        public float VerticalDamping;
        public float ScreenX;
        public float ScreenY;
        public float DeadZoneWidth;
        public float DeadZoneHeight;
        public float SoftZoneWidth;
        public float SoftZoneHeight;
        public float BiasX;
        public float BiasY;
        public bool CenterOnActivate;
    }
}