using UnityEngine;

public class MoverObstaculo : MonoBehaviour
{
    [Tooltip("Debe coincidir con la velocidad del FondoInfinito")]
    public float velocidad = 2f;

    [Tooltip("Posición X fuera del borde izquierdo donde se destruye el objeto")]
    public float limiteIzquierdo = -15f;

    void Update()
    {
        transform.Translate(Vector3.left * velocidad * Time.deltaTime);

        if (transform.position.x < limiteIzquierdo)
        {
            Destroy(gameObject);
        }
    }
}