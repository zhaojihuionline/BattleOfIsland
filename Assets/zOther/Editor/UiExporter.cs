using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TMPro;

public class UiElementData
{
    public string name;
    public string type;
    public float[] pos;
    public float[] size;
    public string sprite;
    public string content;
}

public class UiExporter
{
    [MenuItem("Tools/Export UI JSON")]
    public static void ExportUI()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("Canvas not found! 请确保场景中有 Canvas。");
            return;
        }

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRect.sizeDelta;

        List<UiElementData> exportList = new List<UiElementData>();

        // 获取 Canvas 下所有 UI
        var rects = canvas.GetComponentsInChildren<RectTransform>(true);

        foreach (var rect in rects)
        {
            if (rect == canvasRect)
                continue; // 忽略根 Canvas

            UiElementData data = new UiElementData();
            data.name = rect.gameObject.name;
            data.type = GetUnityUIType(rect.gameObject);
            data.size = new float[] { rect.rect.width, rect.rect.height };

            // 计算相对于 Canvas 左上角的坐标
            Vector2 worldPos = rect.TransformPoint(rect.rect.center);
            Vector2 localOnCanvas = canvasRect.InverseTransformPoint(worldPos);

            // Unity Canvas 以中心为 (0,0)，要转换为左上角 (0,0)
            float x = localOnCanvas.x + canvasSize.x / 2f - rect.rect.width / 2f;
            float y = canvasSize.y / 2f - localOnCanvas.y - rect.rect.height / 2f;

            data.pos = new float[] { x, y };

            // 如果是 Image，记录 Sprite 路径
            Image img = rect.GetComponent<Image>();
            if (img != null && img.sprite != null)
            {
                data.sprite = AssetDatabase.GetAssetPath(img.sprite);
            }
            else
            {
                data.sprite = null;
            }
            exportList.Add(data);
        }

        string json = JsonConvert.SerializeObject(exportList, Formatting.Indented);

        string path = EditorUtility.SaveFilePanel("Save UI JSON", Application.dataPath, "ui_layout", "json");
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, json);
            Debug.Log("导出成功: " + path);
        }
    }

    static string GetUnityUIType(GameObject go)
    {
        if (go.GetComponent<Button>() != null) return "button";
        if (go.GetComponent<Image>() != null) return "image";
        if (go.GetComponent<Text>() != null) return "text";
        return "unknown";
    }
}
