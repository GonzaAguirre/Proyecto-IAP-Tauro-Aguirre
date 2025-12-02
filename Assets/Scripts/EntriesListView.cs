using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class EntriesListView : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Transform entriesContainer; 
    [SerializeField] private GameObject entryButtonPrefab;

    private List<string> unlockedTypes = new List<string>();
    
    public event Action<string> OnPlagueClicked;

    public void SetUnlockedTypes(List<string> types)
    {
        unlockedTypes = types ?? new List<string>();
    }

    public void PopulateList(List<PestData> plagues)
    {
        foreach (Transform child in entriesContainer) Destroy(child.gameObject);

        if (plagues == null) return;

        foreach (var plague in plagues)
        {
            if (plague == null) continue;

            GameObject btnObj = Instantiate(entryButtonPrefab, entriesContainer);
            btnObj.transform.localScale = Vector3.one;

            var tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = plague.name;

            var btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                bool isUnlocked = unlockedTypes.Contains(plague.type);
                
                if (isUnlocked)
                {
                    btn.interactable = true;
                    string id = plague.id;
                    btn.onClick.AddListener(() => OnPlagueClicked?.Invoke(id));
                }
                else
                {
                    btn.interactable = false;
                    if (tmp != null)
                    {
                        tmp.text += LocalizationManager.GetLockedText();
                        tmp.color = Color.gray;
                    }
                }
            }
        }
    }
}