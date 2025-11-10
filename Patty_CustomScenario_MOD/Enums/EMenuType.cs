using Patty_CustomScenario_MOD.QoL;

namespace Patty_CustomScenario_MOD.Enums
{
    public class EMenuType : Enumeration
    {
        public EMenuType(int id, string name)
            : base(id, name)
        {
        }
        public static readonly EMenuType All = new(1, nameof(All));
        public static readonly EMenuType Vanilla = new(2, nameof(Vanilla));
        public static readonly EMenuType Modded = new(2, nameof(Modded));
        public new static EMenuType Parse(int id)
        {
            TryParse(id, out EMenuType result);
            return result!;
        }
        public new static bool TryParse(int id, out EMenuType result)
        {
            result = null!;
            foreach (var @enum in GetAll<EMenuType>())
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
