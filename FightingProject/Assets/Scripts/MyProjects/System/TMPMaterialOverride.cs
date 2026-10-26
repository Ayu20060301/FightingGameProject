using UnityEngine;
using TMPro;
using UnityEditor.ShaderGraph.Internal;


[RequireComponent(typeof(TMP_Text))]
public class TMPMaterialOverride : MonoBehaviour
{
    [Tooltip("未指定なら現在のマテリアルを複製して使う")]
    [SerializeField]
    private Material m_BaseMaterial;

    private TMP_Text m_Tmp;
    private Material m_Instance;

    public Material Material => m_Instance; 

    private void Awake()
    {
        m_Tmp = GetComponent<TMP_Text>();

        var src = m_BaseMaterial != null ? m_BaseMaterial : m_Tmp.fontSharedMaterial;
        m_Instance = new Material(src) { name = src.name + "{Instance}" };
        m_Tmp.fontSharedMaterial = m_Instance;
    }


    public void SetFaceColor(Color color)
        => m_Instance.SetColor(ShaderUtilities.ID_FaceColor, color);

    public void SetOutlineColor(Color color)
        => m_Instance.SetColor(ShaderUtilities.ID_OutlineColor, color);

    public void SetOutlineWidth(float width)
        => m_Instance.SetFloat(ShaderUtilities.ID_OutlineWidth, width);

    public void OnDestroy()
    {
        //複製したマテリアルは自分で破棄しないとリークする
        if(m_Instance != null)
        {
            Destroy(m_Instance);
        }
    }
}
