using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomImageManager : MonoBehaviour
{
    #region Inspector
    [Header("Target UI")]
    public Image targetImage;
    public CanvasGroup canvasGroup;

    [Header("Images")]
    public List<Sprite> images = new List<Sprite>();

    [Header("Timing")]
    public bool autoRun = true;
    public float firstDelay = 1f;
    public float interval = 10f;
    public float showDuration = 2f;
    public float fadeDuration = 0.3f;

    [Header("Selection Rules")]
    public bool noRepeatUntilAllShown = true;
    public bool avoidImmediateRepeat = true;
    public bool useWeights = false;
    public List<float> weights = new List<float>();
    #endregion

    #region Internal State
    private Coroutine loopCo;
    private List<int> bag = new List<int>();
    private int lastIndex = -1;
    private bool isFading = false;
    #endregion

    #region Unity Events
    void Awake()
    {
        if (targetImage == null) targetImage = GetComponent<Image>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        HideInstant();
    }

    void Start()
    {
        if (autoRun) StartAuto();
    }
    #endregion

    #region Auto Loop Control
    public void StartAuto()
    {
        if (loopCo != null) StopCoroutine(loopCo);
        loopCo = StartCoroutine(AutoLoop());
    }

    public void StopAuto()
    {
        if (loopCo != null) StopCoroutine(loopCo);
        loopCo = null;
    }

    private IEnumerator AutoLoop()
    {
        yield return new WaitForSeconds(firstDelay);
        while (true)
        {
            yield return ShowRandomOnceRoutine();
            yield return new WaitForSeconds(interval);
        }
    }
    #endregion

    #region Public API
    public void ShowRandomOnce()
    {
        StartCoroutine(ShowRandomOnceRoutine());
    }

    private IEnumerator ShowRandomOnceRoutine()
    {
        int index = GetNextIndex();
        if (index < 0) yield break;

        targetImage.sprite = images[index];
        yield return FadeTo(1f, fadeDuration);
        yield return new WaitForSeconds(showDuration);
        yield return FadeTo(0f, fadeDuration);
    }
    #endregion

    #region Index Selection
    private int GetNextIndex()
    {
        if (images == null || images.Count == 0) return -1;

        if (useWeights)
        {
            if (weights.Count != images.Count) FixWeightsLength();

            int idx = WeightedRandomIndex();
            if (avoidImmediateRepeat && images.Count > 1 && idx == lastIndex)
            {
                int retry = WeightedRandomIndex();
                if (retry != lastIndex) idx = retry;
            }
            lastIndex = idx;
            return idx;
        }

        if (noRepeatUntilAllShown)
        {
            if (bag.Count == 0)
                for (int i = 0; i < images.Count; i++) bag.Add(i);

            int rand = Random.Range(0, bag.Count);
            int idx = bag[rand];
            bag.RemoveAt(rand);

            if (avoidImmediateRepeat && images.Count > 1 && idx == lastIndex && bag.Count > 0)
            {
                int rand2 = Random.Range(0, bag.Count);
                int idx2 = bag[rand2];
                bag[rand2] = idx;
                idx = idx2;
            }

            lastIndex = idx;
            return idx;
        }
        else
        {
            int idx = Random.Range(0, images.Count);
            if (avoidImmediateRepeat && images.Count > 1 && idx == lastIndex)
                idx = Random.Range(0, images.Count);

            lastIndex = idx;
            return idx;
        }
    }

    private void FixWeightsLength()
    {
        if (weights.Count < images.Count)
        {
            int missing = images.Count - weights.Count;
            for (int i = 0; i < missing; i++) weights.Add(1f);
        }
        else if (weights.Count > images.Count)
        {
            weights.RemoveRange(images.Count, weights.Count - images.Count);
        }
    }

    private int WeightedRandomIndex()
    {
        float total = 0f;
        for (int i = 0; i < weights.Count; i++)
            total += Mathf.Max(0f, weights[i]);

        if (total <= 0f) return Random.Range(0, images.Count);

        float r = Random.value * total;
        float cum = 0f;
        for (int i = 0; i < weights.Count; i++)
        {
            cum += Mathf.Max(0f, weights[i]);
            if (r <= cum) return i;
        }
        return images.Count - 1;
    }
    #endregion

    #region Fade
    private IEnumerator FadeTo(float target, float duration)
    {
        if (canvasGroup == null) yield break;

        isFading = true;
        float start = canvasGroup.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = duration > 0f ? t / duration : 1f;
            canvasGroup.alpha = Mathf.Lerp(start, target, k);
            yield return null;
        }
        canvasGroup.alpha = target;
        isFading = false;
    }

    private void HideInstant()
    {
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }
    #endregion
}
