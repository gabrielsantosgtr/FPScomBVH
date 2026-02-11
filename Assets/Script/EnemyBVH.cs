using UnityEngine;
using System.Collections.Generic;

public class EnemyBVH : MonoBehaviour
{
    private class BVHNode
    {
        public string nomeDaParte;
        public Bounds bounds; 
        public BVHNode[] children; 
        public Collider meshCollider; 

        public bool Intersecta(Ray raio)
        {
            return bounds.IntersectRay(raio);
        }
    }

    [Header("Configuração")]
    public GameObject raizDaBVH; 
    public Collider malhaDeColisaoPrecisa; 

    private BVHNode raizNode; 

    void Start()
    {
        if (malhaDeColisaoPrecisa != null) malhaDeColisaoPrecisa.enabled = true;

        if (raizDaBVH != null)
        {
            raizNode = ConstruirArvore(raizDaBVH.transform);
        }
    }

    private BVHNode ConstruirArvore(Transform atual)
    {
        BVHNode node = new BVHNode();
        
        node.nomeDaParte = atual.name;

        BoxCollider box = atual.GetComponent<BoxCollider>();
        Collider colisorGenerico = atual.GetComponent<Collider>();
        bool ehObjetoDaMalha = (colisorGenerico == malhaDeColisaoPrecisa);

        if (box != null)
        {
            node.bounds = box.bounds;
            if (atual.gameObject != this.gameObject && atual.gameObject != raizDaBVH && !ehObjetoDaMalha)
            {
                box.enabled = false; 
            }
        }
        else if (ehObjetoDaMalha)
        {
            node.bounds = colisorGenerico.bounds;
        }
        else
        {
            node.bounds = new Bounds(atual.position, Vector3.zero);
        }

        if (ehObjetoDaMalha)
        {
            node.meshCollider = malhaDeColisaoPrecisa;
        }

        List<BVHNode> listaFilhos = new List<BVHNode>();
        foreach (Transform filho in atual)
        {
            if (filho.GetComponent<Collider>() != malhaDeColisaoPrecisa)
            {
                listaFilhos.Add(ConstruirArvore(filho));
            }
        }
        node.children = listaFilhos.ToArray();

        return node;
    }

    public string TestarImpacto(Ray raio)
    {
        if (raizNode == null) return null;
        return VerificarNode(raizNode, raio);
    }

    private string VerificarNode(BVHNode node, Ray raio)
    {

        bool ehContainerVazio = (node.bounds.size == Vector3.zero) && (node.children != null && node.children.Length > 0);

        if (!ehContainerVazio)
        {
            if (!node.Intersecta(raio)) return null;
        }

        if (node.children == null || node.children.Length == 0)
        {
            if (malhaDeColisaoPrecisa != null)
            {
                RaycastHit hit;
                if (malhaDeColisaoPrecisa.Raycast(raio, out hit, 1000f))
                {
                    return node.nomeDaParte;
                }
            }
            return null;
        }

        if (node.children != null)
        {
            foreach (var filho in node.children)
            {
                string resultadoFilho = VerificarNode(filho, raio);
                if (resultadoFilho != null)
                {
                    return resultadoFilho;
                }
            }
        }

        if (malhaDeColisaoPrecisa != null)
        {
             RaycastHit hit;
             if(malhaDeColisaoPrecisa.Raycast(raio, out hit, 1000f))
             {
                 return "Torso/Geral"; 
             }
        }

        return null;
    }

    void OnDrawGizmos()
    {
        if (raizNode != null)
        {

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(raizNode.bounds.center, raizNode.bounds.size);

            Gizmos.color = Color.green;
            if (raizNode.children != null)
            {
                foreach (var filho in raizNode.children)
                {
                    DesenharNodeRecursivo(filho);
                }
            }
        }
    }

    void DesenharNodeRecursivo(BVHNode node)
    {
        if (node == null) return;
        
        Gizmos.DrawWireCube(node.bounds.center, node.bounds.size);

        if (node.children != null)
            foreach (var f in node.children) DesenharNodeRecursivo(f);
    }
}