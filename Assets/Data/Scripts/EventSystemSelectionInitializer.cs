using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

class EventSystemSelectionInitializer : MonoBehaviour
{
    public PanelSettings panelSettings; // drag your PanelSettings here in the inspector
    public EventSystem eventSystem;

    void Start()
    {
        if (eventSystem != null)
            eventSystem.SetSelectedGameObject(eventSystem.transform.Find(panelSettings.name).gameObject);
    }
}