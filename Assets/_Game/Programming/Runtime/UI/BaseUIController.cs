using UnityEngine;
using UnityEngine.UIElements;

public class BaseUIController : MonoBehaviour
{
    [SerializeField] protected UIDocument m_document;
    [SerializeField] protected VisualElement m_root;

    protected virtual void Awake()
    {
        if(m_document == null)
        {
            m_document = GetComponent<UIDocument>();
        }

        Init();
    }

    /// <summary>
    /// Initialise the elements of the UI document
    /// </summary>
    protected virtual void Init()
    {
        m_root = m_document.rootVisualElement;
    }
}
