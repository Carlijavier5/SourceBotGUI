using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChunkCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI filePath, language, lineNo, codeText;
    [SerializeField] private Button urlButton;
    [SerializeField] private CodeViewScaler codeViewScaler;

    public void Setup(string filePath, string language, int lineNo, string codeText, string url) {
        this.filePath.text = filePath;
        this.language.text = language;
        this.lineNo.text = $"Line {lineNo}";
        this.codeText.text = codeText;

        urlButton.onClick.RemoveAllListeners();
        urlButton.onClick.AddListener(() => Application.OpenURL(url));
        codeViewScaler.UpdateHeight();
    }
}
