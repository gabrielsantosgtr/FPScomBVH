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
        for (int i = 0; i < quantidade; i++)
        {
            if (prefabsInimigos.Length > 0)
            {
                GameObject prefabSorteado = prefabsInimigos[Random.Range(0, prefabsInimigos.Length)];

                Vector3 posicaoAleatoria = new Vector3(
                    Random.Range(-areaDeSpawn, areaDeSpawn),

                    Random.Range(-areaDeSpawn, areaDeSpawn)
                );

                GameObject novoInimigo = Instantiate(prefabSorteado, posicaoAleatoria, Quaternion.identity);
                novoInimigo.name = "Inimigo_" + i;
                
                novoInimigo.transform.parent = this.transform;
            }
        }
    }
}