using UnityEngine;


public static class ElementChart
{
  
    private static bool Beats(Element a, Element b)
    {
        switch (a)
        {
            case Element.Pyro:   return b == Element.Sprout; 
            case Element.Aqua:   return b == Element.Pyro;   
            case Element.Sprout: return b == Element.Aqua;  
            case Element.Volt:   return b == Element.Aqua;   
            default:             return false;
        }
    }

    
    public static float Multiplier(Element attacker, Element defender)
    {
        if (Beats(attacker, defender)) return 1.5f;   
        if (Beats(defender, attacker)) return 0.75f;  
        return 1f;                                    
    }
}