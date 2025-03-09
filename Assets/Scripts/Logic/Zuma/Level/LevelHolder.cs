using UnityEngine;
using PathCreation;
using PathCreation.Examples;

namespace Logic.Zuma.Level
{
    public class LevelHolder : MonoBehaviour
    {
        [field: SerializeField] public Transform SpawnPositionPlayer { get; private set; }
        [field: SerializeField] public PathCreator PathCreator { get; private set; }
        [field: SerializeField] public RoadMeshCreator RoadMeshCreator { get; private set; }
        [field: SerializeField] public ParticleSystemHolder ParticleSystemHolder { get; private set; }
        [field: SerializeField] public LevelStart LevelStart { get; private set; }
        [field: SerializeField] public LevelEnd LevelEnd { get; private set; }
    }
}