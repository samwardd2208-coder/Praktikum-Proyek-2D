// Segitiga elemen + Volt. Satu method static, dipakai rumus damage di P5.
//   Pyro > Sprout, Aqua > Pyro, Sprout > Aqua, Volt > Aqua  -> x1,5
//   kebalikannya -> x0,75 ; selain itu -> x1
public static class ElementChart
{
    public const float Strong = 1.5f;
    public const float Weak = 0.75f;

    public static float Multiplier(Element attacker, Element defender)
    {
        if (StrongAgainst(attacker, defender)) return Strong;
        if (StrongAgainst(defender, attacker)) return Weak;
        return 1f;
    }

    private static bool StrongAgainst(Element a, Element b)
    {
        return (a == Element.Pyro   && b == Element.Sprout)
            || (a == Element.Aqua   && b == Element.Pyro)
            || (a == Element.Sprout && b == Element.Aqua)
            || (a == Element.Volt   && b == Element.Aqua);
    }
}
