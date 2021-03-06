using SlimeyMod.SkillStates;
using SlimeyMod.SkillStates.BaseStates;
using System;
using System.Collections.Generic;

namespace SlimeyMod.HenryModules
{
    public static class States
    {
        internal static List<Type> entityStates = new List<Type>();

        internal static void RegisterStates()
        {
            entityStates.Add(typeof(BaseMeleeAttack));
            entityStates.Add(typeof(SlashCombo));

            entityStates.Add(typeof(Shoot));

            entityStates.Add(typeof(Roll));

            entityStates.Add(typeof(ThrowBomb));
        }
    }
}