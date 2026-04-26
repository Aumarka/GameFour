using UnityEngine;

[CreateAssetMenu(fileName = "DialogueCollectionData", menuName = "Scriptable Objects/DialogueCollectionData")]
public class DialogueCollectionData : ScriptableObject
{
    [SerializeField] private DialogueData[] m_dialogueCollection;

    public DialogueData[] DialogueCollection => m_dialogueCollection;
}
