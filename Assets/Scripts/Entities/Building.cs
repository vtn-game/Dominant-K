using UnityEngine;

namespace DominantK.Entities
{
    public class Building : MonoBehaviour
    {
        [SerializeField] private int population = 10;
        [SerializeField] private Vector2Int gridPosition;

        public int Population => population;
        public Vector2Int GridPosition => gridPosition;

        public void Initialize(Vector2Int position)
        {
            gridPosition = position;
            population = Random.Range(5, 20);
        }

        public void OnDestroyed()
        {
            // Trigger visual effects, population changes, etc.
            Debug.Log($"Building at {gridPosition} destroyed!");
        }
    }
}
