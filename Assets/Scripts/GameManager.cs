using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    [Header("Estado de la Partida")]
    public bool juegoTerminado = false;
    private bool juegoPausado = false;

    [Header("Referencias de Escena")]
    public PlayerController jugador;
    public GameObject[] objetosFondo;

    [Header("Sprites de Patitos en Escena")]
    public GameObject patitosTristes;
    public GameObject patitosFelices;
    public GameObject patitosReencuentro;

    [Header("Ajustes de Cinemática")]
    [Tooltip("Desplazamiento a la izquierda de patitosFelices donde aterrizará el pato")]
    public Vector2 offsetAterrizaje = new Vector2(-2.2f, 0.2f);
    public float velocidadEntradaPatitos = 3.5f;

    [Header("Interfaz de Usuario (Paneles y Botones)")]
    public GameObject botonPausaHUD;
    public GameObject panelPausa;
    public GameObject panelDerrota;
    public GameObject panelVictoria;

    private Vector3 posicionFinalPatitosFelices;
    private bool moviendoPatitosFelices = false;

    void Awake()
    {
        instancia = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        if (patitosFelices != null)
        {
            posicionFinalPatitosFelices = patitosFelices.transform.position;
        }
    }

    void Update()
    {
        if (moviendoPatitosFelices && patitosFelices != null)
        {
            patitosFelices.transform.position = Vector3.MoveTowards(
                patitosFelices.transform.position,
                posicionFinalPatitosFelices,
                velocidadEntradaPatitos * Time.deltaTime
            );

            if (Vector3.Distance(patitosFelices.transform.position, posicionFinalPatitosFelices) < 0.05f)
            {
                moviendoPatitosFelices = false;
            }
        }
    }

    public float ObtenerPosicionXJugador()
    {
        return jugador != null ? jugador.transform.position.x : -4f;
    }

    // --- DERROTA (Opción C) ---
    public void ActivarGameOver()
    {
        if (juegoTerminado) return;
        juegoTerminado = true;

        DetenerEscenarioYObstaculos();
        if (botonPausaHUD != null) botonPausaHUD.SetActive(false);

        StartCoroutine(SecuenciaDerrota());
    }

    IEnumerator SecuenciaDerrota()
    {
        // 1. Deja ver el impacto con el sprite del pato muerto
        yield return new WaitForSeconds(1.2f);

        // 2. Oculta obstáculos para que no tapen el río y muestra a los patitos tristes
        OcultarObstaculosActivos();
        if (patitosTristes != null) patitosTristes.SetActive(true);

        yield return new WaitForSeconds(0.8f);

        // 3. Muestra el cartel de derrota y el botón Replay
        if (panelDerrota != null) panelDerrota.SetActive(true);
    }

    // --- VICTORIA (Opción C) ---
    public void IniciarSecuenciaVictoria()
    {
        if (juegoTerminado) return;
        juegoTerminado = true;

        if (botonPausaHUD != null) botonPausaHUD.SetActive(false);

        // Coloca a los patitos felices fuera del borde derecho y los desliza a su posición en el río
        if (patitosFelices != null)
        {
            patitosFelices.transform.position = new Vector3(13.5f, posicionFinalPatitosFelices.y, posicionFinalPatitosFelices.z);
            patitosFelices.SetActive(true);
            moviendoPatitosFelices = true;
        }

        // El pato planea hacia el agua junto a sus crías
        if (jugador != null)
        {
            Vector3 destino = posicionFinalPatitosFelices + (Vector3)offsetAterrizaje;
            jugador.IniciarAterrizajeVictoria(destino);
        }
    }

    public void CompletarReencuentroVictoria()
    {
        DetenerEscenarioYObstaculos();
        moviendoPatitosFelices = false;

        // Oculta al pato y a los patitos separados para mostrar la imagen del abrazo
        if (jugador != null) jugador.gameObject.SetActive(false);
        if (patitosFelices != null) patitosFelices.SetActive(false);
        if (patitosReencuentro != null) patitosReencuentro.SetActive(true);

        StartCoroutine(MostrarCartelVictoriaConRetraso());
    }

    IEnumerator MostrarCartelVictoriaConRetraso()
    {
        yield return new WaitForSeconds(1.5f);
        if (panelVictoria != null) panelVictoria.SetActive(true);
    }

    // --- UTILIDADES Y BOTONES UI ---
    void DetenerEscenarioYObstaculos()
    {
        MoverObstaculo[] obstaculosActivos = FindObjectsByType<MoverObstaculo>(FindObjectsSortMode.None);
        foreach (MoverObstaculo obs in obstaculosActivos)
        {
            obs.enabled = false;
        }

        if (objetosFondo != null)
        {
            foreach (GameObject fondo in objetosFondo)
            {
                if (fondo != null)
                {
                    foreach (MonoBehaviour script in fondo.GetComponents<MonoBehaviour>())
                    {
                        script.enabled = false;
                    }
                }
            }
        }
    }

    void OcultarObstaculosActivos()
    {
        MoverObstaculo[] obstaculosActivos = FindObjectsByType<MoverObstaculo>(FindObjectsSortMode.None);
        foreach (MoverObstaculo obs in obstaculosActivos)
        {
            obs.gameObject.SetActive(false);
        }
    }

    public void AlternarPausa()
    {
        if (juegoTerminado) return;

        juegoPausado = !juegoPausado;
        Time.timeScale = juegoPausado ? 0f : 1f;

        if (panelPausa != null) panelPausa.SetActive(juegoPausado);
        if (botonPausaHUD != null) botonPausaHUD.SetActive(!juegoPausado);
    }

    public void ReiniciarPartida()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}