using UnityEngine;

/// Attach this to a level prefab root. It will recolor all child renderers
/// whose GameObjects are on the specified groundLayer at Start().
/// You can also call ApplyColor() manually at runtime (e.g., after spawning).
public class GroundChildrenColorizer : MonoBehaviour
{
    [Header("Which objects to recolor")]
    public LayerMask groundLayer;          // set this to your "Ground" layer
    public bool includeInactiveChildren = true;

    [Header("Coloring")]
    public Color targetColor = Color.white;
    public bool onlyChangeMaterialColorProperty = false; // false = change material.color; true = change "_Color" if present

    void Start()
    {
        ApplyColor();
    }

    /// Re-apply the color to all eligible children.
    public void ApplyColor()
    {
        var renderers = GetComponentsInChildren<Renderer>(includeInactiveChildren);
        int mask = groundLayer.value == 0 ? Physics.DefaultRaycastLayers : groundLayer.value;

        foreach (var r in renderers)
        {
            if (!r) continue;
            GameObject go = r.gameObject;
            int goLayerBit = 1 << go.layer;
            if ((mask & goLayerBit) == 0) continue;

            // Clone materials to avoid editing shared materials in the project
            var mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (!mats[i]) continue;

                if (onlyChangeMaterialColorProperty)
                {
                    if (mats[i].HasProperty("_Color"))
                        mats[i].SetColor("_Color", targetColor);
                }
                else
                {
                    // Directly set the main color (works for Standard shaders)
                    mats[i].color = targetColor;
                }
            }
        }
    }
}