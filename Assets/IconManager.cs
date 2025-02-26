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
        iconCamera = Camera.main;
        if (iconCamera == null)
        {
            Debug.LogError("No camera found");
        }
        ConfigureRenderSystem();
    }

    void CreateIconCanvas()
    {
        GameObject canvasGO = new GameObject("IconCanvas");
        iconCanvas = canvasGO.AddComponent<Canvas>();
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        iconCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        iconCanvas.sortingOrder = 1000;

        canvasGO.layer = LayerMask.NameToLayer("UI");
        DontDestroyOnLoad(canvasGO);
    }

    void ConfigureRenderSystem()
    {
        if (iconCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            iconCanvas.worldCamera = iconCamera;
        }
    }

    private void Update()
    {
        if (iconCanvas == null || iconCamera == null) return;

        foreach (var kvp in activeIcons.ToList())
        {
            var (icon, worldPos) = kvp.Value;
            if (icon == null)
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
        Vector3 screenPos = iconCamera.WorldToScreenPoint(worldPosition);
        bool behindCamera = screenPos.z < 0;

        if (behindCamera)
        {
            screenPos.x = -screenPos.x;
            screenPos.y = -screenPos.y;
        }

        bool onScreen = !behindCamera &&
                        screenPos.x >= 0 && screenPos.x <= Screen.width &&
                        screenPos.y >= 0 && screenPos.y <= Screen.height;

        if (!onScreen)
        {
            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2);
            Vector3 dir = (screenPos - screenCenter).normalized;

            float edgeX = (dir.x < 0) ? iconPadding : Screen.width - iconPadding;

            float edgeY = Mathf.Clamp(screenPos.y, iconPadding, Screen.height - iconPadding);

            screenPos = new Vector3(edgeX, edgeY, 0);
        }

        rt.position = screenPos;
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
}
