using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ROLE: Is the owner standing on something solid? Reads the collision normals.
/// PATTERNS: none - a plain component behind IGroundCheck.
///
/// Tells whether we are standing on ANY solid object - floor tile, spikes, crate, enemy -
/// by reading the physics contact normals instead of expecting a special component on
/// the other side.
/// </summary>
public class GroundCheck : MonoBehaviour, IGroundCheck
{
    [Tooltip("How flat a surface must be to count as ground. 1 = perfectly flat, 0 = vertical wall.")]
    [SerializeField] private float minGroundNormal = 0.5f;

    private readonly List<Collider2D> groundContacts = new List<Collider2D>();

    public bool IsGrounded
    {
        get
        {
            DropDeadContacts();
            return groundContacts.Count > 0;
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        UpdateContact(col);
    }

    /// <summary>
    /// Also refreshed on stay, so sliding off an edge is noticed.
    /// A sleeping Rigidbody2D stops sending Stay - that is fine, the contact
    /// stored on Enter simply remains until Exit.
    /// </summary>
    private void OnCollisionStay2D(Collision2D col)
    {
        UpdateContact(col);
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        groundContacts.Remove(col.collider);
    }

    private void OnDisable()
    {
        groundContacts.Clear();
    }

    private void UpdateContact(Collision2D col)
    {
        if (!IsStandingOn(col))
        {
            groundContacts.Remove(col.collider);
            return;
        }

        if (!groundContacts.Contains(col.collider))
            groundContacts.Add(col.collider);
    }

    /// <summary>Drops contacts whose object was destroyed or disabled meanwhile.</summary>
    private void DropDeadContacts()
    {
        for (int i = groundContacts.Count - 1; i >= 0; i--)
        {
            if (groundContacts[i] == null || !groundContacts[i].gameObject.activeInHierarchy)
                groundContacts.RemoveAt(i);
        }
    }

    private bool IsStandingOn(Collision2D col)
    {
        for (int i = 0; i < col.contactCount; i++)
        {
            // The normal points from the other object towards us:
            // pointing up means that object is under our feet.
            if (col.GetContact(i).normal.y >= minGroundNormal)
                return true;
        }

        return false;
    }
}
