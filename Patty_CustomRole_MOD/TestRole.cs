using Il2Cpp;
using Il2CppInterop.Runtime.Injection;

namespace Patty_CustomRole_MOD
{
    public class TestRole : Role
    {
        public TestRole() : base(ClassInjector.DerivedConstructorPointer<TestRole>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }
        public TestRole(IntPtr ptr) : base(ptr) { }
        public override string Description => "Test Role";
        public override ActedInfo GetInfo(Character charRef) => new ActedInfo("Test Desc");
        public override ActedInfo GetBluffInfo(Character charRef) => new ActedInfo("Test Bluff");
        public override void Act(ETriggerPhase trigger, Character charRef)
        {
            if (trigger != ETriggerPhase.Day)
                return;
            onActed?.Invoke(GetInfo(charRef));
        }
        public override void BluffAct(ETriggerPhase trigger, Character charRef)
        {
            if (trigger != ETriggerPhase.Day)
                return;
            onActed?.Invoke(GetBluffInfo(charRef));
        }
        public override CharacterData? GetBluffIfAble(Character charRef) => null;
    }
}
