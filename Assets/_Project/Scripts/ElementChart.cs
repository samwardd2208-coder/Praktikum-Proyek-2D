using UnityEngine;

public static class ElementChart
{
    public static float Multiplier(string attacker, string defender)
    {
        // 1. Keunggulan elemen (Multiplier x1,5)
        if (attacker == "Pyro" && defender == "Sprout") return 1.5f;
        if (attacker == "Aqua" && defender == "Pyro") return 1.5f;
        if (attacker == "Sprout" && defender == "Aqua") return 1.5f;
        if (attacker == "Volt" && defender == "Aqua") return 1.5f;

        // 2. Kebalikannya / Lemah (Multiplier x0,75)
        if (attacker == "Sprout" && defender == "Pyro") return 0.75f;
        if (attacker == "Pyro" && defender == "Aqua") return 0.75f;
        if (attacker == "Aqua" && defender == "Sprout") return 0.75f;
        if (attacker == "Aqua" && defender == "Volt") return 0.75f;

        // 3. Lainnya / Netral (Multiplier x1)
        return 1.0f;
    }
}