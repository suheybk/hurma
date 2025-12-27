using UnityEngine;
using UnityEngine.InputSystem;

public class Soil : MonoBehaviour
{
    public bool IsOccupied = false;
    public HurmaTree PlantedTree;
    public GameObject TreePrefab; // Reference to the tree prefab to spawn

    private Renderer soilRenderer;
    private Color dryColor = new Color(0.8f, 0.7f, 0.5f); // Sandy
    private Color wetColor = new Color(0.4f, 0.3f, 0.2f); // Dark Earth
    
    private void Start()
    {
        soilRenderer = GetComponent<Renderer>();
        if(soilRenderer) soilRenderer.material.color = dryColor;
    }

    public void PlantSeed(bool silent = false)
    {
        if (IsOccupied)
        {
            // If already occupied, maybe we are trying to water it?
            if (PlantedTree != null)
            {
                PlantedTree.Water(50f); // Generous water
                Debug.Log($"Soil Watered. Tree Water Level: {PlantedTree.WaterLevel}");
                UpdateSoilVisuals(true);
            }
            return;
        }

        if (TreePrefab != null)
        {
            // Plant slightly above ground so it's not buried
            Vector3 plantPos = transform.position + new Vector3(0, 0.2f, 0);
            GameObject treeObj = Instantiate(TreePrefab, plantPos, Quaternion.identity);
            PlantedTree = treeObj.GetComponent<HurmaTree>();
            IsOccupied = true;
            if (!silent) Debug.Log("Seed planted successfully.");
            
            // Initial water
            UpdateSoilVisuals(true);
        }
        else
        {
            Debug.LogError("Tree Prefab is not assigned in Soil script!");
        }
    }

    public AudioClip WaterSound;

    public void UpdateSoilVisuals(bool isWet)
    {
        if (soilRenderer)
        {
            // Simple logic: If wet, darken. Then slowly dry?
            // For prototype, just set wet color, and let it dry over time (optional)
            // Or better: LERP based on Tree's water level if we can access it.
            
            if (isWet)
            {
               StartCoroutine(WetSoilEffect());
               if (GameAudioManager.Instance != null && WaterSound != null)
               {
                   GameAudioManager.Instance.PlaySFX(WaterSound, 0.5f);
               }
            }
        }
    }
    
    private System.Collections.IEnumerator WetSoilEffect()
    {
        // Flash to wet color then slowly fade back to dry-ish
        if(!soilRenderer) yield break;
        
        float t = 0;
        while(t < 1f)
        {
            t += Time.deltaTime * 2f; // Fast wet
            soilRenderer.material.color = Color.Lerp(dryColor, wetColor, t);
            yield return null;
        }
        
        // Stay wet for a bit? Or just stay wet based on logic. 
        // Let's leave it wet-ish for now.
    }

    private void Update()
    {
        // Check for click (New Input System)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Accept click if it hit this object OR any of its children (the visual mound)
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    PlantSeed(false); // Default interaction is not silent
                }
            }
        }
    }
}
