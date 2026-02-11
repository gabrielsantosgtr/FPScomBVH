using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuração do Trabalho")]
    public GameObject[] prefabsInimigos;
    public int quantidade = 50;         
    public float areaDeSpawn = 40f;   

    void Start()
    {
        SpawnarInimigos();
    }

    void SpawnarInimigos()
    {
        // Verificação de segurança fora do loop (economiza processamento)
        if (prefabsInimigos.Length == 0) return;

        for (int i = 0; i < quantidade; i++)
        {
            GameObject prefabSorteado = prefabsInimigos[Random.Range(0, prefabsInimigos.Length)];

            // --- CORREÇÃO AQUI ---
            // Antes: new Vector3(random, random) -> Cria uma parede vertical (X, Y)
            // Agora: new Vector3(random, 0, random) -> Cria um chão plano (X, Z)
            Vector3 posicaoAleatoria = new Vector3(
                Random.Range(-areaDeSpawn, areaDeSpawn), // X (Horizontal)
                0f,                                      // Y (Altura do Chão - mude se seu chão for mais alto)
                Random.Range(-areaDeSpawn, areaDeSpawn)  // Z (Profundidade)
            );

            // Instancia o inimigo
            GameObject novoInimigo = Instantiate(prefabSorteado, posicaoAleatoria, Quaternion.identity);
            
            // Configurações pós-spawn
            novoInimigo.name = "Inimigo_" + i;
            novoInimigo.transform.parent = this.transform; // Organiza na hierarquia
        }
    }
}