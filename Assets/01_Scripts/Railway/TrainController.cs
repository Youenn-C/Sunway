using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Contrôleur de train capable de suivre des splines complexes et de passer d'un tronçon à l'autre.
/// </summary>
public class TrainController : MonoBehaviour
{
    [Header("Configuration")]
    public float speed = 10f; // Vitesse en unités/seconde
    public float stopDistance = 0.1f; // Distance pour considérer qu'on est arrivé à une gare

    [Header("État Actuel")]
    // Le tronçon de rail actuel sur lequel le train se trouve.
    private SplineRail currentRail;
    
    // Distance parcourue sur le tronçon actuel (de 0 à Length).
    private float currentDistanceOnRail = 0f;
    
    // État du train
    private bool isMoving = false;
    private Station currentStation = null; // Si le train est à quai

    // Référence au prochain tronçon si on traverse une gare sans s'arrêter
    private SplineRail nextRailTarget = null;

    void Update()
    {
        if (!isMoving) return;

        MoveTrain();
    }

    /// <summary>
    /// Déplace le train d'une frame.
    /// Gère l'avancement sur la spline actuelle et la transition vers le suivant.
    /// </summary>
    void MoveTrain()
    {
        if (currentRail == null)
        {
            StopTrain();
            return;
        }

        // 1. Avancer la distance
        float distanceToMove = speed * Time.deltaTime;
        currentDistanceOnRail += distanceToMove;

        float railLength = currentRail.GetLength();

        // 2. Vérifier si on atteint la fin du tronçon
        if (currentDistanceOnRail >= railLength)
        {
            // On est arrivé au bout du rail actuel.
            // On regarde s'il y a une connexion (une gare ou un autre rail).
            HandleRailEndReached();
        }
        else
        {
            // 3. Mise à jour de la position sur la spline actuelle
            UpdatePositionOnCurrentRail();
        }
    }

    /// <summary>
    /// Met à jour la position et la rotation du GameObject du train selon la spline.
    /// </summary>
    void UpdatePositionOnCurrentRail()
    {
        currentRail.EvaluateAtDistance(currentDistanceOnRail, out Vector3 pos, out Vector3 tangent);
        
        transform.position = pos;
        
        // Rotation : Le train regarde dans la direction de la tangente.
        // Pour un meilleur rendu (banking dans les virages), on pourrait utiliser SplineUtility.GetFrame.
        transform.rotation = Quaternion.LookRotation(tangent, Vector3.up); 
    }

    /// <summary>
    /// Logique complexe de gestion de la fin d'un tronçon.
    /// Gère les arrêts en gare, les changements de voie, ou la fin du parcours.
    /// </summary>
    void HandleRailEndReached()
    {
        // On récupère la connexion à la fin de ce rail
        IRailConnectable connection = currentRail.endConnection;

        if (connection == null)
        {
            // Cul-de-sac : On s'arrête ou on fait demi-tour (à définir).
            Debug.Log("Fin de ligne atteinte.");
            StopTrain();
            return;
        }

        // Cas 1 : La connexion est une Gare
        if (connection is Station station)
        {
            currentStation = station;
            
            // Décision : Est-ce qu'on s'arrête ou on traverse ?
            // Pour l'exemple, on s'arrête toujours en gare.
            Debug.Log("Arrivée en gare : " + station.name);
            StopTrain();
            
            // Ici, vous pourriez déclencher un événement : "Passagers débarquent", "Chargement", etc.
            // Et planifier un départ futur vers un autre rail de la gare.
            // Exemple : StartCoroutine(DepartAfterDelay(station));
        }
        // Cas 2 : La connexion est un autre Rail (connexion directe rare, souvent via gare)
        else if (connection is SplineRail nextRail)
        {
            SwitchToRail(nextRail, 0f); // Passe au rail suivant, distance 0
        }
    }

    /// <summary>
    /// Fait passer le train d'un tronçon à un autre.
    /// </summary>
    void SwitchToRail(SplineRail newRail, float startDistance)
    {
        currentRail = newRail;
        currentDistanceOnRail = startDistance;
        currentStation = null; // On n'est plus à quai
        
        // Mise à jour immédiate pour éviter un saut visuel
        UpdatePositionOnCurrentRail();
    }

    /// <summary>
    /// Démarre le train sur un tronçon spécifique.
    /// </summary>
    public void StartJourney(SplineRail startRail, float startDistance = 0f)
    {
        currentRail = startRail;
        currentDistanceOnRail = startDistance;
        isMoving = true;
    }

    public void StopTrain()
    {
        isMoving = false;
    }
    
    /// <summary>
    /// Méthode utilitaire pour demander à la gare quel rail prendre ensuite.
    /// À appeler quand le train est à l'arrêt dans une gare.
    /// </summary>
    public void ChooseNextRailFromStation(Station station, SplineRail chosenRail)
    {
        if (chosenRail == null) return;
        
        // On vérifie que ce rail est bien connecté à la gare
        // Et on détermine si on entre par le début ou la fin du rail pour initialiser la distance correctement.
        
        bool isStartConnected = (chosenRail.startConnection == station);
        
        if (isStartConnected)
        {
            SwitchToRail(chosenRail, 0f); // On part du début
        }
        else
        {
            // Si connecté à la fin, on doit parcourir le rail à l'envers ou le système doit gérer l'inversion
            // Pour simplifier ici, on suppose que les rails sont orientés "Sortie de gare" -> "Vers ailleurs"
            // Si vous voulez faire des allers-retours, il faudra une logique d'inversion de spline.
            Debug.LogWarning("Connexion à la fin du rail détectée. Logique d'inversion à implémenter si nécessaire.");
            SwitchToRail(chosenRail, 0f); 
        }
        
        isMoving = true;
    }
}