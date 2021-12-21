// <copyright file="CameraSystemGroup.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Game.Camera
{
    using Unity.Entities;

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class CameraSystemGroup : ComponentSystemGroup
    {
    }
}
