using UnityEngine;
using TMPro;

public class SceneText : MonoBehaviour
{
    public TMP_Text text; // Reference to the TMP_Text component
    public float duration = 2.0f; // Duration of the effect

    private float timer = 0.0f;
    private Vector3 initialScale;
    private Color initialColor;

    void Start()
    {

        initialScale = text.transform.localScale;
        initialColor = text.color;
    }

    void Update()
    {
        if (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // Scale up
            text.transform.localScale = Vector3.Lerp(initialScale, initialScale * 2.0f, progress);

            // Fade out
            text.color = Color.Lerp(initialColor, new Color(initialColor.r, initialColor.g, initialColor.b, 0), progress);
        }
    }
}
