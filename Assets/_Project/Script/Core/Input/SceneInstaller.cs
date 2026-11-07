using System.Collections.Generic;
using UnityEngine;

// Satu installer di ROOT tiap scene. SceneFlow memanggil Install(services)
// tanpa peduli jenisnya (polymorphism): Scn_World -> WorldInstaller, Scn_Battle -> BattleInstaller.
public abstract class SceneInstaller : MonoBehaviour
{
    public abstract void Install(GameServices services);

    // Cari komponen di seluruh scene ini (termasuk objek nonaktif).
    protected IEnumerable<T> FindInScene<T>() where T : Component
    {
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            foreach (T c in root.GetComponentsInChildren<T>(true))
                yield return c;
    }
}
