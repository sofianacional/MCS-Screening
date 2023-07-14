using System;
using System.Collections.Generic;
using UnityEngine;


public static class SingletonManager {
    private static Dictionary<Type, MonoBehaviour> singletons = new();

    /*static SingletonManager() {
        SceneManager.sceneUnloaded += delegate {
            // Remove null singletons
            var toDelete = singletons.Keys.Where(k => !singletons[k]).ToList();
            foreach (var key in toDelete) singletons.Remove(key);
        };
    }*/

    public static T Get<T>() where T : MonoBehaviour {
        Type type = typeof(T);
        return singletons[type] as T;
    }

    public static void Register<T>(T obj) where T : MonoBehaviour {
        if (singletons.ContainsKey(obj.GetType())) return;

        singletons[typeof(T)] = obj;
    }

    public static void Remove<T>() where T : MonoBehaviour {
        singletons.Remove(typeof(T));
    }

    // Have a cleaner for the dictionary for missing types
}