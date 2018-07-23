using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nv
{
    public class Fantasy
    {
        //List<Power> world;
    }

    public class StatBlock
    {
        MobSize size;
        CreatureType type;
        Tags subtypes;
        List<Element> affinity;
        Alignment alignment;
        List<AbilityScore> scores;
        List<Skill> skills;
        List<Sense> senses;
        List<Languages> langs;
        List<Special> spec;
        List<MobAction> actions;
        //List<???> vulnerabilites;
        //List<???> resistances;
        //List<???> immunities;
        float challenge;
    }

    public class MobSize
    {
        //Tiny, Small, Medium, Large, Huge, Giant
    }

    public class CreatureType
    {
        /*
        Aberration,
        Beast,
        Celestial,
        Construct,
        Dragon,
        Elemental,
        Fey,
        Fiend,
        Giant,
        Humanoid,
        Monstrosity,
        Ooze,
        Plant,
        Undead
        */
    }

    public class Element
    {
        /*
        Blood
        Earth
        Air
        Thunder
        Water
        Earth
        Fire
        Holy
         */
    }

    public class Alignment
    {
        /*
         Good
         Evil
         Lawful
         Chaotic
         Neutral
         Unaligned //creature that acts on instinct
        */
    }

    public class Movement
    {
        /*
         None
         Ground
         Burrow
         Climb
         Hover
         Fly
         Swim
         */
    }

    public class AbilityScore
    {

    }

    public class Skill
    { }
    
    public class Sense
    {
        //Sight, Touch, Sound
        //Blindsight (sight w/o eyes) ie. bats
        //Darkvision (sight in dark)
        //Tremorsense (pinpoint vibrations)
        //Truesight (sight in normal and magical darkness, invisible creatures/objects, illusions, original creatures form, see into ethereal plane)
    }

    public class Languages { }

    public class Special { }

    public class MobAction { }
}
