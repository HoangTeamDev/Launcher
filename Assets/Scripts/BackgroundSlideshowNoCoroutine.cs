using UnityEngine;
using UnityEngine.UI;

public class BackgroundSlideshowNoCoroutine : MonoBehaviour
{
    public Image background;           // Ảnh nền UI
    public Sprite[] backgrounds;       // Danh sách ảnh
    public float changeInterval = 5f;  // Thời gian đổi ảnh
    public float fadeDuration = 1f;    // Thời gian mờ dần

    private int currentIndex = 0;
    private float timer = 0f;
    private float fadeTimer = 0f;
    private bool isFading = false;
    private Sprite nextSprite;
    private Color color;

    void Start()
    {
        if (backgrounds.Length > 0)
        {
            background.sprite = backgrounds[0];
            color = background.color;
        }
    }

    void Update()
    {
        if (backgrounds.Length <= 1) return;

        timer += Time.deltaTime;

        // Đến lúc đổi ảnh
        if (timer >= changeInterval && !isFading)
        {
            timer = 0f;
            currentIndex = (currentIndex + 1) % backgrounds.Length;
            nextSprite = backgrounds[currentIndex];
            isFading = true;
            fadeTimer = 0f;
        }

        // Nếu đang fade
        if (isFading)
        {
            fadeTimer += Time.deltaTime;
            float t = fadeTimer / fadeDuration;

            // Giảm alpha đến 0, đổi sprite, rồi tăng alpha lại
            if (t < 0.5f)
            {
                color.a = Mathf.Lerp(1f, 0f, t * 2);
            }
            else
            {
                if (background.sprite != nextSprite)
                    background.sprite = nextSprite;
                color.a = Mathf.Lerp(0f, 1f, (t - 0.5f) * 2);
            }

            background.color = color;

            if (t >= 1f)
            {
                color.a = 1f;
                background.color = color;
                isFading = false;
            }
        }
    }
}
