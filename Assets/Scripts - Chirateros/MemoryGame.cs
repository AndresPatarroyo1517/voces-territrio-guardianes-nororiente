using UnityEngine;
using UnityEngine.InputSystem; // ✅ Nuevo Input System
using System.Collections;
using System.Collections.Generic;

public class Chitarreros : MonoBehaviour
{
    [Header("Configuración del Tapete y Cámara")]
    public Transform vrRig;                  // XR Origin o VR Character
    public Transform gameViewPoint;          // Punto frente al minijuego
    public Transform teleportTarget;         // Dónde se teletransporta al completar 3 aciertos
    public float activationDistance = 2.0f;  // Distancia para activar el juego en VR

    [Header("Peces Interactivos")]
    public List<GameObject> fishObjects;

    [Header("Configuración del Juego")]
    public int currentLevel = 1;
    public float highlightDuration = 1f;

    [Header("Efectos de Sonido")]
    public AudioSource successSound;
    public AudioSource failSound;

    [Header("Color del minijuego")]
    public Color chosenHighlightColor = Color.cyan;

    private List<int> sequence = new List<int>();
    private int playerIndex = 0;
    private bool playerTurn = false;
    private bool gameActive = false;

    void Start()
    
        Debug.Log("Script Chitarreros inicializado en " + gameObject.name);
    }

    void Update()
    {
        // 🔹 Activa el juego si el jugador está cerca (ideal para VR)
        if (!gameActive && vrRig != null)
        {
            float distance = Vector3.Distance(vrRig.position, transform.position);
            if (distance <= activationDistance)
            {
                Debug.Log("Jugador detectado cerca, activando juego automáticamente (VR).");
                ActivateGame();
            }
        }

        // 🔹Modo de prueba manual con el nuevo Input System
        if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame && !gameActive)
        {
            Debug.Log(" Activación manual con tecla G (nuevo Input System).");
            ActivateGame();
        }
    }

    // ===================================================
    // =============== INICIO DEL MINIJUEGO ==============
    // ===================================================

    public void ActivateGame()
    {
        if (gameActive) return;
        gameActive = true;

        // Mover al jugador frente al minijuego
        if (vrRig && gameViewPoint)
        {
            vrRig.position = gameViewPoint.position;
            vrRig.rotation = gameViewPoint.rotation;
        }

        SetAllFishToNeutral();
        StartCoroutine(IntroSequence());
    }

    void SetAllFishToNeutral()
    {
        foreach (var fish in fishObjects)
        {
            Renderer rend = fish.GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = Color.white;
        }
    }

    IEnumerator IntroSequence()
    {
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(StartChallenge());
    }

    IEnumerator StartChallenge()
    {
        yield return new WaitForSeconds(1f);

        GenerateSequence();
        yield return PlaySequence();

        playerTurn = true;
        playerIndex = 0;
        Debug.Log(" Tu turno: toca los peces en el orden correcto.");
    }

    // ===================================================
    // =============== SECUENCIA =========================
    // ===================================================

    void GenerateSequence()
    {
        sequence.Clear();
        for (int i = 0; i < currentLevel + 2; i++)
        {
            int fishIndex = Random.Range(0, fishObjects.Count);
            sequence.Add(fishIndex);
        }
    }

    IEnumerator PlaySequence()
    {
        playerTurn = false;
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < sequence.Count; i++)
        {
            int index = sequence[i];
            yield return HighlightFish(index, chosenHighlightColor);
            yield return new WaitForSeconds(0.5f);
        }

        playerTurn = true;
    }

    IEnumerator HighlightFish(int index, Color color)
    {
        Renderer rend = fishObjects[index].GetComponent<Renderer>();
        if (rend != null)
        {
            Color original = rend.material.color;
            rend.material.color = color;
            yield return new WaitForSeconds(highlightDuration);
            rend.material.color = original;
        }
    }

    // ===================================================
    // =============== INTERACCIÓN DEL JUGADOR ===========
    // ===================================================

    public void OnFishTouched(GameObject fish)
    {
        if (!playerTurn) return;

        int fishIndex = fishObjects.IndexOf(fish);
        if (fishIndex == -1) return;

        int expectedFish = sequence[playerIndex];
        Renderer rend = fish.GetComponent<Renderer>();

        if (fishIndex == expectedFish)
        {
            if (rend != null) rend.material.color = Color.green;

            playerIndex++;
            if (playerIndex >= sequence.Count)
            {
                if (successSound) successSound.Play();
                Debug.Log(" Secuencia completa correctamente.");

                playerTurn = false;
                currentLevel++;

                if (currentLevel > 3)
                {
                    StartCoroutine(EndGame());
                }
                else
                {
                    StartCoroutine(StartChallenge());
                }
            }
        }
        else
        {
            if (rend != null) rend.material.color = Color.red;
            if (failSound) failSound.Play();

            Debug.Log(" Error en la secuencia.");
            playerTurn = false;
            StartCoroutine(ResetSequence());
        }
    }

    IEnumerator ResetSequence()
    {
        yield return new WaitForSeconds(2f);
        SetAllFishToNeutral();
        StartCoroutine(StartChallenge());
    }

    // ===================================================
    // =============== FIN DEL MINIJUEGO =================
    // ===================================================

    IEnumerator EndGame()
    {
        yield return new WaitForSeconds(1f);

        if (vrRig && teleportTarget)
        {
            vrRig.position = teleportTarget.position;
            vrRig.rotation = teleportTarget.rotation;
        }

        Debug.Log(" Juego terminado. Jugador teletransportado.");
        gameActive = false;
        currentLevel = 1;
    }


