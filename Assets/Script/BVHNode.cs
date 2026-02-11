using UnityEngine;
using System.Collections.Generic;

public class BVHNode
{
    public Bounds box;            
    public List<BVHNode> children; 
    public MeshCollider meshRef;  

    public BVHNode(Bounds b)
    {
        box = b;
        children = new List<BVHNode>();
    }

    public void DrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(box.center, box.size);
        
        foreach (var child in children)
        {
            child.DrawGizmos();
        }
    }
}