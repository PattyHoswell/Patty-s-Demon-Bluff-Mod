using Il2CppInterop.Runtime.Injection;
using System;
using UnityEngine;

namespace Patty_CustomScenario_MOD.AscensionEditorGUI.Other
{
    public class CameraOverlayInfo : Il2CppSystem.Object
    {
        public CameraOverlayInfo() : base(ClassInjector.DerivedConstructorPointer<CameraOverlayInfo>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }
        public CameraOverlayInfo(IntPtr ptr) : base(ptr) { }
        public Canvas Canvas { get; internal set; }
        public Camera Camera { get; internal set; }
        public int LayerMask { get; internal set; }
    }
}
