using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyVisualRandomizer : MonoBehaviour
{
    [Header("Optional: Sprites")]
    [SerializeField] private Sprite[] sprites; // 0개면 색만 사용

    [Header("Colors (used when no sprites or 함께 사용 가능)")]
    [SerializeField]
    private Color[] colors =
    {
        Color.red, Color.green, Color.blue, Color.yellow
    };

    private void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();

        // 스프라이트 랜덤(있다면)
        if (sprites != null && sprites.Length > 0)
        {
            sr.sprite = sprites[Random.Range(0, sprites.Length)];
        }

        // 색 랜덤(있다면)
        if (colors != null && colors.Length > 0)
        {
            sr.color = colors[Random.Range(0, colors.Length)];
        }
    }
}
