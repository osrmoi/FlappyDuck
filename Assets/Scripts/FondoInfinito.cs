using UnityEngine;

public class FondoInfinito : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("Velocidad a la que se mueve el fondo hacia la izquierda")]
    public float velocidad = 2f;

    [Tooltip("Arrastra aquí la copia del fondo")]
    public Transform segundoFondo;

    private float anchoFondo;

    void Start()
    {
        // Calcula el ancho exacto de la imagen en unidades de Unity
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        anchoFondo = sr.bounds.size.x;

        // Coloca el segundo fondo exactamente pegado al borde derecho del primero
        if (segundoFondo != null)
        {
            segundoFondo.position = new Vector3(
                transform.position.x + anchoFondo,
                transform.position.y,
                transform.position.z
            );
        }
    }

    void Update()
    {
        float desplazamiento = velocidad * Time.deltaTime;

        // Mueve ambos fondos exactamente la misma distancia para evitar grietas
        transform.Translate(Vector3.left * desplazamiento);

        if (segundoFondo != null)
        {
            segundoFondo.Translate(Vector3.left * desplazamiento);

            // Si el primer fondo salió completamente por la izquierda, lo manda detrás del segundo
            if (transform.position.x <= -anchoFondo)
            {
                transform.position = new Vector3(
                    segundoFondo.position.x + anchoFondo,
                    transform.position.y,
                    transform.position.z
                );
            }

            // Si el segundo fondo salió completamente por la izquierda, lo manda detrás del primero
            if (segundoFondo.position.x <= -anchoFondo)
            {
                segundoFondo.position = new Vector3(
                    transform.position.x + anchoFondo,
                    segundoFondo.position.y,
                    segundoFondo.position.z
                );
            }
        }
    }
}