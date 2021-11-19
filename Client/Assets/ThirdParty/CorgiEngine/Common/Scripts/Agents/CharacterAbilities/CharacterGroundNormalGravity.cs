using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
    /// <summary>
    /// Add this ability to a Character with a CharacterGravity ability, and it will automatically compute the current slope's angle and change the gravity's direction to match the slope normal
    /// 
	/// Animator parameters : none
    /// </summary>
    [RequireComponent(typeof(CharacterGravity))]
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Ground Normal Gravity")]
    public class CharacterGroundNormalGravity : CharacterAbility
    {
        public override string HelpBoxText() { return "This component will automatically compute the current slope's angle and change the gravity ability's direction to match the slope normal."; }

        /// the length of the raycast used to detect slope angle 
        [Tooltip("the length of the raycast used to detect slope angle")]
        public float DownwardsRaycastLength = 5f;
        /// if this is true, slope angle will only be detected if grounded 
        [Tooltip("if this is true, slope angle will only be detected if grounded")]
        public bool OnlyWhenGrounded = false;
        
        protected RaycastHit2D _raycastHit2D;
    
        /// <summary>
        /// On ProcessAbility, we cast a ray downwards, compute its angle, and apply it to the gravity ability
        /// </summary>
        public override void ProcessAbility()
        {
            base.ProcessAbility();

            if (!AbilityAuthorized)
            {
                return;
            }

            if (OnlyWhenGrounded && !_controller.State.IsGrounded)
            {
                return;
            }

            _raycastHit2D = MMDebug.RayCast (_controller.BoundsCenter,-_controller.transform.up, DownwardsRaycastLength, _controller.PlatformMask, Color.blue, _controller.Parameters.DrawRaycastsGizmos);

            if (_raycastHit2D)
            {
                float normalAngle = MMMaths.AngleBetween(_raycastHit2D.normal, Vector2.up);
                _characterGravity.SetGravityAngle(normalAngle);    
            }
        }
    }
}
