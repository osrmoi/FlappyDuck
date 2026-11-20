using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    [Header("Estado de la Partida")]
    public bool juegoTerminado = false;
    private bool juegoPausado = false;
    private int obstaculosSuperados = 0;

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
    public TextMeshProUGUI textoProgreso;
    public int totalObstaculosNivel = 20;
    public GameObject botonPausaHUD;
    public GameObject panelPausa;
    public GameObject panelDerrota;
    public GameObject panelVictoria;

    [Header("Audio - Reproductores")]
    [Tooltip("Arrastra el primer AudioSource (con Loop marcado)")]
    public AudioSource fuenteMusica;
    [Tooltip("Arrastra el segundo AudioSource (sin Loop)")]
    public AudioSource fuenteEfectos;

    [Header("Audio - Clips y Volúmenes")]
    public AudioClip clipMusica;
    [Range(0f, 1f)] public float volumenMusica = 0.35f;

    public AudioClip clipSalto;
    [Range(0f, 1f)] public float volumenSalto = 0.8f;

    public AudioClip clipPunto;
    [Range(0f, 1f)] public float volumenPunto = 0.8f;

    public AudioClip clipImpactoBonk;
    [Range(0f, 1f)] public float volumenBonk = 0.9f;

    public AudioClip clipImpactoMartillo;
    [Range(0f, 1f)] public float volumenMartillo = 0.9f;

    public AudioClip clipPerdiste;
    [Range(0f, 1f)] public float volumenPerdiste = 0.9f;

    public AudioClip clipVictoria;
    [Range(0f, 1f)] public float volumenVictoria = 1f;

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

        ActualizarTextoProgreso();
        IniciarMusicaFondo();
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

    // --- MÉTODOS DE AUDIO ---
    void IniciarMusicaFondo()
    {
        if (fuenteMusica != null && clipMusica != null)
        {
            fuenteMusica.clip = clipMusica;
            fuenteMusica.volume = volumenMusica;
            fuenteMusica.loop = true;
            fuenteMusica.Play();
        }
    }

    void DetenerMusicaFondo()
    {
        if (fuenteMusica != null && fuenteMusica.isPlaying)
        {
            fuenteMusica.Stop();
        }
    }

    void ReproducirSFX(AudioClip clip, float volumen)
    {
        if (fuenteEfectos != null && clip != null && volumen > 0f)
        {
            fuenteEfectos.PlayOneShot(clip, volumen);
        }
    }

    public void ReproducirSalto()
    {
        ReproducirSFX(clipSalto, volumenSalto);
    }

    // --- PROGRESO ---
    public void RegistrarObstaculoSuperado(bool esElUltimo)
    {
        if (juegoTerminado) return;

        obstaculosSuperados++;
        ActualizarTextoProgreso();
        ReproducirSFX(clipPunto, volumenPunto);

        if (esElUltimo || obstaculosSuperados >= totalObstaculosNivel)
        {
            IniciarSecuenciaVictoria();
        }
    }

    void ActualizarTextoProgreso()
    {
        if (textoProgreso != null)
        {
            textoProgreso.text = $"{obstaculosSuperados} / {totalObstaculosNivel}";
        }
    }

    // --- DERROTA ---
    public void ActivarGameOver()
    {
        if (juegoTerminado) return;
        juegoTerminado = true;

        DetenerMusicaFondo();
        ReproducirSFX(clipImpactoBonk, volumenBonk);
        ReproducirSFX(clipImpactoMartillo, volumenMartillo);

        DetenerEscenarioYObstaculos();
        if (botonPausaHUD != null) botonPausaHUD.SetActive(false);

        StartCoroutine(SecuenciaDerrota());
    }

    IEnumerator SecuenciaDerrota()
    {
        yield return new WaitForSeconds(1.2f);

        OcultarObstaculosActivos();
        if (patitosTristes != null) patitosTristes.SetActive(true);
        ReproducirSFX(clipPerdiste, volumenPerdiste);

        yield return new WaitForSeconds(0.8f);

        if (panelDerrota != null) panelDerrota.SetActive(true);
    }

    // --- VICTORIA ---
    public void IniciarSecuenciaVictoria()
    {
        if (juegoTerminado) return;
        juegoTerminado = true;

        if (botonPausaHUD != null) botonPausaHUD.SetActive(false);

        if (patitosFelices != null)
        {
            patitosFelices.transform.position = new Vector3(13.5f, posicionFinalPatitosFelices.y, posicionFinalPatitosFelices.z);
            patitosFelices.SetActive(true);
            moviendoPatitosFelices = true;
        }

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

        if (jugador != null) jugador.gameObject.SetActive(false);
        if (patitosFelices != null) patitosFelices.SetActive(false);
        if (patitosReencuentro != null) patitosReencuentro.SetActive(true);

        DetenerMusicaFondo();
        ReproducirSFX(clipVictoria, volumenVictoria);

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

        if (fuenteMusica != null)
        {
            if (juegoPausado) fuenteMusica.Pause();
            else fuenteMusica.UnPause();
        }

        if (panelPausa != null) panelPausa.SetActive(juegoPausado);
        if (botonPausaHUD != null) botonPausaHUD.SetActive(!juegoPausado);
    }

    public void ReiniciarPartida()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}