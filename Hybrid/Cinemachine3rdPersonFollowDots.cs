// <copyright file="Cinemachine3rdPersonFollowDots.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

// Copyright © 2020 Unity Technologies ApS
// Licensed under the Unity Companion License for Unity-dependent projects--see Unity Companion License.
// Unless expressly provided otherwise, the Software under this license is made available strictly on an “AS IS” BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED. Please review the license for details on these and other terms and conditions.

namespace BovineLabs.Game.Camera.Hybrid
{
    using Cinemachine;
    using Cinemachine.Utility;
    using Unity.Physics;
    using Unity.Physics.Systems;
    using UnityEngine;

    /// <summary>
    /// Third-person follower, with complex pivoting: horizontal about the origin,
    /// vertical about the shoulder.
    /// </summary>
    [AddComponentMenu("")] // Don't display in add component menu
    [SaveDuringPlay]
    public class Cinemachine3rdPersonFollowDots : CinemachineComponentBase
    {
        /// <summary>How responsively the camera tracks the target.  Each axis (camera-local)
        /// can have its own setting.  Value is the approximate time it takes the camera
        /// to catch up to the target's new position.  Smaller values give a more rigid
        /// effect, larger values give a squishier one.</summary>
        [Tooltip("How responsively the camera tracks the target.  Each axis (camera-local) "
           + "can have its own setting.  Value is the approximate time it takes the camera "
           + "to catch up to the target's new position.  Smaller values give a more "
           + "rigid effect, larger values give a squishier one")]
        public Vector3 Damping;

        /// <summary>Position of the shoulder pivot relative to the Follow target origin.
        /// This offset is in target-local space.</summary>
        [Header("Rig")]
        [Tooltip("Position of the shoulder pivot relative to the Follow target origin.  "
            + "This offset is in target-local space")]
        public Vector3 ShoulderOffset;

        /// <summary>Vertical offset of the hand in relation to the shoulder.
        /// Arm length will affect the follow target's screen position
        /// when the camera rotates vertically.</summary>
        [Tooltip("Vertical offset of the hand in relation to the shoulder.  "
            + "Arm length will affect the follow target's screen position when "
            + "the camera rotates vertically")]
        public float VerticalArmLength;

        /// <summary>Specifies which shoulder (left, right, or in-between) the camera is on.</summary>
        [Tooltip("Specifies which shoulder (left, right, or in-between) the camera is on")]
        [Range(0, 1)]
        public float CameraSide;

        /// <summary>How far baehind the hand the camera will be placed.</summary>
        [Tooltip("How far baehind the hand the camera will be placed")]
        public float CameraDistance;

        /// <summary>Camera will avoid obstacles on these layers.</summary>
        [Header("Obstacles")]
        [Tooltip("Camera will avoid obstacles on these layers")]
        public LayerMask CameraCollisionFilter;

        /// <summary>
        /// Specifies how close the camera can get to obstacles
        /// </summary>
        [Tooltip("Specifies how close the camera can get to obstacles")]
        [Range(0, 1)]
        public float CameraRadius;

        /// <summary>
        /// How gradually the camera moves to correct for occlusions.
        /// Higher numbers will move the camera more gradually.
        /// </summary>
        [Range(0, 10)]
        [Tooltip("How gradually the camera moves to correct for occlusions.  " +
            "Higher numbers will move the camera more gradually.")]
        public float DampingIntoCollision;

        /// <summary>
        /// How gradually the camera returns to its normal position after having been corrected by the built-in
        /// collision resolution system. Higher numbers will move the camera more gradually back to normal.
        /// </summary>
        [Range(0, 10)]
        [Tooltip("How gradually the camera returns to its normal position after having been corrected by the built-in " +
            "collision resolution system.  Higher numbers will move the camera more gradually back to normal.")]
        public float DampingFromCollision;

        // State info
        Vector3 m_PreviousFollowTargetPosition;
        Vector3 m_DampingCorrection; // this is in local rig space
        float m_CamPosCollisionCorrection;

        internal BuildPhysicsWorld PhysicsWorld { get; set; }

