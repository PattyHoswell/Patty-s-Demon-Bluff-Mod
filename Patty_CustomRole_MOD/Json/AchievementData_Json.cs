using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Patty_CustomRole_MOD.QoL;

namespace Patty_CustomRole_MOD.Json
{
    public class AchievementData_Json
    {
        public string Name { get; set; } = "";
        public string Id { get; set; } = "";
        public bool AutoUnlocked { get; set; } = true;
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string RealEvent { get; set; } = "";
        public string Art { get; set; } = "";
        public string Bg { get; set; } = "";
        public Color32_Json FrameColor { get; set; } = new Color32_Json();
        public Color32_Json GlowColor { get; set; } = new Color32_Json();
        public List<string> UnlockedSkins { get; set; } = new List<string>();
        public TypeScript_Json UnlockLogicScript { get; set; } = new TypeScript_Json();
        public bool DebugOptions { get; set; }

        public AchievementData_Json() { }
        public AchievementData_Json(AchievementData data)
        {
            Name = data.name;
            Id = data.id;
            AutoUnlocked = true;
            Title = data.title;
            Description = data.description;
            RealEvent = data.realEvent;
            Art = data.art?.name ?? "";
            Bg = data.bg?.name ?? "";
            FrameColor = data.frameColor;
            GlowColor = data.glowColor;
            if (data.unlockedSkins != null)
            {
                foreach (var skin in data.unlockedSkins)
                {
                    UnlockedSkins.Add(skin.skinId);
                }
            }
            if (data.unlockLogic != null)
            {
                UnlockLogicScript.AssemblyName = Path.GetFileName(data.unlockLogic.GetType().Assembly.Location);
                UnlockLogicScript.ScriptName = data.unlockLogic.GetIl2CppType().FullName;
            }
        }
        public void AssignData(AchievementData achievementData)
        {
            achievementData.name = Name;
            achievementData.id = Id;
            achievementData.title = Title;
            achievementData.description = Description;
            achievementData.realEvent = RealEvent;
            achievementData.art = Utility.FindSprite(Art);
            achievementData.bg = Utility.FindSprite(Bg);
            achievementData.frameColor = FrameColor;
            achievementData.glowColor = GlowColor;
            achievementData.unlockedSkins = new SkinData[0];
            foreach (var skinId in UnlockedSkins)
            {
                var skin = Utility.FindSkinById(skinId);
                if (skin == null)
                {
                    continue;
                }
                achievementData.unlockedSkins = achievementData.unlockedSkins.Append(skin).ToArray();
            }
            Id = achievementData.id;
            Title = achievementData.title;
            Description = achievementData.description;
            RealEvent = achievementData.realEvent;
            Art = achievementData.art?.name ?? "";
            Bg = achievementData.bg?.name ?? "";
            FrameColor = achievementData.frameColor;
            GlowColor = achievementData.glowColor;
            if (achievementData.unlockedSkins != null)
            {
                foreach (var skin in achievementData.unlockedSkins)
                {
                    UnlockedSkins.Add(skin.skinId);
                }
            }
            var unlockLogicType = Utility.FindType(UnlockLogicScript.AssemblyName, UnlockLogicScript.ScriptName);
            if (unlockLogicType != null && typeof(AchievementUnlockLogic).IsAssignableFrom(unlockLogicType))
            {
                if (!ClassInjector.IsTypeRegisteredInIl2Cpp(unlockLogicType))
                {
                    ClassInjector.RegisterTypeInIl2Cpp(unlockLogicType);
                }
                achievementData.unlockLogic = (AchievementUnlockLogic)Activator.CreateInstance(unlockLogicType);
            }
            else if (unlockLogicType != null && !typeof(AchievementUnlockLogic).IsAssignableFrom(unlockLogicType))
            {
                CustomRole.Logger.Error($"AchievementUnlockLogic '{UnlockLogicScript.ScriptName}' in assembly '{UnlockLogicScript.AssemblyName}' is not a subclass of AchievementUnlockLogic, will default to using placeholder script.");
                achievementData.unlockLogic = new UnlockableAchiv();
            }
            else
            {
                CustomRole.Logger.Error($"AchievementUnlockLogic '{UnlockLogicScript.ScriptName}' is not found in assembly '{UnlockLogicScript.AssemblyName}', will default to using placeholder script.");
                achievementData.unlockLogic = new UnlockableAchiv();
            }
        }
    }
}
