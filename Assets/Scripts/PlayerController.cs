using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [Header("Físicas de Vuelo")]
    public float fuerzaSalto = 8f;
    public float limiteSuperior = 5f;
    public float limiteInferior = -4.5f;

    [Header("Inclinación Visual")]
    public float anguloSubida = 25f;
    public float anguloCaida = -45f;
    public float velocidadInclinacion = 3f;

    [Header("Sprites de Estado")]
    [Tooltip("Arrastra aquí el sprite del pato muerto")]
    public Sprite spritePatoMuerto;

    [Tooltip("Arrastra aquí el sprite pato-planeando para el aterrizaje de victoria")]
    public Sprite spritePatoPlaneando;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Collider2D col;

    private bool estaMuerto = false;
    private bool enSecuenciaVictoria = false;
    private bool aterrizajeCompletado = false;
    private Vector3 destinoAterrizaje;
    private float velocidadAterrizaje = 4.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (estaMuerto || Time.timeScale == 0f) return;

        if (enSecuenciaVictoria)
        {
            EjecutarAterrizaje();
            return;
        }

        // Evita que hacer clic sobre el botón de Pausa dispare un salto
        bool clicSobreUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        bool saltoPresionado = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
                               (!clicSobreUI && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        if (saltoPresionado)
        {
            Saltar();
        }

        LimitarAltura();
        ActualizarInclinacion();
    }

    void Saltar()
    {
        rb.linearVelocity = new Vector2(0f, fuerzaSalto);
        
        if (GameManager.instancia != null)
        {
            GameManager.instancia.ReproducirSalto();
        }
    }

    void LimitarAltura()
    {
        if (transform.position.y > limiteSuperior)
        {
            transform.position = new Vector3(transform.position.x, limiteSuperior, transform.position.z);
            if (rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
        else if (transform.position.y <= limiteInferior)
        {
            transform.position = new Vector3(transform.position.x, limiteInferior, transform.position.z);
            Morir();
        }
    }

    void ActualizarInclinacion()
    {
        float anguloObjetivo = rb.linearVelocity.y >= 0f ? anguloSubida : anguloCaida;
        Quaternion rotacionObjetivo = Quaternion.Euler(0f, 0f, anguloObjetivo);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotacionObjetivo, velocidadInclinacion * Time.deltaTime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (estaMuerto || enSecuenciaVictoria) return;

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Morir();
        }
    }

    public void Morir()
    {
        if (estaMuerto || enSecuenciaVictoria) return;
        estaMuerto = true;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        transform.rotation = Quaternion.identity;

        if (anim != null) anim.enabled = false;
        if (sr != null && spritePatoMuerto != null) sr.sprite = spritePatoMuerto;

        if (GameManager.instancia != null)
        {
            GameManager.instancia.ActivarGameOver();
        }
    }

    public void IniciarAterrizajeVictoria(Vector3 puntoDestino)
    {
        enSecuenciaVictoria = true;
        destinoAterrizaje = puntoDestino;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        if (col != null) col.enabled = false;

        if (anim != null) anim.enabled = false;
        if (sr != null && spritePatoPlaneando != null) sr.sprite = spritePatoPlaneando;
    }

    void EjecutarAterrizaje()
    {
        if (aterrizajeCompletado) return;

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, 6f * Time.deltaTime);
        transform.position = Vector3.MoveTowards(transform.position, destinoAterrizaje, velocidadAterrizaje * Time.deltaTime);

        if (Vector3.Distance(transform.position, destinoAterrizaje) < 0.05f)
        {
            aterrizajeCompletado = true;
            if (GameManager.instancia != null)
            {
                GameManager.instancia.CompletarReencuentroVictoria();
            }
        }
    }
}