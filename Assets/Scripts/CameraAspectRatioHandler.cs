using UnityEngine;

public class CameraAspectRatioHandler : MonoBehaviour
{
    [SerializeField] private float targetAspect = 16f / 9f; // Your design aspect ratio
    [SerializeField] private bool letterbox = true;

    private Camera mainCamera;
    private float initialOrthographicSize;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        initialOrthographicSize = mainCamera.orthographicSize;
        AdjustCamera();
    }

    void Update()
    {
        // Optional: readjust if screen size changes (for testing)
        if (Application.isEditor)
        {
            AdjustCamera();
        }
    }

    void AdjustCamera()
    {
        float currentAspect = (float)Screen.width / Screen.height;
        float scaleHeight = currentAspect / targetAspect;

        if (letterbox)
        {
            // Letterbox approach (black bars on sides)
            if (scaleHeight < 1.0f)
            {
                // Tall screen (mobile) - add bars on sides
                Rect rect = mainCamera.rect;
                rect.width = 1.0f;
                rect.height = scaleHeight;
                rect.x = 0;
                rect.y = (1.0f - scaleHeight) / 2.0f;
                mainCamera.rect = rect;
            }
            else
            {
                // Wide screen (desktop) - add bars on top/bottom
                float scaleWidth = 1.0f / scaleHeight;
                Rect rect = mainCamera.rect;
                rect.width = scaleWidth;
                rect.height = 1.0f;
                rect.x = (1.0f - scaleWidth) / 2.0f;
                rect.y = 0;
                mainCamera.rect = rect;
            }
        }
        else
        {
            // Zoom to fit approach (crops top/bottom or sides)
            if (scaleHeight < 1.0f)
            {
                // Tall screen - zoom out to show more vertical content
                mainCamera.orthographicSize = initialOrthographicSize / scaleHeight;
            }
            else
            {
                // Wide screen - use original size
                mainCamera.orthographicSize = initialOrthographicSize;
            }
        }
    }
}