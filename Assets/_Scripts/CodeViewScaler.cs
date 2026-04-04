using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CodeViewScaler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI codeText;
    [SerializeField] private LayoutElement layoutElement;
    [SerializeField] private float maxTextHeight;
    [SerializeField] private float padding = 25;

    public void UpdateHeight() {
        Canvas.ForceUpdateCanvases();

        float textHeight = Mathf.Min(codeText.GetPixelAdjustedRect().height, maxTextHeight) + padding;
        layoutElement.minHeight = textHeight;
        layoutElement.preferredHeight = textHeight;
    }
}
