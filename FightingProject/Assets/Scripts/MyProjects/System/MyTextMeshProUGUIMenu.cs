using UnityEngine;
using TMPro;
using UnityEditor;
using UnityEngine.UI;

public static class MyTextMeshProUGUIMenu
{
    [MenuItem("GameObject/UI (Canvas)/My TextMeshPro", false, 2001)]
    private static void Create(MenuCommand menuCommand)
    {
        // 右クリックした対象を親にする(なければ Canvas を探す/作る)
        var parent = menuCommand.context as GameObject;
        var canvas = parent != null ? parent.GetComponentInParent<Canvas>() : null;

        if (canvas == null)
        {
            canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) canvas = CreateCanvas();
            parent = canvas.gameObject;
        }

        var go = new GameObject("My Text (TMP)", typeof(RectTransform));
        GameObjectUtility.SetParentAndAlign(go, parent);

        var text = go.AddComponent<MyTextMeshProUGUI>();
        text.text = "New Text";
        text.fontSize = 36;
        text.alignment = TextAlignmentOptions.Center;
        text.rectTransform.sizeDelta = new Vector2(200f, 50f);

        Undo.RegisterCreatedObjectUndo(go, "Create My TextMeshPro");
        Selection.activeGameObject = go;
    }

    private static Canvas CreateCanvas()
    {
        var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        Undo.RegisterCreatedObjectUndo(go, "Create Canvas");

        // EventSystem がなければ作る
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
        }
        return canvas;
    }

}
