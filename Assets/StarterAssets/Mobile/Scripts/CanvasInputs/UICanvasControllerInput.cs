using UnityEngine;

namespace StarterAssets
{
    public class UICanvasControllerInput : MonoBehaviour
    {
        [Header("Output")]
        public PlayerFreeMove starterAssetsInputs;
        public DoubleJump doubleJumpInput;

        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            starterAssetsInputs.MoveInput(virtualMoveDirection);
        }

        public void VirtualInteractInput(bool virtualInteractState)
        {
            starterAssetsInputs.InteractInput(virtualInteractState);
        }

        public void VirtualJumpInput(bool virtualJumpState)
        {
            doubleJumpInput.JumpInput(virtualJumpState);
        }
    }
}
