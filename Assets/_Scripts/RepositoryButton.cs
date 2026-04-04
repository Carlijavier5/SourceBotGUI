using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RepositoryButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Button button;
    [SerializeField] private Color selectedColor;

    public void Setup(string repositoryName, StructuralAnimate structuralAnimate, bool isSelected) {
        text.text = repositoryName;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => { structuralAnimate.FilterResults(repositoryName); });
        ColorBlock buttonColors = button.colors;
        buttonColors.normalColor = isSelected ? selectedColor : buttonColors.normalColor;
        buttonColors.colorMultiplier = isSelected ? 2f : 1f;
        button.colors = buttonColors;
    }
}
