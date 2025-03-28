using UnityEngine;

namespace Code.Common.Extensions
{
  public static class TransformExtensions
  {
    public static Transform SetWorldXY(this Transform transform, float x, float y)
    {
      transform.position = new Vector3(x, y, transform.position.z);
      return transform;
    }

    public static Transform SetWorldX(this Transform transform, float x)
    {
      transform.position = transform.position.SetX(x);
      return transform;
    }

    public static Transform AddWorldX(this Transform transform, float x)
    {
      transform.position = transform.position.AddX(x);
      return transform;
    }

    public static Transform SetLocalX(this Transform transform, float x)
    {
      transform.localPosition = transform.localPosition.SetX(x);
      return transform;
    }

    public static Transform LocalScaleX(this Transform transform, float x)
    {
      transform.localScale = transform.localScale.SetX(x);
      return transform;
    }
    
    public static Transform LocalScaleY(this Transform transform, float y)
    {
      transform.localScale = transform.localScale.SetY(y);
      return transform;
    }
    
    public static void SetScaleX(this Transform t, float scale) =>
      t.localScale = new Vector3(scale, t.localScale.y, t.localScale.z);

    public static Transform AddLocalX(this Transform transform, float x)
    {
      transform.localPosition = transform.localPosition.AddX(x);
      return transform;
    }

    public static Transform SetWorldY(this Transform transform, float y)
    {
      transform.position = transform.position.SetY(y);
      return transform;
    }

    public static Transform AddWorldY(this Transform transform, float y)
    {
      transform.position = transform.position.AddY(y);
      return transform;
    }

    public static Transform SetLocalY(this Transform transform, float y)
    {
      transform.localPosition = transform.localPosition.SetY(y);
      return transform;
    }

    public static Transform AddLocalY(this Transform transform, float y)
    {
      transform.localPosition = transform.localPosition.AddX(y);
      return transform;
    }
  }
}