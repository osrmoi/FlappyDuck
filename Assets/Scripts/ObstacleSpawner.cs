using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Prefabs de Obstáculos")]
    [Tooltip("Arrastra aquí liana y rama (pivote Top)")]
    public GameObject[] obstaculosSuperiores;

    [Tooltip("Arrastra aquí rocas, tronco y junco (pivote Bottom)")]
    public GameObject[] obstaculosInferiores;

    [Header("Estructura del Nivel (4 rondas x 5 = 20)")]
    public int cantidadRondas = 4;

    [Header("Separación entre Bordes (Unidades de Unity)")]
    [Tooltip("Espacio libre mínimo garantizado entre el borde derecho de un obstáculo y el izquierdo del siguiente")]
    public float separacionMinima = 4.5f;

    [Tooltip("Espacio libre máximo entre obstáculos")]
    public float separacionMaxima = 7f;

    [Tooltip("Sincroniza la velocidad de todos los obstáculos con el fondo")]
    public float velocidadObstaculos = 2f;

    [Header("Posición y Variación de Altura")]
    [Tooltip("Coordenada X fuera del borde derecho de la cámara (16:9 termina en 9.6)")]
    public float posicionX = 13.5f;

    public float bordeSuperior = 5.4f;
    public float bordeInferior = -5.4f;

    [Tooltip("Desplaza ligeramente hacia afuera del borde para variar el hueco sin que floten")]
    [Range(0f, 1.5f)]
    public float variacionAltura = 0.6f;

    private List<GameObject> colaObstaculos = new List<GameObject>();
    private int indiceActual = 0;
    private float temporizador = 1.2f;
    private bool generacionCompleta = false;

    void Start()
    {
        PrepararSecuenciaDeNivel();
    }

    void Update()
    {
        if (generacionCompleta) return;
        if (GameManager.instancia != null && GameManager.instancia.juegoTerminado) return;

        temporizador -= Time.deltaTime;

        if (temporizador <= 0f)
        {
            GenerarSiguienteObstaculo();
        }
    }

    void PrepararSecuenciaDeNivel()
    {
        colaObstaculos.Clear();
        int inferioresConsecutivos = 0;

        for (int r = 0; r < cantidadRondas; r++)
        {
            List<GameObject> bloqueRonda = new List<GameObject>();

            if (obstaculosSuperiores != null)
            {
                foreach (GameObject obs in obstaculosSuperiores)
                {
                    if (obs != null) bloqueRonda.Add(obs);
                }
            }

            if (obstaculosInferiores != null)
            {
                foreach (GameObject obs in obstaculosInferiores)
                {
                    if (obs != null) bloqueRonda.Add(obs);
                }
            }

            // Reintenta mezclar hasta evitar 3 obstáculos inferiores seguidos
            for (int intento = 0; intento < 50; intento++)
            {
                BarajarLista(bloqueRonda);
                if (EsSecuenciaEquilibrada(bloqueRonda, inferioresConsecutivos))
                {
                    break;
                }
            }

            // Actualiza el conteo arrastrado para el inicio de la siguiente ronda
            foreach (GameObject obs in bloqueRonda)
            {
                if (EsObstaculoSuperior(obs))
                    inferioresConsecutivos = 0;
                else
                    inferioresConsecutivos++;
            }

            colaObstaculos.AddRange(bloqueRonda);
        }
    }

    void BarajarLista(List<GameObject> lista)
    {
        for (int i = 0; i < lista.Count; i++)
        {
            GameObject temp = lista[i];
            int indiceAleatorio = Random.Range(i, lista.Count);
            lista[i] = lista[indiceAleatorio];
            lista[indiceAleatorio] = temp;
        }
    }

    bool EsSecuenciaEquilibrada(List<GameObject> lista, int rachaInicial)
    {
        int rachaInferiores = rachaInicial;
        foreach (GameObject obs in lista)
        {
            if (EsObstaculoSuperior(obs))
            {
                rachaInferiores = 0;
            }
            else
            {
                rachaInferiores++;
                if (rachaInferiores > 2) return false;
            }
        }
        return true;
    }

    bool EsObstaculoSuperior(GameObject prefab)
    {
        return System.Array.Exists(obstaculosSuperiores, item => item == prefab);
    }

    float ObtenerAnchoPrefab(GameObject prefab)
    {
        SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            return sr.sprite.bounds.size.x * Mathf.Abs(prefab.transform.localScale.x);
        }
        return 2f;
    }

    void GenerarSiguienteObstaculo()
    {
        if (indiceActual >= colaObstaculos.Count)
        {
            generacionCompleta = true;
            return;
        }

        GameObject prefabElegido = colaObstaculos[indiceActual];
        indiceActual++;

        bool esSuperior = EsObstaculoSuperior(prefabElegido);

        float posY = esSuperior
            ? bordeSuperior + Random.Range(0f, variacionAltura)
            : bordeInferior - Random.Range(0f, variacionAltura);

        GameObject nuevoObstaculo = Instantiate(prefabElegido, new Vector3(posicionX, posY, 0f), Quaternion.identity);

        MoverObstaculo movimiento = nuevoObstaculo.GetComponent<MoverObstaculo>();
        if (movimiento != null)
        {
            movimiento.velocidad = velocidadObstaculos;
        }

        if (indiceActual >= colaObstaculos.Count)
        {
            generacionCompleta = true;
            return;
        }

        // Calcula el tiempo exacto según el ancho del obstáculo actual + el siguiente + el hueco libre
        float anchoActual = ObtenerAnchoPrefab(prefabElegido);
        float anchoSiguiente = ObtenerAnchoPrefab(colaObstaculos[indiceActual]);
        float huecoLibre = Random.Range(separacionMinima, separacionMaxima);

        float distanciaEntreCentros = (anchoActual * 0.5f) + (anchoSiguiente * 0.5f) + huecoLibre;
        temporizador = distanciaEntreCentros / Mathf.Max(0.1f, velocidadObstaculos);
    }
}