using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    [Header("Estado de la Partida")]
    public bool juegoTerminado = false;

    [Header("Referencias para Detener el Escenario")]
    [Tooltip("Arrastra aquí los objetos fondo_0 y fondo_0 (1) para congelar su desplazamiento al perder")]
    public GameObject[] objetosFondo;

    void Awake()
    {
        instancia = this;
    }

    public void ActivarGameOver()
    {
        if (juegoTerminado) return;
        juegoTerminado = true;

        // Detiene el desplazamiento de todos los obstáculos en pantalla
        MoverObstaculo[] obstaculosActivos = FindObjectsByType<MoverObstaculo>(FindObjectsSortMode.None);
        foreach (MoverObstaculo obs in obstaculosActivos)
        {
            obs.enabled = false;
        }

        // Detiene el desplazamiento del fondo infinito
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

        Debug.Log("GAME OVER: El pato chocó o cayó al río.");
    }
}