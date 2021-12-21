// <copyright file="Transposer.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Game.Camera.Hybrid
{
    using Cinemachine;
    using Unity.Entities;
    using Unity.Mathematics;

    public struct Transposer : IComponentData
    {
        public CinemachineTransposer.BindingMode BindingMode;
        public float3 FollowOffset;
        public float XDamping;
        public float YDamping;
        public float ZDamping;
        public CinemachineTransposer.AngularDampingMode AngularDampingMode;
        public float PitchDamping;
        public float YawDamping;
        public float RollDamping;
        public float AngularDamping;
    }
}