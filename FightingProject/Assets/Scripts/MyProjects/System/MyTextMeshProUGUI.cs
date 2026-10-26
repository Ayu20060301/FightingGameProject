using TMPro;
using UnityEngine;

public class MyTextMeshProUGUI : TextMeshProUGUI
{
    [SerializeField] private Color _outlineColor = Color.black;
    [SerializeField, Range(0f, 1f)] private float _outlineWidth = 0.1f;

    private Material _instance;

    protected override void Awake()
    {
        base.Awake();
        EnsureInstance();
        Apply();
    }

    private void EnsureInstance()
    {
        if (_instance != null) return;
        var src = fontSharedMaterial;
        _instance = new Material(src) { name = src.name + " (Instance)" };
        fontSharedMaterial = _instance;
    }

    private void Apply()
    {
        if (_instance == null) return;
        _instance.SetColor(ShaderUtilities.ID_OutlineColor, _outlineColor);
        _instance.SetFloat(ShaderUtilities.ID_OutlineWidth, _outlineWidth);
    }

    public void SetOutline(Color color, float width)
    {
        _outlineColor = color;
        _outlineWidth = width;
        Apply();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (_instance != null) Destroy(_instance);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        Apply();
    }
#endif
}