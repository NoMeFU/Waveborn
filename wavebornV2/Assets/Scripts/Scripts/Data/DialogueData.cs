using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Data", fileName = "NewDialogue")]
public class DialogueData : ScriptableObject
{
    [TextArea(2, 5)]
    [SerializeField] private string[] lines;

    public int Count => lines?.Length ?? 0;
    public string GetLine(int i) =>
        (lines != null && i >= 0 && i < lines.Length) ? lines[i] : "";
}
