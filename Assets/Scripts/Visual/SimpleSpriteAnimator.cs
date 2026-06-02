using UnityEngine;

// Plays an array of sprites one after another on a SpriteRenderer.
// This is our animation system instead of Unity's Animator: just a frame counter.
[RequireComponent(typeof(SpriteRenderer))]
public class SimpleSpriteAnimator : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Sprite[] frames;
    private float framesPerSecond = 10f;
    private bool loop = true;
    private int currentIndex;
    private float timer;

    public bool IsFinished { get; private set; }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Play(Sprite[] newFrames, float fps, bool shouldLoop)
    {
        if (newFrames == null || newFrames.Length == 0)
        {
            return;
        }

        // Already playing this clip -> keep going (don't restart walk/idle every frame).
        if (frames == newFrames && !IsFinished)
        {
            framesPerSecond = Mathf.Max(1f, fps);
            loop = shouldLoop;
            return;
        }

        frames = newFrames;
        framesPerSecond = Mathf.Max(1f, fps);
        loop = shouldLoop;
        currentIndex = 0;
        timer = 0f;
        IsFinished = false;
        spriteRenderer.sprite = frames[0];
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0 || IsFinished)
        {
            return;
        }

        timer += Time.deltaTime;
        float frameDuration = 1f / framesPerSecond;
        if (timer < frameDuration)
        {
            return;
        }

        timer -= frameDuration;
        currentIndex++;

        if (currentIndex >= frames.Length)
        {
            if (loop)
            {
                currentIndex = 0;
            }
            else
            {
                currentIndex = frames.Length - 1;
                IsFinished = true;
            }
        }

        spriteRenderer.sprite = frames[currentIndex];
    }
}
