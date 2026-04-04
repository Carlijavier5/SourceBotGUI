using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class SearchPromptLogic : MonoBehaviour
{

    private static readonly string[] sortByOptions = { "default", "bm25", "pagerank", "fqn", "fqn-coderank", "bm25-fqn-coderank" };

    private static readonly string[] languages = { "C#", "C++", "Go", "Java", "Python", "TypeScript" };

    [SerializeField] private SourcebotSearch searchAPI;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button searchButton;
    [SerializeField] private TMP_Dropdown sortByDropdown;
    [SerializeField] private TMP_Dropdown languagesDropdown;
    [SerializeField] private StructuralAnimate structuralAnimate;
    
    private bool bypassEnter;

    void Awake() {
        structuralAnimate.OnSearchAllowed += StructuralAnimate_OnSearchAllowed;
    }

    private void StructuralAnimate_OnSearchAllowed() {
        ToggleInteractables(true);
    }

    public void ForceGatherAndSearch() {
        bypassEnter = true;
        GatherAndSearch(inputField.text);
    }

    public void GatherAndSearch(string searchQuery) {
        if (string.IsNullOrWhiteSpace(searchQuery)
                || (!Keyboard.current.enterKey.isPressed && !bypassEnter)) {
            return;
        }

        bypassEnter = false;

        ToggleInteractables(false);
        structuralAnimate.DoNextResult();

        string sortBy = sortByOptions[sortByDropdown.value];

        HashSet<string> selectedLanguages = new();
        for (int i = 0; i < languages.Length; i++) {
            if ((languagesDropdown.value & (1 << i)) != 0) {
                selectedLanguages.Add(languages[i]);
            }
        }

        bool filterByLanguage = selectedLanguages.Count > 0 && selectedLanguages.Count < 6;

        searchAPI.Search(searchQuery, 20, sortBy, (response) => {

            List<SearchFile> results = filterByLanguage
                                     ? response.files.FindAll(f => selectedLanguages.Contains(f.language))
                                     : response.files;

            structuralAnimate.UpdateResults(results);

        }, (err) => Debug.LogError(err));
    }

    private void ToggleInteractables(bool on) {
        inputField.interactable = on;
        searchButton.interactable = on;
    }
}
