using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// VIEW of the power bar: a row of little images, of which the first Current ones are lit.
///
/// It only draws. It holds no reference to the model, contains no rule about starving, and
/// never decides when the value changes - it is told what to show.
///
/// It also does not decide HOW MANY sockets the bar has. That number is the model's Max,
/// and it arrives as the second argument of Render. This used to be a serialised field
/// here as well, which meant the ceiling lived in two places: raising Max Power on the
/// player to 20 left the bar built for 15, and five segments were simply unreachable.
/// One value, one owner - the view follows (Single Responsibility).
/// </summary>
public class PowerBarView : MonoBehaviour, IPowerView
{
    [Tooltip("One segment of the bar. Any UI Image will do - a white sprite is enough. " +
             "Its Rect Transform Width/Height is what sets the size of every segment.")]
    [SerializeField] private Image segmentPrefab;

    [Tooltip("Parent for the segments. Empty = this object. Put a Horizontal Layout Group " +
             "on it and the spacing takes care of itself.")]
    [SerializeField] private RectTransform container;

    [Header("Colours")]
    [SerializeField] private Color filledColor = Color.white;

    [Tooltip("Colour of a spent segment. Low alpha leaves a faint socket, 0 hides it.")]
    [SerializeField] private Color emptyColor = new Color(1f, 1f, 1f, 0.15f);

    private Image[] segments;

    /// <summary>
    /// Draws the bar, building the sockets first if the ceiling changed.
    ///
    /// The rebuild is not a per-frame cost: max only changes when the level or the
    /// character does, so this is one allocation at start-up and then nothing.
    /// </summary>
    public void Render(int current, int max)
    {
        if (!EnsureSegments(max))
            return;

        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] != null)
                segments[i].color = i < current ? filledColor : emptyColor;
        }
    }

    /// <summary>Builds exactly <paramref name="max"/> sockets. Returns false if it could not.</summary>
    private bool EnsureSegments(int max)
    {
        if (max < 0)
            max = 0;

        if (segments != null && segments.Length == max)
            return true;

        if (segmentPrefab == null)
        {
            Debug.LogError("PowerBarView: no Segment Prefab assigned on " + gameObject.name, this);
            return false;
        }

        ClearSegments();

        RectTransform parent = container != null ? container : (RectTransform)transform;
        segments = new Image[max];

        for (int i = 0; i < max; i++)
        {
            Image segment = Instantiate(segmentPrefab, parent);
            segment.name = "Segment" + i;
            segments[i] = segment;
        }

        return true;
    }

    private void ClearSegments()
    {
        if (segments == null)
            return;

        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] == null)
                continue;

            // Unparented BEFORE being destroyed: Destroy is deferred to the end of the
            // frame, and a Horizontal Layout Group would spend that frame still laying out
            // the corpses next to the new sockets.
            segments[i].transform.SetParent(null, false);
            Destroy(segments[i].gameObject);
        }

        segments = null;
    }
}
