using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class UIOptimizeTool : EditorWindow
{
    [MenuItem("Tools/Vertigo/Optimize UI")]
    public static void OptimizeUI()
    {
        int optimizedImages = 0;
        int optimizedButtons = 0;

        var allImages = GameObject.FindObjectsOfType<Image>(true);
        var allButtons = GameObject.FindObjectsOfType<Button>(true);

        foreach (var img in allImages)
        {
            bool isButtonGraphic = false;
            bool isViewport = false;

            // 1) Button içindeki grafik mi?
            var btn = img.GetComponentInParent<Button>();
            if (btn != null && btn.targetGraphic == img)
                isButtonGraphic = true;

            // 2) Mask içeren objeler viewport oluyor
            var mask = img.GetComponent<Mask>();
            if (mask != null)
                isViewport = true;

            // VIEWPORT ise özel kurallar
            if (isViewport)
            {
                img.raycastTarget = false;     // viewport raycast kapalı
                img.maskable = true;           // maskable açık
                optimizedImages++;
                continue;
            }

            // BUTTON ARKA PLAN GÖRSELİ
            if (isButtonGraphic)
            {
                img.raycastTarget = false;   // Button kendisi raycast alır, graphic almaz
                optimizedImages++;
                continue;
            }

            // NORMAL İKON VE GÖRSELLER
            img.raycastTarget = false;
            img.maskable = false;

            // Sprite border varsa Sliced yapalım
            if (img.sprite != null)
            {
                if (HasBorder(img.sprite))
                    img.type = Image.Type.Sliced;
                else
                    img.type = Image.Type.Simple;  // ikonlar için doğru olan
            }

            optimizedImages++;
        }

        // BUTTON AYARLARI
        foreach (var btn in allButtons)
        {
            if (btn.targetGraphic != null)
                optimizedButtons++;

            // Buton kendi raycast'ini açık tutar
            var graphic = btn.targetGraphic;
            if (graphic != null)
                graphic.raycastTarget = false;

            // Button component input alır
            var img = btn.GetComponent<Image>();
            if (img != null)
                img.raycastTarget = true;
        }

        Debug.Log($"[Vertigo UI Optimizer] Completed! ✓ Images: {optimizedImages}, ✓ Buttons: {optimizedButtons}");
    }

    private static bool HasBorder(Sprite sprite)
    {
        return sprite.border.x > 0 ||
               sprite.border.y > 0 ||
               sprite.border.z > 0 ||
               sprite.border.w > 0;
    }
}