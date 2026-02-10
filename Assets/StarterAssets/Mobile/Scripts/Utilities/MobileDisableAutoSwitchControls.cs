

using UnityEngine;
using UnityEngine.InputSystem;

public class MobileDisableAutoSwitchControls : MonoBehaviour
{
    
#if (UNITY_IOS || UNITY_ANDROID)

   // Mobile -- Do nothing
#else 
    private void Start()
    {
        Destroy(gameObject);
    }

#endif
}
