using System;

namespace Code.Extensions
{
    public static class FunctionalExtensions
    {
        public static T With<T>(this T self, Action<T> set)
        {
            set.Invoke(self);
            return self;
        }
    
        public static T With<T>(this T self, Action<T> apply, bool when)
        {
            if (when)
                apply?.Invoke(self);

            return self;
        }
    }
}