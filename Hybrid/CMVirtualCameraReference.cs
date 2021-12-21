// <copyright file="CMVirtualCameraReference.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Game.Camera.Hybrid
{
    using Cinemachine;
    using Unity.Entities;
    using UnityEngine;

    public class CMVirtualCameraReference : ISystemStateComponentData
    {
        public CinemachineVirtualCamera Value;
        public Transform Follow;
        public Transform LookAt;
    }
}
