// <copyright file="CinemachineBrainReference.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Game.Camera.Hybrid
{
    using Cinemachine;
    using Unity.Entities;

    public class CMBrainReference : ISystemStateComponentData
    {
        public CinemachineBrain Value;
    }
}