        void OnValidate()
        {
            this.CameraSide = Mathf.Clamp(this.CameraSide, -1.0f, 1.0f);
            this.Damping.x = Mathf.Max(0, this.Damping.x);
            this.Damping.y = Mathf.Max(0, this.Damping.y);
            this.Damping.z = Mathf.Max(0, this.Damping.z);
            this.CameraRadius = Mathf.Max(0.001f, this.CameraRadius);
            this.DampingIntoCollision = Mathf.Max(0, this.DampingIntoCollision);
            this.DampingFromCollision = Mathf.Max(0, this.DampingFromCollision);
        }

        void Reset()
        {
            this.ShoulderOffset = new Vector3(0.5f, -0.4f, 0.0f);
            this.VerticalArmLength = 0.4f;
            this.CameraSide = 1.0f;
            this.CameraDistance = 2.0f;
            this.Damping = new Vector3(0.1f, 0.5f, 0.3f);
            this.CameraCollisionFilter = 0;
            this.CameraRadius = 0.2f;
            this.DampingIntoCollision = 0;
            this.DampingFromCollision = 2f;
        }

        /// <summary>True if component is enabled and has a Follow target defined</summary>
        public override bool IsValid => this.enabled && this.FollowTarget != null;

        /// <summary>Get the Cinemachine Pipeline stage that this component implements.
        /// Always returns the Aim stage</summary>
        public override CinemachineCore.Stage Stage { get { return CinemachineCore.Stage.Body; } }

        /// <summary>
        /// Report maximum damping time needed for this component.
        /// </summary>
        /// <returns>Highest damping setting in this component</returns>
        public override float GetMaxDampTime()
        {
            return Mathf.Max(
                Mathf.Max(this.DampingIntoCollision, this.DampingFromCollision),
                Mathf.Max(this.Damping.x, Mathf.Max(this.Damping.y, this.Damping.z)));
        }

        /// <summary>Orients the camera to match the Follow target's orientation</summary>
        /// <param name="curState">The current camera state</param>
        /// <param name="deltaTime">Elapsed time since last frame, for damping calculations.
        /// If negative, previous state is reset.</param>
        public override void MutateCameraState(ref Cinemachine.CameraState curState, float deltaTime)
        {
            if (this.IsValid)
            {
                if (!this.VirtualCamera.PreviousStateIsValid)
                    deltaTime = -1;
                this.PositionCamera(ref curState, deltaTime);
            }
        }

        /// <summary>This is called to notify the us that a target got warped,
        /// so that we can update its internal state to make the camera
        /// also warp seamlessy.</summary>
        /// <param name="target">The object that was warped</param>
        /// <param name="positionDelta">The amount the target's position changed</param>
        public override void OnTargetObjectWarped(Transform target, Vector3 positionDelta)
        {
            base.OnTargetObjectWarped(target, positionDelta);
            if (target == this.FollowTarget)
            {
                this.m_PreviousFollowTargetPosition += positionDelta;
            }
        }

        void PositionCamera(ref Cinemachine.CameraState curState, float deltaTime)
        {
            var up = curState.ReferenceUp;
            var targetPos = this.FollowTargetPosition;
            var targetRot = this.FollowTargetRotation;
            var targetForward = targetRot * Vector3.forward;
            var heading = GetHeading(targetForward, up);

            if (deltaTime < 0)
            {
                // No damping - reset damping state info
                this.m_DampingCorrection = Vector3.zero;
                this.m_CamPosCollisionCorrection = 0;
            }
            else
            {
                // Damping correction is applied to the shoulder offset - stretching the rig
                this.m_DampingCorrection += Quaternion.Inverse(heading) * (this.m_PreviousFollowTargetPosition - targetPos);
                this.m_DampingCorrection -= this.VirtualCamera.DetachedFollowTargetDamp(this.m_DampingCorrection, this.Damping, deltaTime);
            }

            this.m_PreviousFollowTargetPosition = targetPos;
            var root = targetPos;
            this.GetRawRigPositions(root, targetRot, heading, out _, out Vector3 hand);

            // Place the camera at the correct distance from the hand
            var camPos = hand - (targetForward * (this.CameraDistance - this.m_DampingCorrection.z));

            // Check if hand is colliding with something, if yes, then move the hand
            // closer to the player. The radius is slightly enlarged, to avoid problems
            // next to walls
            float dummy = 0;
            var collidedHand = this.ResolveCollisions(root, hand, -1, this.CameraRadius * 1.05f, ref dummy);
            camPos = this.ResolveCollisions(collidedHand, camPos, deltaTime, this.CameraRadius, ref this.m_CamPosCollisionCorrection);

            // Set state
            curState.RawPosition = camPos;
            curState.RawOrientation = targetRot; // not necessary, but left in to avoid breaking scenes that depend on this
        }

