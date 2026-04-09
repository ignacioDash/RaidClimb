using Constants;
using UnityEngine;

namespace Units
{
    public class UnitVisualsController : MonoBehaviour
    {
        [SerializeField] private SkinnedMeshRenderer[] unitMeshes;

        [Header("Settings")]
        [SerializeField] private Material playerMaterial;
        [SerializeField] private Material opponentMaterial;

        public void Init(string playerId)
        {
            var isPlayer = playerId == Keys.PLAYER_ID;

            foreach (var mesh in unitMeshes)
            {
                mesh.sharedMaterial = isPlayer ? playerMaterial : opponentMaterial;
            }
        }
    }
}