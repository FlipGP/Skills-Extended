﻿using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using Comfort.Common;
using DrakiaXYZ.VersionChecker;
using EFT;
using EFT.InventoryLogic;
using Newtonsoft.Json;
using SkillsExtended.Controllers;
using SkillsExtended.Helpers;
using SkillsExtended.Models;
using SkillsExtended.Patches;
using SPT.Common.Http;
using SPT.Reflection.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillsExtended
{
    [BepInPlugin("com.dirtbikercj.SkillsExtended", "Skills Extended", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public const int TarkovVersion = 30626;

        public static ISession Session;

        public static GameWorld GameWorld => Singleton<GameWorld>.Instance;
        public static Player Player => Singleton<GameWorld>.Instance.MainPlayer;
        public static IEnumerable<Item> Items => Session?.Profile?.Inventory?.AllRealPlayerItems;

        // Contains key information
        public static KeysResponse Keys;

        // Contains skill data
        public static SkillDataResponse SkillData;

        public static RealismConfig RealismConfig;

        internal static GameObject Hook;

        internal static FirstAidBehaviour FirstAidScript;
        internal static FieldMedicineBehaviour FieldMedicineScript;
        internal static UsecRifleBehaviour UsecRifleScript;
        internal static BearRifleBehaviour BearRifleScript;
        internal static BearRawPowerBehavior BearPowerScript;

        internal static ManualLogSource Log;

        private void Awake()
        {
            if (!VersionChecker.CheckEftVersion(Logger, Info, Config))
            {
                throw new Exception("Invalid EFT Version");
            }
            
            new SkillPanelDisablePatch().Enable();
            new BuffIconShowPatch().Enable();
            new SkillManagerConstructorPatch().Enable();
            new SkillClassCtorPatch().Enable();
            new OnScreenChangePatch().Enable();
            new OnGameStartedPatch().Enable();
            new GetActionsClassPatch().Enable();
            new DoMedEffectPatch().Enable();
            new SetItemInHands().Enable();
#if DEBUG
            new LocationSceneAwakePatch().Enable();
#endif
            SEConfig.InitializeConfig(Config);
            Utils.GetTypes();

            Log = Logger;

            Hook = new GameObject("Skills Controller Object");
            DontDestroyOnLoad(Hook);

#if DEBUG
            new LocationSceneAwakePatch().Enable();
            ConsoleCommands.RegisterCommands();
#endif
        }

        private void Start()
        {
            Keys = Utils.Get<KeysResponse>("/skillsExtended/GetKeys");
            SkillData = Utils.Get<SkillDataResponse>("/skillsExtended/GetSkillsConfig");

            // If realism is installed, load its config
            if (Chainloader.PluginInfos.ContainsKey("RealismMod"))
            {
                var jsonString = RequestHandler.GetJson("/RealismMod/GetInfo");
                RealismConfig = JsonConvert.DeserializeObject<RealismConfig>(jsonString);
                Log.LogInfo("Realism mod detected");
            }

            if (SkillData.MedicalSkills.EnableFirstAid)
            {
                FirstAidScript = Hook.AddComponent<FirstAidBehaviour>();
            }

            if (SkillData.MedicalSkills.EnableFieldMedicine)
            {
                FieldMedicineScript = Hook.AddComponent<FieldMedicineBehaviour>();
            }

            if (SkillData.UsecRifleSkill.Enabled)
            {
                UsecRifleScript = Hook.AddComponent<UsecRifleBehaviour>();
            }

            if (SkillData.UsecTacticsSkill.Enabled)
            {
                // TODO
            }

            if (SkillData.BearRifleSkill.Enabled)
            {
                BearRifleScript = Hook.AddComponent<BearRifleBehaviour>();
            }

            if (SkillData.BearRawPowerSkill.Enabled)
            {
                //BearPowerScript = Hook.AddComponent<BearRawPowerBehavior>();
            }
        }

        private void Update()
        {
            if (Session == null && ClientAppUtils.GetMainApp().GetClientBackEndSession() != null)
            {
                Session = ClientAppUtils.GetMainApp().GetClientBackEndSession();

                Log.LogDebug("Session set");
            }
        }
    }
}