        /// <summary>
        /// Internal use only.  Public for the inspector gizmo
        /// </summary>
        /// <param name="root">Root of the rig.</param>
        /// <param name="shoulder">Shoulder of the rig.</param>
        /// <param name="hand">Hand of the rig.</param>
        public void GetRigPositions(out Vector3 root, out Vector3 shoulder, out Vector3 hand)
        {
            var up = this.VirtualCamera.State.ReferenceUp;
            var targetRot = this.FollowTargetRotation;
            var targetForward = targetRot * Vector3.forward;
            var heading = GetHeading(targetForward, up);
            root = this.m_PreviousFollowTargetPosition;
            this.GetRawRigPositions(root, targetRot, heading, out shoulder, out hand);
            float dummy = 0;
            hand = this.ResolveCollisions(root, hand, -1, this.CameraRadius * 1.05f, ref dummy);

        }

        static Quaternion GetHeading(Vector3 targetForward, Vector3 up)
        {
            var planeForward = targetForward.ProjectOntoPlane(up);
            planeForward = Vector3.Cross(up, Vector3.Cross(planeForward, up));
            return Quaternion.LookRotation(planeForward, up);
        }

        void GetRawRigPositions(
            Vector3 root, Quaternion targetRot, Quaternion heading,
            out Vector3 shoulder, out Vector3 hand)
        {
            var shoulderOffset = this.ShoulderOffset;
            shoulderOffset.x = Mathf.Lerp(-shoulderOffset.x, shoulderOffset.x, this.CameraSide);
            shoulderOffset.x += this.m_DampingCorrection.x;
            shoulderOffset.y += this.m_DampingCorrection.y;
            shoulder = root + heading * shoulderOffset;
            hand = shoulder + targetRot * new Vector3(0, this.VerticalArmLength, 0);
        }

        Vector3 ResolveCollisions(
            Vector3 root, Vector3 tip, float deltaTime,
            float cameraRadius, ref float collisionCorrection)
        {
            if (this.PhysicsWorld == null)
            {
                return tip;
            }

            if (this.CameraCollisionFilter == 0)
            {
                return tip;
            }

            var dir = tip - root;
            var len = dir.magnitude;
            dir /= len;

            var result = tip;
            float desiredCorrection = 0;

            this.PhysicsWorld.GetOutputDependency().Complete();
            var collisionWorld = this.PhysicsWorld.PhysicsWorld.CollisionWorld;

            var collisionFilter = new CollisionFilter
            {
                BelongsTo = uint.MaxValue,
                CollidesWith = (uint)this.CameraCollisionFilter.value,
            };

            if (collisionWorld.SphereCast(root, cameraRadius, dir, len, out var hitInfo, collisionFilter))
            {
                Vector3 desiredResult = hitInfo.Position + (hitInfo.SurfaceNormal * cameraRadius);
                desiredCorrection = (desiredResult - tip).magnitude;
            }

            collisionCorrection += deltaTime < 0 ? desiredCorrection - collisionCorrection : Damper.Damp(
                desiredCorrection - collisionCorrection,
                desiredCorrection > collisionCorrection ? this.DampingIntoCollision : this.DampingFromCollision,
                deltaTime);

            // Apply the correction
            if (collisionCorrection > Epsilon)
                result -= dir * collisionCorrection;

            return result;
        }
    }
}
