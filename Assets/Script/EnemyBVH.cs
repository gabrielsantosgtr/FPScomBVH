using UnityEngine;
using System.Collections.Generic;

public class EnemyBVH : MonoBehaviour
{
    private class BVHNode
    {
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
        // Garante que a malha precisa está ligada e na layer correta se necessário
        if (malhaDeColisaoPrecisa != null) malhaDeColisaoPrecisa.enabled = true;

        if (raizDaBVH != null)
        {
            raizNode = ConstruirArvore(raizDaBVH.transform);
        }
    }

    private BVHNode ConstruirArvore(Transform atual)
    {
        BVHNode node = new BVHNode();

        // 1. Tenta pegar o BoxCollider (prioridade para a estrutura BVH)
        BoxCollider box = atual.GetComponent<BoxCollider>();
        
        // 2. Se não tiver Box, mas for o objeto da Malha, usa a bounding box da malha!
        Collider colisorGenerico = atual.GetComponent<Collider>();
        bool ehObjetoDaMalha = (colisorGenerico == malhaDeColisaoPrecisa);

        if (box != null)
        {
            node.bounds = box.bounds;
            // Só desliga se não for a raiz principal e não for a malha visual
            if (atual.gameObject != this.gameObject && atual.gameObject != raizDaBVH && !ehObjetoDaMalha)
            {
                box.enabled = false; 
            }
        }
        else if (ehObjetoDaMalha)
        {
            // CORREÇÃO: Se for a malha e não tiver box, usa o tamanho da malha para o nó!
            // Isso evita que o Bounds fique com tamanho zero e o raio falhe.
            node.bounds = colisorGenerico.bounds;
        }
        else
        {
            // Se não é nada, cria um ponto vazio (provavelmente um agrupador vazio)
            node.bounds = new Bounds(atual.position, Vector3.zero);
        }

        // Verifica se este nó carrega a referência da malha precisa
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

    public bool TestarImpacto(Ray raio)
    {
        if (raizNode == null) return false;
        return VerificarNode(raizNode, raio);
    }

    // Versão "Ligação Direta" - Resolve o problema das caixas vazias
    private bool VerificarNode(BVHNode node, Ray raio)
    {
        // 1. O raio bateu na caixa matemática deste nó?
        if (!node.Intersecta(raio))
        {
            return false; // Se não bateu na caixa, nem perde tempo
        }

        // 2. Se bateu na caixa, verifica se é uma folha (não tem filhos)
        // Se for uma folha (ex: Box da Cabeça), AGORA testamos a malha pesada.
        if (node.children == null || node.children.Length == 0)
        {
            // O Truque: Mesmo que a caixa não tenha a malha nela, 
            // nós forçamos o teste na "malhaDeColisaoPrecisa" global do script.
            if (malhaDeColisaoPrecisa != null)
            {
                RaycastHit hit;
                return malhaDeColisaoPrecisa.Raycast(raio, out hit, 1000f);
            }
        }

        // 3. Se não é folha (ainda tem caixas menores dentro), continua descendo
        if (node.children != null)
        {
            foreach (var filho in node.children)
            {
                if (VerificarNode(filho, raio))
                {
                    return true;
                }
            }
        }
        
        // Proteção extra: Se chegou aqui, bateu na caixa mas não nos filhos.
        // Tenta testar a malha uma última vez caso a estrutura seja rasa.
        if (malhaDeColisaoPrecisa != null)
        {
             RaycastHit hit;
             return malhaDeColisaoPrecisa.Raycast(raio, out hit, 1000f);
        }

        return false;
    }

    // Gizmos para debug visual
    void OnDrawGizmos()
    {
        if (raizNode != null)
        {
            Gizmos.color = Color.green;
            DesenharNodeRecursivo(raizNode);
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