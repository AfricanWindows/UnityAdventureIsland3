using System.Collections;
using UnityEngine;

/// <summary>
/// Exercise item 3: a floor that is visible for 2 seconds and gone for 2 seconds.
///
/// A coroutine is the right tool here, not a Task: this is a frame based timer that
/// must stop by itself when the object is disabled or the level restarts.
///
/// The tile is hidden by turning the renderer and the collider OFF, never by
/// SetActive(false) - a disabled GameObject kills its own coroutine and the floor
/// would stay invisible forever.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BlinkTile : MonoBehaviour
{
    [Tooltip("Seconds the floor stays visible")]
    [SerializeField] private float visibleTime = 2f;

    [Tooltip("Seconds the floor stays hidden")]
    [SerializeField] private float hiddenTime = 2f;

    private SpriteRenderer spriteRenderer;
    private Collider2D tileCollider;
    private Coroutine blinkRoutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        tileCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    private void OnDisable()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        // Leave the floor solid, so a disabled tile never traps the player.
        SetVisible(true);
    }

    private IEnumerator BlinkRoutine()
    {
        // Cached once instead of allocating a new WaitForSeconds every loop.
        WaitForSeconds visibleWait = new WaitForSeconds(visibleTime);
        WaitForSeconds hiddenWait = new WaitForSeconds(hiddenTime);

        while (true)
        {
            SetVisible(true);
            yield return visibleWait;

            SetVisible(false);
            yield return hiddenWait;
        }
    }

    private void SetVisible(bool isVisible)
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = isVisible;

        if (tileCollider != null)
            tileCollider.enabled = isVisible;
    }
}
