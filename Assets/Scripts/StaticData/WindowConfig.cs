using System;
using UnityEngine;
using Window;

namespace StaticData
{
  [Serializable]
  public class WindowConfig
  {
    public WindowTypeId WindowTypeId;
    public GameObject Prefab;
  }
}