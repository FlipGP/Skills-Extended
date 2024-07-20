﻿using EFT.UI;
using EFT.UI.Screens;
using SkillsExtended.Helpers;
using SPT.Reflection.Patching;
using System.Linq;
using System.Reflection;

namespace SkillsExtended.Patches
{
    internal class OnScreenChangePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(MenuTaskBar).GetMethod("OnScreenChanged");

        [PatchPrefix]
        public static void Prefix(EEftScreenType eftScreenType)
        {
            if (eftScreenType == EEftScreenType.Inventory)
            {
                if (Plugin.SkillData.MedicalSkills.EnableFieldMedicine)
                {
                    Plugin.FieldMedicineScript.FieldMedicineInstanceIDs.Clear();

                    Plugin.FieldMedicineScript.FieldMedicineUpdate();
                }

                if (Plugin.SkillData.MedicalSkills.EnableFirstAid)
                {
                    Plugin.FirstAidScript.FirstAidInstanceIDs.Clear();

                    Plugin.FirstAidScript.FirstAidUpdate();
                }

                if (Plugin.SkillData.UsecRifleSkill.Enabled)
                {
                    Plugin.UsecRifleScript.WeaponInstanceIds.Clear();

                    var usecWeapons = Plugin.SkillData.UsecRifleSkill;

                    Plugin.UsecRifleScript.UsecWeapons = Plugin.Session.Profile.Inventory.AllRealPlayerItems
                        .Where(x => usecWeapons.Weapons.Contains(x.TemplateId));
                }

                if (!Plugin.SkillData.BearRifleSkill.Enabled)
                {
                    Plugin.BearRifleScript.WeaponInstanceIds.Clear();

                    var bearWeapons = Plugin.SkillData.BearRifleSkill;

                    Plugin.BearRifleScript.BearWeapons = Plugin.Session.Profile.Inventory.AllRealPlayerItems
                        .Where(x => bearWeapons.Weapons.Contains(x.TemplateId));
                }
            }
        }
    }
}