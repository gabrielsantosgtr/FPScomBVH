using UnityEngine;
using System.Collections.Generic;

public class BVHNode
{
    public Bounds box;              // O volume do cubo (AABB)
    public List<BVHNode> children;  // Filhos (se houver)
    public MeshCollider meshRef;    // Referência para o teste final de precisão

    public BVHNode(Bounds b)
    {
        box = b;
        children = new List<BVHNode>();
    }

    // Desenha os Gizmos para visualização (Verde para raiz/nós, Amarelo para folhas)
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