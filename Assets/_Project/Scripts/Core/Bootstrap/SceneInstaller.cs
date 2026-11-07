using System.Collections.Generic;
using UnityEngine;

public abstract class SceneInstaller : MonoBehaviour
{
    public abstract void Install(GameServices services);

    // Cari komponen T di scene milik installer ini saja.
    protected IEnumerable<T> FindInScene<T>() where T : Component
    {
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            foreach (T c in root.GetComponentsInChildren<T>(true))
                yield return c;
    }
}
