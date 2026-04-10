using UnityEngine;

/// <summary>
/// ScriptableObject that stores every piece of Scrollbook lore.
/// Create one via Assets ? Create ? BitBound ? Scrollbook Database,
/// then fill in the arrays in the Inspector.
/// </summary>
[CreateAssetMenu(fileName = "ScrollbookDatabase", menuName = "BitBound/Scrollbook Database")]
public class ScrollbookDatabase : ScriptableObject
{
    [Header("Characters")]
    [Tooltip("Backstory / personality entries for Bits, Thief, Ninja, Wiz Kid, Crystal, and Cubit.")]
    public ScrollbookEntry[] characters = new ScrollbookEntry[]
    {
        new ScrollbookEntry { title = "Bits",    description = "Enter Bits' backstory here..." },
        new ScrollbookEntry { title = "Thief",   description = "Enter Thief's backstory here..." },
        new ScrollbookEntry { title = "Ninja",   description = "Enter Ninja's backstory here..." },
        new ScrollbookEntry { title = "Wiz Kid", description = "Enter Wiz Kid's backstory here..." },
        new ScrollbookEntry { title = "Crystal",  description = "Enter Crystal's backstory here..." },
        new ScrollbookEntry { title = "Cubit",   description = "Enter Cubit's backstory here..." },
    };

    [Header("Obstacles")]
    [Tooltip("Detail entries for Spikes, Aliens, Slimes, Magic Bolts, etc.")]
    public ScrollbookEntry[] obstacles = new ScrollbookEntry[]
    {
        new ScrollbookEntry { title = "Spikes",      description = "Enter Spikes details here..." },
        new ScrollbookEntry { title = "Aliens",      description = "Enter Aliens details here..." },
        new ScrollbookEntry { title = "Slimes",      description = "Enter Slimes details here..." },
        new ScrollbookEntry { title = "Magic Bolts", description = "Enter Magic Bolts details here..." },
    };

    [Header("Spritz")]
    [Tooltip("The history of the planet Spritz. Can be one long entry or split into sections.")]
    public ScrollbookEntry[] spritzHistory = new ScrollbookEntry[]
    {
        new ScrollbookEntry
        {
            title = "The World of Spritz",
            description = "Enter the full history of Spritz here..."
        },
    };

    [Header("The Founders")]
    [Tooltip("Entries for the five legendary founders of the major cities.")]
    public ScrollbookEntry[] founders = new ScrollbookEntry[]
    {
        new ScrollbookEntry { title = "Founder of Pixelville",    description = "Enter founder lore here..." },
        new ScrollbookEntry { title = "Founder of Greenwood",     description = "Enter founder lore here..." },
        new ScrollbookEntry { title = "Founder of Ninja Valley",  description = "Enter founder lore here..." },
        new ScrollbookEntry { title = "Founder of Frosty Outpost", description = "Enter founder lore here..." },
        new ScrollbookEntry { title = "Founder of Wizardspeak",   description = "Enter founder lore here..." },
    };
}
