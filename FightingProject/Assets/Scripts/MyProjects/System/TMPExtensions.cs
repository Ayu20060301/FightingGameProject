using TMPro;

public static class TMPExtensions
{
    public static TMPMaterialOverride GetOrAddMaterialOverride(this TMP_Text tmp)
    {
        if (!tmp.TryGetComponent(out TMPMaterialOverride o))
            o = tmp.gameObject.AddComponent<TMPMaterialOverride>();
        return o;
    }
}