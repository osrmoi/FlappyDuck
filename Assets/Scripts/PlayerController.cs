using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Estado de Derrota")]
    [Tooltip("Arrastra aquí el sprite del pato muerto")]
    public Sprite spritePatoMuerto;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private bool estaMuerto = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (estaMuerto) return;

        bool saltoPresionado = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
                               (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

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
        if (estaMuerto) return;

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Morir();
        }
    }

    public void Morir()
    {
        if (estaMuerto) return;
        estaMuerto = true;

        // Congela físicas y endereza al personaje en el punto de impacto
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        transform.rotation = Quaternion.identity;

        // Detiene la animación de aleteo para que no sobrescriba el sprite muerto
        if (anim != null)
        {
            anim.enabled = false;
        }

        if (sr != null && spritePatoMuerto != null)
        {
            sr.sprite = spritePatoMuerto;
        }

        if (GameManager.instancia != null)
        {
            GameManager.instancia.ActivarGameOver();
        }
    }
}