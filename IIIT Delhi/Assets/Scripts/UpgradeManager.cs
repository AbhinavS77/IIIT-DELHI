using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Singleton UpgradeManager
/// - Handles gun damage upgrades (level 1..10)
/// - Handles gun rate-of-fire upgrades (level 1..10)
/// - Handles block max-health upgrades (level 1..3) for the currently selected block
/// - Updates TMP UI and hides upgrade buttons at max levels
/// 
/// Inspector setup (brief):
/// - Assign playerGun (PlayerGun component in scene)
/// - Assign two TMP texts: dmgLevelText, rofLevelText
/// - Assign two Buttons: dmgUpgradeButton, rofUpgradeButton (optional but recommended)
/// - Fill arrays: damagePerLevel (len 10), rofPerLevel (len 10),
///   damageUpgradeCosts (len 10), rofUpgradeCosts (len 10)
/// - Assign projectile sprite groups: spriteGroupA (levels 1..4), spriteGroupB (5..7), spriteGroupC (8..10)
/// - For blocks: blockMaxHealthPerLevel (len 3), blockUpgradeCosts (len 3), blockLevelSprites (len 3)
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager I { get; private set; }

    [Header("References")]
    public PlayerGun playerGun;                       // assign in inspector
    public TextMeshProUGUI dmgLevelText;              // shows damage level
    public TextMeshProUGUI rofLevelText;              // shows rate-of-fire level
    public Button dmgUpgradeButton;                   // optional
    public Button rofUpgradeButton;                   // optional

    [Header("Gun Damage (levels 1..10)")]
    [Tooltip("Absolute damage value for each level (index 0 => level1, index 9 => level10)")]
    public float[] damagePerLevel = new float[10];
    [Tooltip("Currency cost to go from current level L to L+1. Index 0 = cost to upgrade from level1->2")]
    public int[] damageUpgradeCosts = new int[10];

    [Header("Gun Rate of Fire (levels 1..10)")]
    [Tooltip("Absolute rateOfFire values for each level (shots per second). Index 0 => level1")]
    public float[] rofPerLevel = new float[10];
    [Tooltip("Currency cost to go from current level L to L+1. Index 0 = cost to upgrade from level1->2")]
    public int[] rofUpgradeCosts = new int[10];

    [Header("Projectile Sprite groups")]
    [Tooltip("Sprite used for damage levels 1..4")]
    public Sprite projSpriteGroupA;   // levels 1-4
    [Tooltip("Sprite used for damage levels 5..7")]
    public Sprite projSpriteGroupB;   // levels 5-7
    [Tooltip("Sprite used for damage levels 8..10")]
    public Sprite projSpriteGroupC;   // levels 8-10

    [Header("Gun level state (runtime)")]
     public int gunDamageLevel = 1;
     public int gunRofLevel = 1;
    const int MAX_GUN_LEVEL = 10;

    [Header("Block upgrades (levels 1..3)")]
    [Tooltip("Index 0 = level1, index 1 = level2, index 2 = level3")]
    public float[] blockMaxHealthPerLevel = new float[3];
    public int[] blockUpgradeCosts = new int[3];
    public Sprite[] blockLevelSprites = new Sprite[3];
    const int MAX_BLOCK_LEVEL = 3;

    // block currently selected (set by Block when its upgrade button is clicked)
    [HideInInspector] public Block selectedBlock;

    void Awake()
    {
        if (I == null) I = this;
        else if (I != this) Destroy(gameObject);

        // Basic safety checks
        if (damagePerLevel.Length != MAX_GUN_LEVEL) Debug.LogWarning("damagePerLevel should have length 10 (levels 1..10).");
        if (rofPerLevel.Length != MAX_GUN_LEVEL) Debug.LogWarning("rofPerLevel should have length 10 (levels 1..10).");
    }

    void Start()
    {
        // initialize player gun values from level 1 arrays if available
        ApplyGunValuesToPlayer();
        UpdateUI();
    }

    #region Public upgrade API (call these from buttons or other scripts)

    /// <summary>
    /// Upgrades gun damage by one level. Returns true if upgrade succeeded.
    /// Usage: call UpgradeManager.I.UpgradeGunDamage()
    /// </summary>
    public bool UpgradeGunDamage()
    {
        if (gunDamageLevel >= MAX_GUN_LEVEL)
        {
            Debug.LogWarning("[UpgradeManager] Gun damage already at max level.");
            if (dmgUpgradeButton != null) dmgUpgradeButton.gameObject.SetActive(false);
            return false;
        }

        int costIndex = gunDamageLevel - 1; // index 0 = cost to upgrade 1->2
        int cost = (damageUpgradeCosts != null && damageUpgradeCosts.Length > costIndex) ? damageUpgradeCosts[costIndex] : 0;

        // check currency using the global Manager API you described
        if (!HasEnoughCurrency(cost))
        {
            Debug.LogWarning("[UpgradeManager] Not enough currency to upgrade gun damage.");
            return false;
        }

        DeductCurrency(cost);

        // advance level
        gunDamageLevel = Mathf.Min(MAX_GUN_LEVEL, gunDamageLevel + 1);

        // apply absolute damage value for the new level (if provided)
        if (damagePerLevel != null && damagePerLevel.Length >= gunDamageLevel)
        {
            float newDamage = damagePerLevel[gunDamageLevel - 1];
            if (playerGun != null) playerGun.damage = newDamage;
        }

        // update UI and projectile sprite group usage (PlayerGun will use this getter when spawning)
        ApplyGunValuesToPlayer();
        UpdateUI();

        // if reached max, hide button
        if (gunDamageLevel >= MAX_GUN_LEVEL && dmgUpgradeButton != null)
            dmgUpgradeButton.gameObject.SetActive(false);

        return true;
    }

    /// <summary>
    /// Upgrades gun rate-of-fire by one level. Returns true if upgrade succeeded.
    /// Usage: call UpgradeManager.I.UpgradeGunRateOfFire()
    /// </summary>
    public bool UpgradeGunRateOfFire()
    {
        if (gunRofLevel >= MAX_GUN_LEVEL)
        {
            Debug.LogWarning("[UpgradeManager] Gun rate-of-fire already at max level.");
            if (rofUpgradeButton != null) rofUpgradeButton.gameObject.SetActive(false);
            return false;
        }

        int costIndex = gunRofLevel - 1; // index 0 = cost to upgrade 1->2
        int cost = (rofUpgradeCosts != null && rofUpgradeCosts.Length > costIndex) ? rofUpgradeCosts[costIndex] : 0;

        if (!HasEnoughCurrency(cost))
        {
            Debug.LogWarning("[UpgradeManager] Not enough currency to upgrade rate-of-fire.");
            return false;
        }

        DeductCurrency(cost);

        gunRofLevel = Mathf.Min(MAX_GUN_LEVEL, gunRofLevel + 1);

        // apply absolute rof value for the new level (if provided)
        if (rofPerLevel != null && rofPerLevel.Length >= gunRofLevel)
        {
            float newRof = rofPerLevel[gunRofLevel - 1];
            if (playerGun != null) playerGun.rateOfFire = newRof;
        }

        ApplyGunValuesToPlayer();
        UpdateUI();

        if (gunRofLevel >= MAX_GUN_LEVEL && rofUpgradeButton != null)
            rofUpgradeButton.gameObject.SetActive(false);

        return true;
    }

    /// <summary>
    /// Upgrades the currently selected block (selectedBlock must be set first).
    /// Blocks call UpgradeManager.I.SelectBlock(this) then UpgradeManager.I.UpgradeBlockMaxHealth()
    /// Returns true if upgrade succeeded.
    /// </summary>
    public bool UpgradeBlockMaxHealth()
    {
        if (selectedBlock == null)
        {
            Debug.LogWarning("[UpgradeManager] No block selected for upgrade.");
            return false;
        }

        int cur = selectedBlock.level;
        if (cur >= MAX_BLOCK_LEVEL)
        {
            Debug.LogWarning("[UpgradeManager] Block already at max level.");
            selectedBlock.MarkUpgradeButtonHidden(); // hide the block's own button
            return false;
        }

        int costIndex = cur - 1; // index 0 = cost to upgrade 1->2
        int cost = (blockUpgradeCosts != null && blockUpgradeCosts.Length > costIndex) ? blockUpgradeCosts[costIndex] : 0;

        if (!HasEnoughCurrency(cost))
        {
            Debug.LogWarning("[UpgradeManager] Not enough currency to upgrade block.");
            return false;
        }

        DeductCurrency(cost);

        // increment level
        int newLevel = Mathf.Min(MAX_BLOCK_LEVEL, cur + 1);

        // sprite & health for new level
        Sprite newSprite = (blockLevelSprites != null && blockLevelSprites.Length >= newLevel) ? blockLevelSprites[newLevel - 1] : null;
        float newMaxHp = (blockMaxHealthPerLevel != null && blockMaxHealthPerLevel.Length >= newLevel) ? blockMaxHealthPerLevel[newLevel - 1] : (selectedBlock.blockHealth != null ? selectedBlock.blockHealth.maxHealth : 0f);

        // apply to the block
        selectedBlock.ApplyUpgrade(newLevel, newSprite, newMaxHp);

        // if block reached max level, hide its button (Block.ApplyUpgrade does this already, but keep safe)
        if (selectedBlock.level >= MAX_BLOCK_LEVEL) selectedBlock.MarkUpgradeButtonHidden();

        // clear selection
        selectedBlock = null;

        return true;
    }

    #endregion

    #region Utilities & helpers

    void ApplyGunValuesToPlayer()
    {
        if (playerGun == null) return;

        // set current damage from array if exists
        if (damagePerLevel != null && damagePerLevel.Length >= gunDamageLevel)
        {
            playerGun.damage = damagePerLevel[gunDamageLevel - 1];
        }

        // set current rof from array if exists
        if (rofPerLevel != null && rofPerLevel.Length >= gunRofLevel)
        {
            playerGun.rateOfFire = rofPerLevel[gunRofLevel - 1];
        }
    }

    void UpdateUI()
    {
        if (dmgLevelText != null) dmgLevelText.text = $"DMG L{gunDamageLevel}";
        if (rofLevelText != null) rofLevelText.text = $"ROF L{gunRofLevel}";
    }

    /// <summary>
    /// Return the projectile sprite to use based on a damage level (1..10) using your mapping:
    /// 1-4 => group A, 5-7 => group B, 8-10 => group C
    /// </summary>
    public Sprite GetProjectileSpriteForDamageLevel(int level)
    {
        if (level <= 4) return projSpriteGroupA;
        if (level <= 7) return projSpriteGroupB;
        return projSpriteGroupC;
    }

    /// <summary>
    /// Select a block so that UpgradeBlockMaxHealth() can be called without parameters.
    /// Blocks should call this before calling UpgradeBlockMaxHealth().
    /// </summary>
    public void SelectBlock(Block b)
    {
        selectedBlock = b;
    }

    bool HasEnoughCurrency(int cost)
    {
        if (cost <= 0) return true;
        // call external Manager API you mentioned. The project's Manager class should implement these.
        try
        {
            if (GameManager.I != null)
            {
                int current = GameManager.I.GetScore();
                return current >= cost;
            }
            else
            {
                Debug.LogWarning("[UpgradeManager] Manager.I is null — unable to check currency. Treating as insufficient.");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[UpgradeManager] Exception when querying Manager.I.getScore(): " + ex.Message);
            return false;
        }
    }

    void DeductCurrency(int cost)
    {
        if (cost <= 0) return;
        try
        {
            if (GameManager.I != null)
            {
                GameManager.I.DeductScore(cost); 
            }
            else
            {
                Debug.LogWarning("[UpgradeManager] Manager.I is null — cannot deduct currency.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[UpgradeManager] Exception when calling Manager.I.deductScore(): " + ex.Message);
        }
    }

    #endregion
}
