using UnityEngine;

namespace Infrastructure
{
  public class CoroutineRunner : MonoBehaviour, ICoroutineRunner
  {
    public void Awake()
    {
      DontDestroyOnLoad(gameObject);
    }
  }
}