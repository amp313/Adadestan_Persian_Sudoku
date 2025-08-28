using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIConfetti : MonoBehaviour
{
    #region Enums
    public enum EmissionMode { CenterBurst, TopBurst, TopRain }
    #endregion

    #region Inspector Fields
    [Header("References")]
    public RectTransform container;

    [Header("Play")]
    public bool autoClearBeforePlay = true;
    [Min(1)] public int count = 500;
    public EmissionMode emissionMode = EmissionMode.CenterBurst;

    [Header("Physics (UI units = pixel)")]
    public Vector2 gravity = new Vector2(0f, -900f);
    public Vector2 initialSpeedRange = new Vector2(450f, 900f);
    [Range(0f, 180f)] public float angleSpread = 100f;
    [Range(0f, 1f)] public float drag = 0.08f;
    [Min(0.5f)] public float maxLifetime = 5.5f;

    [Header("Piece Size (px)")]
    public Vector2 widthRange = new Vector2(10f, 28f);
    public Vector2 heightRange = new Vector2(4f, 14f);

    [Header("Wobble / Flip")]
    public float wobbleAmplitude = 35f;
    public Vector2 wobbleFrequencyRange = new Vector2(0.8f, 1.8f);
    public Vector2 angularSpeedRange = new Vector2(90f, 360f);

    [Header("Pooling")]
    public int poolCapacity = 600;

    [Header("Colors (Iranian Palette)")]
    public string[] iranianHexColors = new string[]
    {
        "#00A693", "#C81D11", "#1C39BB", "#0067A5", "#32127A", "#D99058",
        "#701C1C", "#F77FBE", "#FE28A2"
    };

    [Header("Raycast")]
    public bool disableRaycastTarget = true;
    #endregion

    #region Internal Classes
    class Piece
    {
        public RectTransform rt;
        public Image img;
        public Vector2 vel;
        public float angularVel;
        public float life;
        public float wobbleFreq;
        public float wobblePhase;
        public float startX;
        public bool active;
    }
    #endregion

    #region Private Fields
    readonly List<Piece> pool = new List<Piece>();
    Sprite _unitSprite;
    Color[] palette;
    #endregion

    #region Unity Methods
    void Awake()
    {
        if (container == null)
        {
            var canv = GetComponentInParent<Canvas>();
            if (canv != null) container = canv.GetComponent<RectTransform>();
        }

        if (_unitSprite == null)
        {
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply(false, true);
            _unitSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f), 100f);
        }

        palette = BuildPalette(iranianHexColors);
        EnsurePoolCapacity(poolCapacity);
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        if (dt <= 0f) return;

        var rect = container.rect;
        float left = rect.xMin, right = rect.xMax, bottom = rect.yMin;

        for (int i = 0; i < pool.Count; i++)
        {
            var p = pool[i];
            if (!p.active) continue;

            p.life += dt;
            if (p.life > maxLifetime) { Deactivate(p); continue; }

            p.vel += gravity * dt;
            p.vel *= Mathf.Clamp01(1f - drag * dt);

            float wobble = wobbleAmplitude * Mathf.Sin(
                (p.life + p.wobblePhase) * (Mathf.PI * 2f) * p.wobbleFreq);

            Vector2 pos = p.rt.anchoredPosition;
            pos += p.vel * dt;
            pos.x = p.startX + wobble;

            p.rt.anchoredPosition = pos;
            p.rt.localRotation = Quaternion.Euler(0, 0,
                p.rt.localEulerAngles.z + p.angularVel * dt);

            if (pos.y < bottom - 50f || pos.x < left - 50f || pos.x > right + 50f)
                Deactivate(p);
        }
    }
    #endregion

    #region Public API
    public void ClearAll()
    {
        for (int i = 0; i < pool.Count; i++)
            Deactivate(pool[i], true);
    }

    public void Play(int? overrideCount = null, EmissionMode? mode = null)
    {
        if (container == null) return;
        if (autoClearBeforePlay) ClearAll();

        int n = Mathf.Max(1, overrideCount ?? count);
        var useMode = mode ?? emissionMode;
        var rect = container.rect;
        float centerX = (rect.xMin + rect.xMax) * 0.5f;
        float centerY = (rect.yMin + rect.yMax) * 0.5f;

        for (int i = 0; i < n; i++)
        {
            var p = GetInactivePiece();
            Activate(p);

            float w = RandomRange(widthRange);
            float h = RandomRange(heightRange);
            p.rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
            p.rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
            p.img.color = palette[RandomIndex(palette.Length)];

            switch (useMode)
            {
                case EmissionMode.CenterBurst:
                    p.rt.anchoredPosition = new Vector2(centerX, centerY);
                    p.startX = p.rt.anchoredPosition.x;
                    p.vel = AngleToDir(90f + RandomSigned(angleSpread * 0.5f))
                          * RandomRange(initialSpeedRange);
                    break;

                case EmissionMode.TopBurst:
                    float tx = Mathf.Lerp(rect.xMin + 40f, rect.xMax - 40f, Random.value);
                    float ty = rect.yMax - 30f;
                    p.rt.anchoredPosition = new Vector2(tx, ty);
                    p.startX = tx;
                    p.vel = AngleToDir(260f + RandomSigned(angleSpread * 0.5f))
                          * RandomRange(initialSpeedRange);
                    break;

                case EmissionMode.TopRain:
                    float rx = Mathf.Lerp(rect.xMin + 20f, rect.xMax - 20f, Random.value);
                    float ry = rect.yMax + 20f;
                    p.rt.anchoredPosition = new Vector2(rx, ry);
                    p.startX = rx;
                    float speed = Mathf.Lerp(initialSpeedRange.x, initialSpeedRange.y, Random.value * 0.35f);
                    p.vel = new Vector2(RandomSigned(60f), -Mathf.Abs(speed));
                    break;
            }

            p.rt.localRotation = Quaternion.Euler(0, 0, RandomSigned(180f));
            p.angularVel = RandomSigned(Mathf.Lerp(angularSpeedRange.x, angularSpeedRange.y, Random.value));
            p.wobbleFreq = Mathf.Lerp(wobbleFrequencyRange.x, wobbleFrequencyRange.y, Random.value);
            p.wobblePhase = Random.value * 10f;
        }
    }
    #endregion

    #region Pool & Utils
    void EnsurePoolCapacity(int cap)
    {
        if (cap < 1) cap = 1;
        while (pool.Count < cap)
        {
            var go = new GameObject("Piece", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(container, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            var img = go.GetComponent<Image>();
            img.sprite = _unitSprite;
            img.raycastTarget = !disableRaycastTarget ? true : false;
            img.material = null;

            var piece = new Piece { rt = rt, img = img, active = false };
            go.SetActive(false);
            pool.Add(piece);
        }
    }

    Piece GetInactivePiece()
    {
        for (int i = 0; i < pool.Count; i++)
            if (!pool[i].active) return pool[i];

        EnsurePoolCapacity(pool.Count + 50);
        return GetInactivePiece();
    }

    void Activate(Piece p)
    {
        p.active = true;
        p.life = 0f;
        p.vel = Vector2.zero;
        p.angularVel = 0f;
        p.wobblePhase = 0f;
        p.wobbleFreq = 1f;
        p.rt.gameObject.SetActive(true);
    }

    void Deactivate(Piece p, bool immediate = false)
    {
        p.active = false;
        if (p.rt != null && p.rt.gameObject.activeSelf)
            p.rt.gameObject.SetActive(false);
    }

    static float RandomRange(Vector2 r) => Mathf.Lerp(r.x, r.y, Random.value);
    static int RandomIndex(int len) => Mathf.Clamp(Mathf.FloorToInt(Random.value * len), 0, len - 1);
    static float RandomSigned(float maxAbs) => (Random.value * 2f - 1f) * maxAbs;

    static Vector2 AngleToDir(float deg)
    {
        float rad = deg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    static Color[] BuildPalette(string[] hexes)
    {
        var list = new List<Color>(hexes.Length);
        for (int i = 0; i < hexes.Length; i++)
            if (ColorUtility.TryParseHtmlString(hexes[i], out var c)) list.Add(c);

        if (list.Count == 0) list.Add(Color.white);
        return list.ToArray();
    }
    #endregion
}
