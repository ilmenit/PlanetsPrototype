using System.Collections.Generic;

/* Traits
 * Move -> Are we going to have this? First try without.
 * Size - double, triple, quad
 * Target - double, triple, quad - does 2x damage to this size and 1/2 damage to the otehrs
 * MarkTarget / Engaged - None, Standard, Double
 * Does not Return Fire
 * Does not trigger return fire
 *
 * * Cloak?
 * Ignore defense?
 * Revenge - deals damage when destroyed
 * ----------------------
 * To replace from HDM
 * - Move -
 * - Slow -
 * - Different attack ranges - RockPaperScissors Small/Medium/Large
 */

/* Tactics
 * Attack again
 * Increase defense? But what if there is no? Extra shields?
 * Hidden strike - damage random unit
 * Megabomb - damage all units
 * Retreat - one unit
 */

/* Make cool unit names
 * - Bolt
 * - Striker
 * - Ratchet - healer
 * - Lacrimosa
 * - Juggernaut
 * - Inferno
 * - Clink
 * - Dragon
 * - Wraith
 * - Zero
 * - Hydra
 * - Shadow
 * - Serpent
 * - Spider
 * - Pegasus
 */

public enum UnitSize
{
    None,
    Small,
    Medium,
    Large,
}

public class Unit
{
    public Player Player;
    public string Name;
    public string Icon;
    public int Attack;
    public UnitSize AttackSize;
    public UnitSize Size;
    public int HitPoints;
    public int HitPointsMax;
    public List<string> Traits;

    // per battle
    public int Engaged; // when enemy attacks this unit, increase the counter

    public bool Tapped;
    public BattleCell Cell;

    public Unit(string name, int attack, int hitPoints, List<string> traits, UnitSize size = UnitSize.Small, UnitSize attackSize = UnitSize.None)
    {
        Name = name;
        Icon = name.Substring(0, System.Math.Min(4, name.Length));
        Attack = attack;
        HitPointsMax = hitPoints;
        HitPoints = HitPointsMax;
        Size = size;
        AttackSize = attackSize;

        Tapped = false;

        Traits = new List<string>();
        if (traits != null)
            Traits.AddRange(traits);
    }

    public int HPProportionalDamage()
    {
        float floatAttack = (float)Attack * (float)HitPoints / (float)HitPointsMax;
        int attack = (int)System.Math.Ceiling(floatAttack);

        if (attack <= 0)
            attack = 1; // attack is always at least 1 to prevent infinite battles

        return attack;
    }

    public int HPProportionalDamage2()
    {
        return Attack;
    }

    public void Kill()
    {
        Frontend.Instance.SetDirty(Dirty.Map | Dirty.RightPanel);

        HitPoints = 0;
        if (Battle.Instance.SelectedUnit == this)
        {
            Battle.Instance.DeselectUnit();
        }

        Battle.Instance.RemoveFromCell(this);
    }

    public bool IsAlive()
    {
        return HitPoints > 0;
    }

    public void ReceiveDamage(int damage)
    {
        Frontend.Instance.SetDirty(Dirty.Map | Dirty.RightPanel);

        HitPoints -= damage;
        if (HitPoints <= 0)
        {
            Kill();
        }
    }

    /* Traits
     * [Implemented]
     * - FirstStrike
     * - NoEngage
     * - DoubleEngage
     * - TrippleEngage
     * - Ranged
     * - Berserk - after killing, attack again
     * - Side attack - enemy does not return fire
     * - SpreadAttack - attacks all in line
     * [TODO]
     * - AllInColumn
     * - Board - take as own when killing
     * - Swarm - always cause 1 damage to this unit, engage does not apply
     * - Regeneration
     * - Heals - instead of attacking it can target other friendly unit
     * - Leader - when this unit is attacking, remove all "Engagement" marks? Or Untap all in line?
     */

    public static List<Unit> Definitions = new List<Unit>()
    {
        // FIGHTERS/BOMBERS/RANGED/CRUISERS - as a tree?

        // 1
        new Unit("Fighter", 4, 9, null, UnitSize.Small, UnitSize.None ), // standard
        new Unit("Armored", 2, 15, null, UnitSize.Small, UnitSize.None ), // ore
        new Unit("Bolt", 5, 10, new List<string>() { "DoubleEngage" }, UnitSize.Small, UnitSize.None  ), // energy
        // 2
        new Unit("Cruiser", 5, 13, null, UnitSize.Medium, UnitSize.Small ), // standard
        new Unit("Torpedo Ship", 2, 10, new List<string>() { "Ranged" }  ), // energy
        new Unit("Guardian", 5, 10, new List<string>() { "FirstStrike" }  ), // experience
        // 3
        new Unit("Bomber", 5, 10, null, UnitSize.Small, UnitSize.Medium ), // standard
        new Unit("Interceptor", 4, 12, null, UnitSize.Small, UnitSize.Small ), // standard
        new Unit("Medium Bomber", 6, 10, null, UnitSize.Medium, UnitSize.Medium ), // standard
        // 4
        new Unit("Proton Bomber", 4, 9, null, UnitSize.Small, UnitSize.Large ), // standard
        new Unit("Defender", 2, 18, null, UnitSize.Medium, UnitSize.None ), // standard
        new Unit("Assault", 6, 10, null, UnitSize.Small, UnitSize.None ), // standard
        // 5
        new Unit("Berserker", 6, 13,new List<string>() { "Berserk" }, UnitSize.Small, UnitSize.None ),
        new Unit("Support", 2, 11, new List<string>() { "Ranged", "DoubleEngage" } ),
        // 6
        new Unit("Shadow", 5, 10, new List<string>() { "SideAttack" }  ), // with experience?
        new Unit("Blaster", 3, 15, new List<string>() { "SpreadAttack", "SideAttack", "NoEngage" }, UnitSize.Small, UnitSize.None ), // energy
        // 7
        new Unit("Juggernaut", 8, 20, null, UnitSize.Large, UnitSize.Medium ),
        new Unit("Titan", 8, 30, null, UnitSize.Large, UnitSize.Large  ),

        // ranged units
        // new Unit("Artillery Ship", 3, 10, new List<string>() {"Ranged", "FirstStrike" }  ),
        // new Unit("Missile Ship", 2, 10, new List<string>() { "Ranged", "SpreadAttack" }  ),
        // new Unit("DeathRay Ship", 3, 10, new List<string>() { "Ranged", "AllInColumn" }  ),

        // support units - remove enemy defense
//        new Unit("Ion", 5, 14, new List<string>() { "TrippleEngage" }  ),
//        new Unit("Command Ship", 5, 11, new List<string>() { "RegroupOnAttack?", "FirstStrike" }  ),
    };

    public static Unit FindDefinition(string name)
    {
        return Definitions.Find(i => i.Name == name) as Unit;
    }

    public Unit Clone()
    {
        return new Unit(this.Name, this.Attack, this.HitPoints, new List<string>(this.Traits), Size, AttackSize);
    }
}