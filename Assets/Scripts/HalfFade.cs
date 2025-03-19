using UnityEngine;

public class HalfFade : MonoBehaviour
{

    public float opacity = 0.1f;
    // Duration is immediate in this case; adjust if you want a gradual fade
    private void Start()
    {
        // Apply half opacity to this object's materials
        ApplyHalfFade();

        // Attach this component to all existing child objects
        AddHalfFadeToChildren();
    }

    // Called when the list of children changes (e.g., when a new child is added)
    private void OnTransformChildrenChanged()
    {
        AddHalfFadeToChildren();
    }

    // Sets the alpha of all materials on this GameObject's Renderer to 0.5
    private void ApplyHalfFade()
    {
        MeshRenderer rend = this.GetComponent<MeshRenderer>();
        if (rend != null)
        {
            // Loop through each material and set its alpha
            foreach (Material mat in rend.materials)
            {
                SetMaterialTransparent(mat);
                Color col = mat.color;
                col.a = opacity;  // 50% opacity
                mat.color = col;
            }
        }
    }

    // Adds the HalfFade component to all direct children that don't already have it
    private void AddHalfFadeToChildren()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.GetComponent<HalfFade>() == null)
            {
                child.gameObject.AddComponent<HalfFade>();
            }
        }
    }

    // Configure the material to support transparency (for Standard shader)
    private void SetMaterialTransparent(Material material)
    {
        if (material.HasProperty("_Mode"))
        {
            // Set mode to Transparent (3 is Transparent for Standard Shader)
            material.SetFloat("_Mode", 3);
        }
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
    }
}
