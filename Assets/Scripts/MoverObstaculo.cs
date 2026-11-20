using UnityEngine;

public class MoverObstaculo : MonoBehaviour
{
    [Tooltip("Debe coincidir con la velocidad del FondoInfinito")]
    public float velocidad = 2f;

    [Tooltip("Posición X fuera del borde izquierdo donde se destruye el objeto")]
    public float limiteIzquierdo = -15f;

    [HideInInspector]
    public bool esUltimoObstaculo = false;

    private bool yaSuperado = false;

    void Update()
    {
        transform.Translate(Vector3.left * velocidad * Time.deltaTime);

        if (esUltimoObstaculo && !yaSuperado && GameManager.instancia != null && !GameManager.instancia.juegoTerminado)
        {
            float posXJugador = GameManager.instancia.ObtenerPosicionXJugador();
            if (transform.position.x < posXJugador)
            {
                yaSuperado = true;
                GameManager.instancia.IniciarSecuenciaVictoria();
            }
        }

        if (transform.position.x < limiteIzquierdo)
        {
            Destroy(gameObject);
        }
    }
}