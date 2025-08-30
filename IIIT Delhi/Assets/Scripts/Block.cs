using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Per-block component. Put this on the block prefab.
/// - Has a level (1..3) and upgrade button (assigned in inspector).
/// - On button press the block will request the UpgradeManager to upgrade itself.
/// - Assumes sprite renderer & BlockHealth are on the same GameObject (as you specified).
/// </summary>
public class Block : MonoBehaviour
{
    [Header("Runtime state")]
    public int level = 1;                           // starts at 1

    [Header("Refs (assign in inspector or auto-find)")]
    public SpriteRenderer spriteRenderer;
    public BlockHealth blockHealth;
    public TextMeshProUGUI levelText;               // world-space TMP text showing level
    public Button upgradeButton;                    // button for this block

    void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (blockHealth == null) blockHealth = GetComponent<BlockHealth>();
    }

    void Start()
    {
        // initialize visuals/stats from UpgradeManager arrays if present
        if (UpgradeManager.I != null)
        {
            int idx = Mathf.Clamp(level - 1, 0, UpgradeManager.I.blockLevelSprites.Length - 1);
            if (UpgradeManager.I.blockLevelSprites != null && UpgradeManager.I.blockLevelSprites.Length > idx)
            {
                var s = UpgradeManager.I.blockLevelSprites[idx];
                if (s != null) spriteRenderer.sprite = s;
            }

            if (UpgradeManager.I.blockMaxHealthPerLevel != null && UpgradeManager.I.blockMaxHealthPerLevel.Length > idx)
            {
                float hp = UpgradeManager.I.blockMaxHealthPerLevel[idx];
                if (blockHealth != null)
                {
                    blockHealth.maxHealth = hp;
                    blockHealth.HealTo(hp);
                }
            }
        }

        UpdateLevelText();
    }

    public void UpdateLevelText()
    {
        if (levelText != null) levelText.text = $"L{level}";
    }

    /// <summary>
    /// Called by this block's UI button. It selects this block on the manager and triggers the manager upgrade.
    /// This matches your requirement that each block has its own button and is upgraded alone.
    /// </summary>
    public void OnUpgradeButtonClicked()
    {
        if (UpgradeManager.I == null)
        {
            Debug.LogWarning("[Block] UpgradeManager.I not found.");
            return;
        }

        UpgradeManager.I.SelectBlock(this);
        UpgradeManager.I.UpgradeBlockMaxHealth();
    }

    /// <summary>
    /// Apply an upgrade to this block (called by UpgradeManager).
    /// - newLevel: 1..3
    /// - newSprite: may be null => keep existing
    /// - newMaxHp: absolute max hp for level
    /// </summary>
    public void ApplyUpgrade(int newLevel, Sprite newSprite, float newMaxHp)
    {
        level = Mathf.Clamp(newLevel, 1, 3);

        if (newSprite != null && spriteRenderer != null) spriteRenderer.sprite = newSprite;

        if (blockHealth != null)
        {
            blockHealth.maxHealth = newMaxHp;
            // you requested that upgrading restores to full health
            blockHealth.HealTo(newMaxHp);
        }

        UpdateLevelText();

        if (level >= 3) MarkUpgradeButtonHidden();
    }

    /// <summary>
    /// Hides this block's own upgrade button (called when reaching max)
    /// </summary>
    public void MarkUpgradeButtonHidden()
    {
        if (upgradeButton != null) upgradeButton.gameObject.SetActive(false);
    }
}
