using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuração do Trabalho")]
    public GameObject[] prefabsInimigos; // Arraste seus 3 tipos de inimigos aqui
    public int quantidade = 50;          // Requisito: Mínimo 50
    public float areaDeSpawn = 40f;      // Tamanho da área onde eles vão aparecer

    void Start()
    {
        SpawnarInimigos();
    }

    void SpawnarInimigos()
    {
        for (int i = 0; i < quantidade; i++)
        {
            // 1. Escolhe um prefab aleatório (entre os 3 tipos exigidos)
            if (prefabsInimigos.Length > 0)
            {
                GameObject prefabSorteado = prefabsInimigos[Random.Range(0, prefabsInimigos.Length)];

                // 2. Gera uma posição aleatória no chão (X e Z)
                Vector3 posicaoAleatoria = new Vector3(
                    Random.Range(-areaDeSpawn, areaDeSpawn),
                    0, // Assume que o chão está no Y=0
                    Random.Range(-areaDeSpawn, areaDeSpawn)
                );

                // 3. Cria o inimigo e garante que o nome não fica duplicado
                GameObject novoInimigo = Instantiate(prefabSorteado, posicaoAleatoria, Quaternion.identity);
                novoInimigo.name = "Inimigo_" + i;
                
                // Organiza na hierarquia para não virar bagunça
                novoInimigo.transform.parent = this.transform;
            }
        }
    }
}