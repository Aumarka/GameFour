using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Scriptable Objects/DialogueData")]
public class DialogueData : ScriptableObject
{
    [TextArea]
    [SerializeField] private string m_dialogue;

    public string Dialogue => m_dialogue;
}
