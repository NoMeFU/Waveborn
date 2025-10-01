// Assets/Scripts/NPC/DialogueData.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [TextArea(2, 4)] public string[] lines;

    public int Count => lines != null ? lines.Length : 0;
    public string GetLine(int i) => (lines != null && i >= 0 && i < lines.Length) ? lines[i] : "";
}
