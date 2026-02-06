using UnityEngine;

namespace StarterAssets
{
    public class UICanvasControllerInput2 : MonoBehaviour
    {
        [Header("Output")]
        public DoubleJump doubleJumpInput;
        public PlayerSlide playerSlideInput;

        public void VirtualJumpInput(bool virtualJumpState)
        {
            doubleJumpInput.JumpInput(virtualJumpState);
        }

        public void VirtualSlideInput(bool virtualSlideState)
        {
            playerSlideInput.SlideInput(virtualSlideState);
        }
    }
}
