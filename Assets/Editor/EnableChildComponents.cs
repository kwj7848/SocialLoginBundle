using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class EnableChildComponents : MonoBehaviour
{
    public bool enable = false;
    public MeshRenderer[] meshRenderers;

    void Start()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var meshRenderer in meshRenderers)
        {
            meshRenderer.enabled = enable;
        }
    }
    
}
