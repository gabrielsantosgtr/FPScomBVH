using UnityEngine;
using UnityEngine.UI; 
using System.Diagnostics; 

public class GameManager : MonoBehaviour
{
    [Header("Configurações da Câmera")]
    public Camera cam;

    [Header("Interface de Usuário (UI)")]
    public Toggle toggleBVH;       
    public Text textoPerformance;  
    public Text textoStatus;       

    [Header("Visual do Tiro")]
    public LineRenderer lineRenderer;
    public float duracaoDoRastro = 0.1f;

    [Header("Configuração de Física")]
    // Selecione a layer "Inimigo" aqui no Inspector
    public LayerMask layerMascaraInimigo; 

    // Cache para o modo "Sem BVH" (que precisa da lista)
    private EnemyBVH[] todosInimigos;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Ainda precisamos da lista para o modo "Sem BVH" (Força Bruta)
        todosInimigos = FindObjectsOfType<EnemyBVH>();
        
        if(textoStatus) textoStatus.text = "Pronto para teste.";
        if(lineRenderer) lineRenderer.enabled = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray raio = cam.ScreenPointToRay(Input.mousePosition);
            
            // Verifica se o mouse não está sobre a UI
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                Atirar(raio);
            }
        }
    }

    void Atirar(Ray raio)
    {
        // Atualiza lista se estiver no modo força bruta
        bool usarBVH = toggleBVH != null ? toggleBVH.isOn : true;
        if (!usarBVH) todosInimigos = FindObjectsOfType<EnemyBVH>();

        // Visual
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, raio.origin + new Vector3(0, -0.5f, 0)); 
            lineRenderer.SetPosition(1, raio.origin + raio.direction * 100f);
            StartCoroutine(ApagarRastro());
        }

        Stopwatch sw = new Stopwatch();
        sw.Start();

        bool acertou = false;
        string nomeDoAlvo = "";

        if (usarBVH)
        {
            // --- MODO BVH HÍBRIDO ---
            RaycastHit hitInicial;
            // O raio bate na Layer "Inimigo" (no BoxCollider da Raiz que deixamos ligado agora)
            if (Physics.Raycast(raio, out hitInicial, 1000f, layerMascaraInimigo))
            {
                EnemyBVH candidato = hitInicial.collider.GetComponentInParent<EnemyBVH>();
                
                // Se achou o inimigo, pede pra ele rodar a lógica interna dele
                if (candidato != null && candidato.TestarImpacto(raio))
                {
                    acertou = true;
                    nomeDoAlvo = candidato.name;
                }
            }
        }
        else
        {
            // --- MODO SEM BVH (CORRIGIDO: Problema 1) ---
            foreach(var inimigo in todosInimigos)
            {
                if (inimigo == null) continue;

                // CORREÇÃO: Não usamos mais 'GetComponent<Collider>' genérico.
                // Forçamos o teste EXCLUSIVAMENTE na malha precisa.
                // Se bater na caixa, ele ignora. Só conta se bater na malha.
                if (inimigo.malhaDeColisaoPrecisa != null)
                {
                    RaycastHit hit;
                    if (inimigo.malhaDeColisaoPrecisa.Raycast(raio, out hit, 1000f))
                    {
                        acertou = true;
                        nomeDoAlvo = inimigo.name;
                    }
                }
            }
        }

        sw.Stop();
        
        double tempoMs = sw.Elapsed.TotalMilliseconds;
        string resultadoTexto = acertou ? "ACERTOU: " + nomeDoAlvo : "ERROU";

        if(textoPerformance)
        {
            textoPerformance.text = "Tempo: " + tempoMs.ToString("F4") + " ms";
            // Ajuste o limiar de cor conforme seu PC
            textoPerformance.color = tempoMs > 0.5 ? Color.red : Color.green; 
        }

        if(textoStatus) textoStatus.text = resultadoTexto;

        UnityEngine.Debug.Log("Analise - BVH: " + usarBVH + " | Tempo: " + tempoMs.ToString("F4") + " ms | Status: " + resultadoTexto);
    }

    System.Collections.IEnumerator ApagarRastro()
    {
        yield return new WaitForSeconds(duracaoDoRastro);
        if(lineRenderer != null) lineRenderer.enabled = false;
    }
}