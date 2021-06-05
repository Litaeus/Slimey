using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using SlimeyMod.HenryModules.Survivors;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace SlimeyMod
{
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]
    [R2APISubmoduleDependency(nameof(PrefabAPI), nameof(LanguageAPI), nameof(SoundAPI))]

    public class SlimeyPlugin : BaseUnityPlugin
    {
        public const string MODUID = "com.Litaeus.SlimeyMod";
        public const string MODNAME = "SlimeyMod";
        public const string MODVERSION = "0.0.1";

        // a prefix for name tokens to prevent conflicts- please capitalize all name tokens for convention
        public const string developerPrefix = "LIT";

        internal List<SurvivorBase> Survivors = new List<SurvivorBase>();

        public static SlimeyPlugin instance;

        private void Awake()
        {
            instance = this;

            // load assets and read config
            HenryModules.Assets.Initialize();
            HenryModules.Config.ReadConfig();
            HenryModules.States.RegisterStates(); // register states for networking
            HenryModules.Buffs.RegisterBuffs(); // add and register custom buffs/debuffs
            HenryModules.Projectiles.RegisterProjectiles(); // add and register custom projectiles
            HenryModules.Tokens.AddTokens(); // register name tokens
            HenryModules.ItemDisplays.PopulateDisplays(); // collect item display prefabs for use in our display rules

            // survivor initialization
            new SlimeySurvivor().Initialize();

            // now make a content pack and add it- this part will change with the next update
            new HenryModules.ContentPacks().Initialize();

            RoR2.ContentManagement.ContentManager.onContentPacksAssigned += LateSetup;

            Hook();
        }

        private void LateSetup(HG.ReadOnlyArray<RoR2.ContentManagement.ReadOnlyContentPack> obj)
        {
            // have to set item displays later now because they require direct object references..
            HenryModules.Survivors.SlimeySurvivor.instance.SetItemDisplays();
        }

        private void Hook()
        {
            // run hooks here, disabling one is as simple as commenting out the line
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            // a simple stat hook, adds armor after stats are recalculated
            if (self)
            {
                if (self.HasBuff(HenryModules.Buffs.armorBuff))
                {
                    self.armor += 300f;
                }
            }
        }
    }
}