using UnityEngine;

public class Item : MonoBehaviour
{
    protected Color _color;
    
    public Color Color => _color;
    
    public virtual void SetColor(Color color)
    {
        _color = color;
    }
}