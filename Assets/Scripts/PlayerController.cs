using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Físicas de Vuelo")]
    [Tooltip("Impulso vertical al presionar clic izquierdo o barra espaciadora")]
    public float fuerzaSalto = 8f;

    [Tooltip("Tope superior (borde de cámara en 5.4)")]
    public float limiteSuperior = 5f;

    [Tooltip("Tope inferior (borde de cámara en -5.4)")]
    public float limiteInferior = -5f;

    [Header("Inclinación Visual")]
    public float anguloSubida = 25f;
    public float anguloCaida = -45f;
    public float velocidadInclinacion = 3f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
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
        else if (transform.position.y < limiteInferior)
        {
            transform.position = new Vector3(transform.position.x, limiteInferior, transform.position.z);
            if (rb.linearVelocity.y < 0f)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    void ActualizarInclinacion()
    {
        float anguloObjetivo = rb.linearVelocity.y >= 0f ? anguloSubida : anguloCaida;
        Quaternion rotacionObjetivo = Quaternion.Euler(0f, 0f, anguloObjetivo);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotacionObjetivo, velocidadInclinacion * Time.deltaTime);
    }
}