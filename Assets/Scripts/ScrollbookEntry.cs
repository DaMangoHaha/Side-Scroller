using UnityEngine;

/// <summary>
/// A single lore entry in the Scrollbook.
/// Used inside ScrollbookDatabase to hold per-item data for Characters,
/// Obstacles, Spritz history, and Founders.
/// </summary>
[System.Serializable]
public class ScrollbookEntry
{
    [Tooltip("Display name shown at the top of the entry (e.g. 'Bits', 'Spikes', 'Pixelville').")]
    public string title;

    [Tooltip("Optional portrait / icon shown next to the entry text.")]
    public Sprite icon;

    [Tooltip("The full lore text for this entry.")]
    [TextArea(4, 12)]
    public string description;
}
