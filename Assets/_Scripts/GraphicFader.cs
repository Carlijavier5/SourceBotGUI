using System.Collections;
using UnityEngine;

public class GraphicFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup cg;

    public void DoFade(float target) {
        if (gameObject.activeSelf) {
            StopAllCoroutines();
            StartCoroutine(IDoFade(target));
        }
    }

    private IEnumerator IDoFade(float target) {
        while (Mathf.Abs(cg.alpha - target) > 0) {
            cg.alpha = Mathf.MoveTowards(cg.alpha, target, Time.deltaTime);
            yield return null;
        }
    }
}