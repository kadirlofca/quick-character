// Author: Kadir Lofca
// github.com/kadirlofca

using UnityEngine;

namespace QUICK.EXAMPLE
{
    /// <summary>
    /// AdvancedCharacter is an example class that derives from QuickCharacter and contains logic for various types of movement
    /// that QuickCharacter has to offer.
    /// </summary>
    public class AdvancedCharacter : QuickCharacter
    {
        [Header("Ground Movement")]
        public Gait gait;

        [Header("Jumping")]
        public float jumpForce = 5f;
        public float cayoteTime = 0.15f;

        [Header("Air Movement")]
        public float drag = 0.06f;
        public float airControl = 1.4f;
        public float airAcceleration = 8f;
        public float airAccelBoostThreshold = 4f;
        public float ascendingGravity = 12f;
        public float descendingGravity = 16f;

        [Header("Wall Movement")]
        public Gait wallClimbGait;
        public float wallCheckLength = 0.25f;
        public float climbOrRunDotThreshold = 0.5f;
        private Surface wallSurface;

        [Header("References")]
        public Transform controlTransform;
        public Animator fpsceneAnimator;

        public void OnJump()
        {
            bool jumpSuccessful = Jump(jumpForce, 2, true, cayoteTime);

            // Animate jumps.
            if (jumpSuccessful)
            {
                if (numberOfJumps == 1)
                {
                    fpsceneAnimator.SetTrigger("Jump");
                }
                else
                {
                    fpsceneAnimator.ResetTrigger("Jump");
                    fpsceneAnimator.SetTrigger("DoubleJump");
                }
            }
        }

        private MoveMedium FindNextMedium()
        {
            FindWall(worldInput, wallCheckLength, cap.height - (cap.radius * 2), 0, out wallSurface);

            if (wallSurface.isValid)
            {
                bool climb = Vector3.Dot(controlTransform.forward.XZ().normalized, -wallSurface.normal.XZ()) > climbOrRunDotThreshold;

                if (!climb)
                {
                    wallSurface = Surface.invalidSurface;
                }
            }

            if (floor.isValid)
            {
                return MoveMedium.ground;
            }
            else if (wallSurface.isValid)
            {
                return MoveMedium.wall;
            }
            else
            {
                return MoveMedium.air;
            }
        }

        protected override MoveMedium PhysicsUpdate()
        {
            switch (medium)
            {
                case MoveMedium.ground:
                    ApplyFloorMovement(gait);
                    break;

                case MoveMedium.air:
                    ApplyGravity(descendingGravity, ascendingGravity);
                    ApplyAirControlMovement(airControl);
                    ApplyAirMovement(rb.velocity.XZ().magnitude.Map(airAccelBoostThreshold, 0, 0, airAcceleration));
                    ApplyDrag(drag);
                    break;

                case MoveMedium.wall:
                    ApplySurfacePull(1, wallSurface);
                    ApplyWallClimbMovement(wallClimbGait, wallSurface);
                    break;

                default:
                    return MoveMedium.air;
            }

            return FindNextMedium();
        }

        protected override void OnMediumChange(MoveMedium oldMedium)
        {
            if (DidMediumChangeTo(oldMedium, MoveMedium.ground))
            {
                fpsceneAnimator.ResetTrigger("Jump");
                fpsceneAnimator.ResetTrigger("DoubleJump");
                fpsceneAnimator.SetTrigger("Land");
            }
            else
            {
                fpsceneAnimator.ResetTrigger("Land");
            }
        }

        private void Update()
        {
            fpsceneAnimator.SetBool("Running", HasMovementInput() && isOnFloor);
            fpsceneAnimator.SetFloat("RunSpeed", velocity.magnitude.Map(0, gait.speed, 0, 1));
        }
    }
}