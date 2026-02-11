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

    public LayerMask layerMascaraInimigo; 

    private EnemyBVH[] todosInimigos;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        todosInimigos = FindObjectsOfType<EnemyBVH>();
        
        if(textoStatus) textoStatus.text = "Pronto para teste.";
        if(lineRenderer) lineRenderer.enabled = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray raio = cam.ScreenPointToRay(Input.mousePosition);
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                Atirar(raio);
            }
        }
    }

    void Atirar(Ray raio)
    {
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
            RaycastHit hitInicial;
            if (Physics.Raycast(raio, out hitInicial, 1000f, layerMascaraInimigo))
            {
                EnemyBVH candidato = hitInicial.collider.GetComponentInParent<EnemyBVH>();
                
                if (candidato != null)
                {
                    string parteAtingida = candidato.TestarImpacto(raio);
                    if (!string.IsNullOrEmpty(parteAtingida))
                    {
                        acertou = true;
                        nomeDoAlvo = $"{candidato.name} ({parteAtingida})";
                    }
                }
            }
        }
        else
        {
            foreach(var inimigo in todosInimigos)
            {
                if (inimigo == null) continue;
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