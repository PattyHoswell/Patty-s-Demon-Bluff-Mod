using Patty_CustomScenario_MOD.QoL;

namespace Patty_CustomScenario_MOD.Enums
{
    public class ECharactersCountType : Enumeration
    {
        public ECharactersCountType(int id, string name)
            : base(id, name)
        {
        }
        /// <summary>
        /// The amount of character that will actually show up in the scenario.
        /// </summary>
        public static readonly ECharactersCountType Characters = new(0, nameof(Characters));
        /// <summary>
        /// The amount of character that will show up in the deck view.
        /// </summary>
        public static readonly ECharactersCountType Deck = new(1, nameof(Deck));
        public new static ECharactersCountType Parse(int id)
        {
            TryParse(id, out ECharactersCountType result);
            return result!;
        }
        public static bool TryParse(int id, out ECharactersCountType result)
        {
            result = null!;
            foreach (var @enum in GetAll<ECharactersCountType>())
            {
                if (@enum.Id == id)
                {
                    result = @enum;
                    return true;
                }
            }
            return false;
        }
    }
}
