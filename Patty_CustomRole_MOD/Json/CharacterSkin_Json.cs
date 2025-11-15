using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Patty_CustomRole_MOD.QoL;

namespace Patty_CustomRole_MOD.Json
{
    public class CharacterSkin_Json
    {
        public string Name { get; set; } = "";
        public string SkinId { get; set; } = "";
        public bool AutoUnlocked { get; set; } = true;
        public string ArtistName { get; set; } = "";
        public string ArtistLink { get; set; } = "";

        public ERarity SkinRarity { get; set; } = ERarity.Default;
        public string Art { get; set; } = "";
        public string AnimatedArt { get; set; } = "";
        public string LockedArt { get; set; } = "";
        public EArtType Type { get; set; } = EArtType.Default;
        public Color32_Json GlowColor { get; set; } = new Color32_Json();

        public TypeScript_Json UnlockWith { get; set; } = new TypeScript_Json();
        public string UnlockWithAchievementTarget { get; set; } = "";

        public string Flavor { get; set; } = "";
        public string Notes { get; set; } = "";
        public string SkinFor { get; set; } = "";

        public CharacterSkin_Json() { }
        public CharacterSkin_Json(SkinData data)
        {
            Name = data.name;
            SkinId = data.skinId;
            AutoUnlocked = true;
            ArtistName = data.artistName;
            ArtistLink = data.artistLink;
            SkinRarity = data.skinRarity;
            Type = data.type;
            Art = data.art?.name ?? "";
            AnimatedArt = data.animated_art?.name ?? "";
            LockedArt = data.lockedArt?.name ?? "";
            GlowColor = new Color32_Json(data.glowColor);
            if (data.unlockWith != null)
            {
                UnlockWith.AssemblyName = Path.GetFileName(data.unlockWith.GetType().Assembly.Location);
                UnlockWith.ScriptName = data.unlockWith.GetIl2CppType().FullName;
                var unlockWithAchievement = data.unlockWith.TryCast<UnlockWithAchievement>();
                if (unlockWithAchievement != null)
                {
                    UnlockWithAchievementTarget = unlockWithAchievement.achiv?.id ?? "";
                }
            }
            Flavor = data.flavor;
            Notes = data.notes;
            SkinFor = data.skinFor?.characterId ?? "";
        }


        // By this point a custom character may not be added yet
        public void AssignData(SkinData assignTo)
        {
            assignTo.name = Name;
            assignTo.skinId = SkinId;
            assignTo.artistName = ArtistName;
            assignTo.artistLink = ArtistLink;
            assignTo.skinRarity = SkinRarity;
            assignTo.art = Utility.FindSprite(Art);
            assignTo.animated_art = Utility.FindSprite(AnimatedArt);
            assignTo.lockedArt = Utility.FindSprite(LockedArt);
            assignTo.type = Type;
            assignTo.glowColor = GlowColor;
            var unlockLogicType = Utility.FindType(UnlockWith.AssemblyName, UnlockWith.ScriptName);
            if (unlockLogicType != null && typeof(UnlockWith).IsAssignableFrom(unlockLogicType))
            {
                if (!ClassInjector.IsTypeRegisteredInIl2Cpp(unlockLogicType))
                {
                    ClassInjector.RegisterTypeInIl2Cpp(unlockLogicType);
                }
                assignTo.unlockWith = (UnlockWith)Activator.CreateInstance(unlockLogicType);
                var unlockWithAchievement = assignTo.unlockWith.TryCast<UnlockWithAchievement>();
                if (unlockWithAchievement != null)
                {
                    unlockWithAchievement.achiv = Utility.FindAchievementById(UnlockWithAchievementTarget);
                }
            }
            else if (unlockLogicType != null && !typeof(UnlockWith).IsAssignableFrom(unlockLogicType))
            {
                CustomRole.Logger.Error($"UnlockWith '{UnlockWith.ScriptName}' in assembly '{UnlockWith.AssemblyName}' is not a subclass of UnlockWith, will default to using placeholder script.");
                assignTo.unlockWith = new UnlockWithAchievement();
            }
            else
            {
                CustomRole.Logger.Error($"UnlockWith '{UnlockWith.ScriptName}' is not found in assembly '{UnlockWith.AssemblyName}', will default to using placeholder script.");
                assignTo.unlockWith = new UnlockWithAchievement();
            }
            assignTo.flavor = Flavor;
            assignTo.notes = Notes;
        }

        public void AssignSkin(SkinData assignTo, CharacterData character)
        {
            assignTo.skinFor = character;
        }
    }
}
