// Segitiga elemen + Volt (sesuai modul P4)
public static class ElementChart
{
    public static float Multiplier(Element attacker, Element defender)
    {
        if (IsStrong(attacker, defender)) return 1.5f;
        if (IsStrong(defender, attacker)) return 0.75f;
        return 1f;
    }

    private static bool IsStrong(Element a, Element d)
    {
        return (a == Element.Pyro   && d == Element.Sprout) ||
               (a == Element.Aqua   && d == Element.Pyro)   ||
               (a == Element.Sprout && d == Element.Aqua)   ||
               (a == Element.Volt   && d == Element.Aqua);
    }
}
