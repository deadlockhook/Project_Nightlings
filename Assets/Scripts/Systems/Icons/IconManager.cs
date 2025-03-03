using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class IconManager : MonoBehaviour
{
    public static IconManager Instance { get; private set; }

    [Header("Icon Settings")]
    public GameObject iconPrefab;
    public float iconPadding = 50f;

    private Canvas iconCanvas;
    private Camera iconCamera;
    private RectTransform canvasRect;
    private Dictionary<int, (GameObject icon, Vector3 worldPos)> activeIcons =
        new Dictionary<int, (GameObject, Vector3)>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeIconSystem();
    }

    void InitializeIconSystem()
    {
        CreateIconCanvas();
    }

    void CreateIconCanvas()
    {
        GameObject canvasGO = new GameObject("IconCanvas");
        iconCanvas = canvasGO.AddComponent<Canvas>();
        canvasRect = canvasGO.GetComponent<RectTransform>();
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        iconCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        DontDestroyOnLoad(canvasGO);
    }

    private void Update()
    {
        if (!iconCanvas) return;

        foreach (var kvp in activeIcons.ToList())
        {
            var (icon, worldPos) = kvp.Value;
            if (!icon)
            {
                activeIcons.Remove(kvp.Key);
                continue;
            }
            UpdateIconPosition(icon, worldPos);
        }
    }

    void UpdateIconPosition(GameObject icon, Vector3 worldPosition)
    {
        RectTransform rt = icon.GetComponent<RectTransform>();
        Camera mainCam = Camera.main;

        if (!mainCam) return;

        Vector3 viewportPos = mainCam.WorldToViewportPoint(worldPosition);
        bool behindCamera = Vector3.Dot(mainCam.transform.forward, worldPosition - mainCam.transform.position) < 0;

        icon.SetActive(true);

        Vector3 screenPos;

        if (!behindCamera)
        {
            bool onScreen = viewportPos.x >= 0 && viewportPos.x <= 1 &&
                            viewportPos.y >= 0 && viewportPos.y <= 1;

            if (onScreen)
            {
                screenPos = mainCam.WorldToScreenPoint(worldPosition);
            }
            else
            {
                screenPos = GetScreenEdgePosition(viewportPos, mainCam, worldPosition, behindCamera);
            }
        }
        else
        {
            screenPos = GetScreenEdgePosition(viewportPos, mainCam, worldPosition, behindCamera);
        }

        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            null,
            out anchoredPos
        );

        rt.anchoredPosition = anchoredPos;
    }

    Vector3 GetScreenEdgePosition(Vector3 viewportPos, Camera mainCam, Vector3 worldPosition, bool behindCamera)
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Vector3 screenDir = (mainCam.WorldToScreenPoint(worldPosition) - screenCenter).normalized;

        if (behindCamera)
        {
            screenDir *= -1;
        }

        float screenWidth = Screen.width - iconPadding * 2;
        float screenHeight = Screen.height - iconPadding * 2;

        float angle = Mathf.Atan2(screenDir.y, screenDir.x);
        float slope = Mathf.Tan(angle);

        Vector3 screenPos;

        if (Mathf.Abs(slope) > (screenHeight / screenWidth))
        {
            float y = Mathf.Sign(screenDir.y) * (screenHeight / 2);
            float x = y / slope;
            screenPos = new Vector3(x + screenCenter.x, y + screenCenter.y, 0);
        }
        else
        {
            float x = Mathf.Sign(screenDir.x) * (screenWidth / 2);
            float y = x * slope;
            screenPos = new Vector3(x + screenCenter.x, y + screenCenter.y, 0);
        }

        screenPos.x = Mathf.Clamp(screenPos.x, iconPadding, Screen.width - iconPadding);
        screenPos.y = Mathf.Clamp(screenPos.y, iconPadding, Screen.height - iconPadding);

        return screenPos;
    }

    public void RegisterIcon(int id, Vector3 worldPosition)
    {
        if (activeIcons.ContainsKey(id)) return;

        GameObject newIcon = Instantiate(iconPrefab, iconCanvas.transform, false);
        activeIcons[id] = (newIcon, worldPosition);
        newIcon.SetActive(true);
    }

    public void UnregisterIcon(int id)
    {
        if (!activeIcons.TryGetValue(id, out var iconData)) return;

        Destroy(iconData.icon);
        activeIcons.Remove(id);
    }

    public void UpdateIconPosition(int id, Vector3 newWorldPosition)
    {
        if (activeIcons.TryGetValue(id, out var iconData))
        {
            activeIcons[id] = (iconData.icon, newWorldPosition);
        }
    }

    public void ToggleIcons(bool visible)
    {
        foreach (var iconData in activeIcons.Values)
        {
            if (iconData.icon)
                iconData.icon.SetActive(visible);
        }
    }

    public void ClearAllIcons()
    {
        foreach (var kvp in activeIcons.ToList())
        {
            if (kvp.Value.icon != null)
            {
                Destroy(kvp.Value.icon);
            }
        }
        activeIcons.Clear();
    }
}