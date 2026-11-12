using UnityEngine;

public class PlayerVisualUI : MonoBehaviour
{
    [SerializeField] public MeshRenderer playerHead;
    [SerializeField] public MeshRenderer playerBody;

    public Material playerMaterial;

    void Awake()
    {
        EnsureMaterial();
    }

    private void EnsureMaterial()
    {
        if (playerMaterial != null || playerHead == null || playerHead.sharedMaterial == null) return;
        playerMaterial = new Material(playerHead.sharedMaterial);
        playerHead.sharedMaterial = playerMaterial;
        if (playerBody != null) playerBody.sharedMaterial = playerMaterial;
    }

    public void SetPlayerColor(Color color)
    {
        EnsureMaterial();
        if (playerMaterial == null) return;
        playerMaterial.color = color;
    }
}
