using System;

namespace Infrastructure
{
    public interface ISceneLoader
    {
        void Load(string name, Action onLevelLoad);
    }
}