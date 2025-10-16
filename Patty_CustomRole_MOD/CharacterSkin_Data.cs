using Il2Cpp;

namespace Patty_CustomRole_MOD
{
    public class CharacterSkin_Data
    {
        public string SkinId { get; set; } = "";
        public string ArtistName { get; set; } = "";
        public string ArtistLink { get; set; } = "";

        public ERarity SkinRarity { get; set; } = ERarity.Default;
        public string Art { get; set; } = "";
        public string AnimatedArt { get; set; } = "";
        public string LockedArt { get; set; } = "";
        public EArtType Type { get; set; } = EArtType.Default;
        public Color32_Data GlowColor { get; set; } = new Color32_Data();

        // public UnlockWith unlockWith;

        public string Flavor { get; set; } = "";
        public string Notes { get; set; } = "";
        public string SkinFor { get; set; } = "";

        public CharacterSkin_Data(SkinData data)
        {
            SkinId = data.skinId;
            ArtistName = data.artistName;
            ArtistLink = data.artistLink;
            SkinRarity = data.skinRarity;
            Type = data.type;
            Art = data.art?.name ?? "";
            AnimatedArt = data.animated_art?.name ?? "";
            LockedArt = data.lockedArt?.name ?? "";
            GlowColor = new Color32_Data(data.glowColor);
            Flavor = data.flavor;
            Notes = data.notes;
            SkinFor = data.skinFor?.name ?? "";
        }

        public CharacterSkin_Data() { }

        // By this point a custom character may not be added yet
        public void AssignData(SkinData assignTo, bool assignSkin = false)
        {
            assignTo.skinId = SkinId;
            assignTo.artistName = ArtistName;
            assignTo.artistLink = ArtistLink;
            assignTo.skinRarity = SkinRarity;
            assignTo.art = Utility.FindSprite(Art);
            assignTo.animated_art = Utility.FindSprite(AnimatedArt);
            assignTo.lockedArt = Utility.FindSprite(LockedArt);
            assignTo.type = Type;
            assignTo.glowColor = GlowColor;
            assignTo.flavor = Flavor;
            assignTo.notes = Notes;
            if (assignSkin)
                AssignSkin(assignTo);
        }

        public void AssignSkin(SkinData assignTo)
        {
            assignTo.skinFor = Utility.FindCharacter(SkinFor);
        }
    }
}
