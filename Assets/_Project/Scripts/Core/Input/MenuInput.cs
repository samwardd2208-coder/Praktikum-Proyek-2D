using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public static class MenuInput
{
    static Keyboard K => Keyboard.current;
    static Gamepad G => Gamepad.current;

    // Helper untuk mengecek apakah salah satu tombol baru saja ditekan
    static bool PressedThisFrame(params KeyControl[] keys)
    {
        foreach (var key in keys)
        {
            if (key != null && key.wasPressedThisFrame) return true;
        }
        return false;
    }

    public static bool Up => PressedThisFrame(K?.upArrowKey, K?.wKey) 
        || (G != null && G.dpad.up.wasPressedThisFrame);
    
    public static bool Down => PressedThisFrame(K?.downArrowKey, K?.sKey) 
        || (G != null && G.dpad.down.wasPressedThisFrame);
    
    public static bool Left => PressedThisFrame(K?.leftArrowKey, K?.aKey) 
        || (G != null && G.dpad.left.wasPressedThisFrame);
    
    public static bool Right => PressedThisFrame(K?.rightArrowKey, K?.dKey) 
        || (G != null && G.dpad.right.wasPressedThisFrame);
    
    public static bool Confirm => PressedThisFrame(K?.eKey, K?.spaceKey, K?.enterKey, K?.numpadEnterKey, K?.zKey) 
        || (G != null && G.buttonSouth.wasPressedThisFrame);
    
    public static bool Cancel => PressedThisFrame(K?.escapeKey, K?.xKey, K?.backspaceKey) 
        || (G != null && G.buttonEast.wasPressedThisFrame);
    
    public static bool Mute => PressedThisFrame(K?.mKey);
}