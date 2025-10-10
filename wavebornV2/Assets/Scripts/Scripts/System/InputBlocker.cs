// Assets/Scripts/System/InputBlocker.cs
using UnityEngine;

public static class InputBlocker
{
    private static int _blocks;
    public static bool Blocked => _blocks > 0;
    public static int Count => _blocks;

    public static void Push() { _blocks++; /* Debug.Log($"InputBlocker++ => {_blocks}"); */ }
    public static void Pop() { if (_blocks > 0) _blocks--; /* Debug.Log($"InputBlocker-- => {_blocks}"); */ }
    public static void Clear() { _blocks = 0; }
}
