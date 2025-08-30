using System.Collections;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [Header("Refs")]
    public GameObject blockPrefab;     // prefab to spawn (BoxCollider2D or SpriteRenderer recommended)
    public Transform spawnPoint;       // column X position and spawn reference
    public Transform parent;           // parent for spawned blocks (e.g., StackParent)

    [Header("Layer / Tag")]
    public string blockLayerName = "Block";
    public string blockTag = "Block";

    [Header("Spawn / Physics")]
    public Vector3 spawnOffset = Vector3.zero;   // inspector offset from spawnPoint
    public float horizontalSearchWidth = 0.5f;   // how wide to search for blocks (small value)
    public float verticalSearchHeight = 50f;     // how tall to search for blocks
    public float upwardImpulse = 5f;             // impulse given to new block (tweak)
    public float spawnYOffset = 0.02f;           // small gap adjustment

    bool isSpawning = false;

    // Public function you call to spawn one block under the stack
    public void SpawnBlockUnderStack()
    {
        if (isSpawning) return;

        if(GameManager.I.GetScore() >= 200){
        StartCoroutine(SpawnRoutine());
    GameManager.I.DeductScore(200);
}
    }

    IEnumerator SpawnRoutine()
    {


        isSpawning = true;

        if (blockPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("[BlockSpawner] Missing prefab or spawnPoint.");
            isSpawning = false;
            yield break;
        }

        // 1) Calculate block height (try BoxCollider2D first, then SpriteRenderer)
        float blockHeight = 1f;
        var prefabCollider = blockPrefab.GetComponent<BoxCollider2D>();
        if (prefabCollider != null)
        {
            blockHeight = Mathf.Abs(prefabCollider.size.y) * Mathf.Abs(blockPrefab.transform.lossyScale.y);
        }
        else
        {
            var sr = blockPrefab.GetComponent<SpriteRenderer>();
            if (sr != null && sr.bounds.size.y > 0.001f)
            {
                blockHeight = sr.bounds.size.y * Mathf.Abs(blockPrefab.transform.lossyScale.y);
            }
        }

        // 2) Find existing blocks in a thin tall box above spawnPoint (same X column)
        Vector2 boxCenter = (Vector2)spawnPoint.position + Vector2.up * (verticalSearchHeight * 0.5f);
        Vector2 boxSize = new Vector2(horizontalSearchWidth, verticalSearchHeight);
        int blockLayer = LayerMask.NameToLayer(blockLayerName);
        int mask = (blockLayer >= 0) ? (1 << blockLayer) : 0;

        Collider2D[] hits = (mask != 0)
            ? Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f, mask)
            : Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f);

        // 3) Compute spawn position: if no hits -> spawnPoint + offset, else spawn just below lowest block + offset
        Vector3 spawnPos;
        if (hits == null || hits.Length == 0)
        {
            spawnPos = spawnPoint.position + spawnOffset;
        }
        else
        {
            float minY = float.PositiveInfinity;
            foreach (var c in hits)
            {
                if (c == null) continue;
                float y = c.transform.position.y;
                if (y < minY) minY = y;
            }

            if (minY == float.PositiveInfinity)
                spawnPos = spawnPoint.position + spawnOffset;
            else
                spawnPos = new Vector3(spawnPoint.position.x + spawnOffset.x,
                                       minY - blockHeight + spawnOffset.y,
                                       spawnPoint.position.z + spawnOffset.z);
        }

        spawnPos.y += spawnYOffset;

        // 4) Instantiate and set tag / layer / parent
        GameObject go = Instantiate(blockPrefab, spawnPos, Quaternion.identity, parent);
        go.tag = blockTag;
        if (blockLayer >= 0) go.layer = blockLayer;

        // 5) Ensure Rigidbody2D exists, then freeze X and rotation, apply upward impulse
        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
        if (rb == null) rb = go.AddComponent<Rigidbody2D>();

        // Keep bodyType as Dynamic so it reacts to other spawned blocks
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.AddForce(Vector2.up * upwardImpulse, ForceMode2D.Impulse);

        // Done � do not convert to kinematic; let physics handle further interactions
        isSpawning = false;
        yield return null;
    }

    // Debug visualization for the search box and spawn offset
    void OnDrawGizmosSelected()
    {
        if (spawnPoint == null) return;

        Gizmos.color = Color.cyan;
        Vector2 boxCenter = (Vector2)spawnPoint.position + Vector2.up * (verticalSearchHeight * 0.5f);
        Vector2 boxSize = new Vector2(horizontalSearchWidth, verticalSearchHeight);
        Gizmos.DrawWireCube(boxCenter, boxSize);

        Gizmos.color = Color.green;
        Vector3 sp = spawnPoint.position + spawnOffset;
        Gizmos.DrawSphere(sp, 0.05f);
    }
}
