using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class DialogueBoxController : BaseUIController
{
    private const string DEFAULT_PROMPT = "Press [E] to continue";

    private VisualElement m_dialogueContainer;
    private Label m_dialogueText;
    private Label m_dialoguePrompt;

    private Coroutine m_dialogueCoroutine;

    [SerializeField] private float m_charactersPerSecond = 60f;

    [SerializeField] private InputActionAsset m_playerInputMap;
    private InputAction m_confirmAction;

    private bool m_dialogueBoxActive;
    private string m_currentText;

    private DialogueCollectionData m_currentDialogueCollection;
    private int m_currentDialogueIndex = 0;

    [SerializeField] DialogueCollectionData TestDialogueCollection;

    private void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            OpenDialogueBox(TestDialogueCollection);
        }
    }

    protected override void Init()
    {
        base.Init();

        m_dialogueContainer = m_root.Q("ve_DialogueContainer");
        m_dialogueText = m_root.Q<Label>("lbl_DialogueText");
        m_dialoguePrompt = m_root.Q<Label>("lbl_DialoguePrompt");
    }

    protected override void Awake()
    {
        base.Awake();

        m_confirmAction = m_playerInputMap.FindAction("Confirm");

        m_confirmAction.performed += UpdateDialogueBox;

        ResetDialogueBox();   
    }

    private void Start()
    {
        //OpenDialogueBox(TestDialogueCollection);
    }

    public void ResetDialogueBox()
    {
        m_dialogueContainer.AddToClassList("dialogueDown");

        m_dialogueBoxActive = false;

        m_dialogueContainer.style.visibility = Visibility.Hidden;

        m_dialogueText.style.visibility = Visibility.Hidden;
        m_dialogueText.text = string.Empty;

        HidePrompt();
    }

    public void OpenDialogueBox(string text)
    {
        PlayerInputLock.Lock(PlayerInputLockReasons.Dialogue);

        m_dialogueBoxActive = true;

        m_dialogueContainer.RemoveFromClassList("dialogueDown");
        m_dialogueContainer.style.visibility = Visibility.Visible;

        m_dialogueText.style.visibility = Visibility.Visible;
        m_dialogueText.text = string.Empty;

        HidePrompt();

        ShowText(text);
    }

    public void OpenDialogueBox(DialogueCollectionData dialogueCollectionData)
    {
        PlayerInputLock.Lock(PlayerInputLockReasons.Dialogue);

        m_dialogueBoxActive = true;

        m_dialogueContainer.RemoveFromClassList("dialogueDown");
        m_dialogueContainer.style.visibility = Visibility.Visible;

        m_dialogueText.style.visibility = Visibility.Visible;
        m_dialogueText.text = string.Empty;

        HidePrompt();

        LoadDialogueCollection(dialogueCollectionData);
    }

    public void CloseDialogueBox()
    {
        m_dialogueBoxActive = false;

        m_dialogueContainer.AddToClassList("dialogueDown");

        m_dialogueText.style.visibility = Visibility.Hidden;
        m_dialogueText.text = string.Empty;

        HidePrompt();

        PlayerInputLock.Unlock(PlayerInputLockReasons.Dialogue);
    }

    public void ShowText(string text)
    {
        m_currentDialogueCollection = null;

        if (m_dialogueCoroutine != null)
        {
            StopCoroutine(m_dialogueCoroutine);
        }

        m_currentText = text;

        m_dialogueCoroutine = StartCoroutine(TypeText(m_currentText));
    }

    public void LoadDialogueCollection(DialogueCollectionData dialogueCollection)
    {
        m_currentDialogueCollection = dialogueCollection;

        m_currentDialogueIndex = 0;

        LoadDialogue();
    }

    public void LoadDialogue()
    {
        if (m_dialogueCoroutine != null)
        {
            StopCoroutine(m_dialogueCoroutine);
        }

        m_currentText = m_currentDialogueCollection.DialogueCollection[m_currentDialogueIndex].Dialogue;

        m_dialogueCoroutine = StartCoroutine(TypeText(m_currentText));
    }

    private IEnumerator TypeText(string text)
    {
        HidePrompt();

        m_dialogueText.text = "";

        float delay = 1f / m_charactersPerSecond;

        foreach (char c in text)
        {
            m_dialogueText.text += c;
            yield return new WaitForSeconds(delay);
        }

        m_dialogueCoroutine = null;

        ShowPrompt();
    }

    private void ShowPrompt()
    {
        m_dialoguePrompt.style.visibility = Visibility.Visible;
        m_dialoguePrompt.text = DEFAULT_PROMPT;
    }

    private void HidePrompt()
    {
        m_dialoguePrompt.style.visibility = Visibility.Hidden;
        m_dialoguePrompt.text = DEFAULT_PROMPT;
    }
    
    private void UpdateDialogueBox(InputAction.CallbackContext context)
    {
        if (m_dialogueBoxActive)
        {
            // Skip typing dialogue if there is still one in progress
            if (m_dialogueCoroutine != null)
            {
                StopCoroutine(m_dialogueCoroutine);

                m_dialogueText.text = m_currentText;

                m_dialogueCoroutine = null;

                ShowPrompt();
            }
            else
            {
                // Check if dialogue is standalone or part of a collection
                if(m_currentDialogueCollection != null)
                {
                    if(m_currentDialogueIndex < m_currentDialogueCollection.DialogueCollection.Length - 1)
                    {
                        m_currentDialogueIndex++;
                        LoadDialogue();
                    }
                    else
                    {
                        CloseDialogueBox();
                    }
                }
                else
                {
                    CloseDialogueBox();
                }               
            }
        }
    }
}
