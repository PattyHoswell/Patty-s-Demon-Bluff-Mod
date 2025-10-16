using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace Patty_CustomRole_MOD
{
    public class Role_Data
    {
        public string Name { get; set; } = "";

        public List<string> BundledCharacters { get; set; } = new List<string>();

        public string CharacterId { get; set; } = "";
        public string Description { get; set; } = "";
        public string FlavorText { get; set; } = "";
        public string Hints { get; set; } = "";
        public string IfLies { get; set; } = "";
        public string Notes { get; set; } = "";

        public string Art { get; set; } = "";
        public string ArtCute { get; set; } = "";
        public string ArtNice { get; set; } = "";
        public string ArtAnimated { get; set; } = "";
        public string RandomArt { get; set; } = "";
        public string BackgroundArt { get; set; } = "";

        public string CurrentSkin { get; set; } = "";
        public List<string> Skins { get; set; } = new List<string>();

        public Color32_Data Color { get; set; } = new Color32_Data();
        public Color32_Data ArtBgColor { get; set; } = new Color32_Data();
        public Color32_Data CardBgColor { get; set; } = new Color32_Data();
        public Color32_Data CardBorderColor { get; set; } = new Color32_Data();

        public List<ECharacterTag> Tags { get; set; } = new List<ECharacterTag>();
        public List<string> CanAppearIf { get; set; } = new List<string>();
        public ECharacterType Type { get; set; } = ECharacterType.None;
        public EAlignment StartingAlignment { get; set; } = EAlignment.None;
        public EAbilityUsage AbilityUsage { get; set; } = EAbilityUsage.Once;
        public bool Bluffable { get; set; } = true;
        public bool Picking { get; set; } = false;
        public TypeScript_Data RoleScript { get; set; } = new TypeScript_Data();

        public Role_Data() { }

        public Role_Data(CharacterData characterData)
        {
            Name = characterData.name;
            BundledCharacters = new List<string>();
            foreach (var bundled in characterData.bundledCharacters)
            {
                if (bundled == null)
                    continue;
                BundledCharacters.Add(bundled.name);
            }
            CharacterId = characterData.characterId;
            Description = characterData.description;
            FlavorText = characterData.flavorText;
            Hints = characterData.hints;
            IfLies = characterData.ifLies;
            Notes = characterData.notes;

            Art = characterData.art?.name ?? "";
            ArtCute = characterData.art_cute?.name ?? "";
            ArtNice = characterData.art_nice?.name ?? "";
            ArtAnimated = characterData.art_animated?.name ?? "";
            RandomArt = characterData.randomArt?.name ?? "";
            BackgroundArt = characterData.backgroundArt?.name ?? "";

            CurrentSkin = characterData.currentSkin?.name ?? "";
            Skins = new List<string>();
            foreach (var skin in characterData.skins)
            {
                if (skin == null)
                    continue;
                Skins.Add(skin.name);
            }

            Color = new Color32_Data(characterData.color);
            ArtBgColor = new Color32_Data(characterData.artBgColor);
            CardBgColor = new Color32_Data(characterData.cardBgColor);
            CardBorderColor = new Color32_Data(characterData.cardBorderColor);

            Tags = new List<ECharacterTag>();
            foreach (var tag in characterData.tags)
                Tags.Add(tag);
            CanAppearIf = new List<string>();
            foreach (var canAppear in characterData.canAppearIf)
            {
                if (canAppear == null)
                    continue;
                CanAppearIf.Add(canAppear.name);
            }

            Type = characterData.type;
            StartingAlignment = characterData.startingAlignment;
            AbilityUsage = characterData.abilityUsage;
            Bluffable = characterData.bluffable;
            Picking = characterData.picking;
            RoleScript = new TypeScript_Data();
            if (characterData.role != null)
            {
                RoleScript.AssemblyName = Path.GetFileName(characterData.role.GetType().Assembly.Location);
                RoleScript.ScriptName = characterData.role.GetIl2CppType().FullName;
            }
        }

        public void AssignData(CharacterData assignTo)
        {
            assignTo.name = Name;
            assignTo.bundledCharacters = new Il2CppSystem.Collections.Generic.List<CharacterData>();
            foreach (var bundled in BundledCharacters)
            {
                var character = Utility.FindCharacter(bundled);
                if (character == null)
                {
                    CustomRole.Logger.Error($"Character '{bundled}' not found in game data. Skipping.");
                    continue;
                }
                assignTo.bundledCharacters.Add(character);
            }

            assignTo.characterId = CharacterId;
            assignTo.description = Description;
            assignTo.flavorText = FlavorText;
            assignTo.hints = Hints;
            assignTo.ifLies = IfLies;
            assignTo.notes = Notes;


            assignTo.art ??= Utility.FindSprite(Art);
            assignTo.art_cute ??= Utility.FindSprite(ArtCute);
            assignTo.art_nice ??= Utility.FindSprite(ArtNice);
            assignTo.art_animated ??= Utility.FindSprite(ArtAnimated);
            assignTo.randomArt ??= Utility.FindSprite(RandomArt);
            assignTo.backgroundArt ??= Utility.FindSprite(BackgroundArt);

            assignTo.currentSkin ??= Utility.FindSkin(CurrentSkin);
            assignTo.skins = new Il2CppSystem.Collections.Generic.List<SkinData>();
            foreach (var skinName in Skins)
            {
                var skin = Utility.FindSkin(skinName);
                if (skin == null)
                {
                    // At this point in time, custom skin haven't been loaded yet
                    // CustomRole.Logger.Error($"Skin '{skinName}' not found in game data. Skipping.");
                    continue;
                }
                assignTo.skins.Add(skin);
            }
            assignTo.color = Color;
            assignTo.artBgColor = ArtBgColor;
            assignTo.cardBgColor = CardBgColor;
            assignTo.cardBorderColor = CardBorderColor;

            assignTo.tags = new Il2CppSystem.Collections.Generic.List<ECharacterTag>();
            foreach (var tag in Tags)
                assignTo.tags.Add(tag);
            assignTo.canAppearIf = new Il2CppSystem.Collections.Generic.List<CharacterData>();
            foreach (var charName in CanAppearIf)
            {
                var character = Utility.FindCharacter(charName);
                if (character == null)
                {
                    CustomRole.Logger.Error($"Character '{charName}' not found in game data. Skipping.");
                    continue;
                }
                assignTo.canAppearIf.Add(character);
            }

            assignTo.type = Type;
            assignTo.startingAlignment = StartingAlignment;
            assignTo.abilityUsage = AbilityUsage;
            assignTo.bluffable = Bluffable;
            assignTo.picking = Picking;
            var roleType = Utility.FindType(RoleScript.AssemblyName, RoleScript.ScriptName);
            if (roleType != null && typeof(Role).IsAssignableFrom(roleType))
            {
                if (!ClassInjector.IsTypeRegisteredInIl2Cpp(roleType))
                {
                    ClassInjector.RegisterTypeInIl2Cpp(roleType);
                }
                assignTo.role = (Role)Activator.CreateInstance(roleType);
            }
            else if (roleType != null && !typeof(Role).IsAssignableFrom(roleType))
            {
                CustomRole.Logger.Error($"Role '{RoleScript.ScriptName}' in assembly '{RoleScript.AssemblyName}' is not a subclass of Role, will default to using placeholder script.");
                assignTo.role = new TestRole();
            }
            else
            {
                CustomRole.Logger.Error($"Role '{RoleScript.ScriptName}' is not found in assembly '{RoleScript.AssemblyName}', will default to using placeholder script.");
                assignTo.role = new TestRole();
            }
        }
    }

    public class Color32_Data
    {
        public byte R { get; set; } = 255;
        public byte G { get; set; } = 255;
        public byte B { get; set; } = 255;
        public byte A { get; set; } = 255;

        public Color32_Data() { }
        public Color32_Data(Color32 color32)
        {
            R = color32.r;
            G = color32.g;
            B = color32.b;
            A = color32.a;
        }

        public static implicit operator Color(Color32_Data data)
        {
            return new Color32(data.R, data.G, data.B, data.A);
        }

        public static implicit operator Color32_Data(Color color)
        {
            return new Color32_Data(color);
        }

        public static implicit operator Color32(Color32_Data data)
        {
            return new Color32(data.R, data.G, data.B, data.A);
        }

        public static implicit operator Color32_Data(Color32 color32)
        {
            return new Color32_Data(color32);
        }
    }

    public class TypeScript_Data
    {
        public string AssemblyName { get; set; } = "";
        public string ScriptName { get; set; } = "";
    }
}