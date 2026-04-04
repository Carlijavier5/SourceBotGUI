using System.Collections;
using UnityEngine;

public class SearchSectionDisplacer : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private AnimationCurve animationCurve;

    private Vector3 startPosition, startScale;
    private bool isInitialized;

    void Awake() {
        startPosition = transform.position;
        startScale = transform.localScale;
    }

    public void DoMove() {
        if (!isInitialized) {
            isInitialized = true;
            StartCoroutine(IDoMove());
        }
    }

    private IEnumerator IDoMove() {
        float lerpVal = 0;
        while (lerpVal < 1) {
            lerpVal = Mathf.MoveTowards(lerpVal, 1, Time.deltaTime);
            transform.position = Vector3.Lerp(startPosition, targetTransform.position, animationCurve.Evaluate(lerpVal));
            transform.localScale = Vector3.Lerp(startScale, targetTransform.localScale, animationCurve.Evaluate(lerpVal));
            yield return null;
        }
    }
}
