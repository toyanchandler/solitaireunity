using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace _Game.Tests.SolitaireModule.PlayMode
{
    public sealed class SolitaireRuntimeWiringPlayModeTests
    {
        private readonly List<GameObject> _createdObjects = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            StaticCall(FeatureRegistration, "Reset");
            _createdObjects.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            StaticCall(FeatureRegistration, "Reset");

            for (int i = _createdObjects.Count - 1; i >= 0; i--)
            {
                if (_createdObjects[i] != null)
                    UnityEngine.Object.DestroyImmediate(_createdObjects[i]);
            }

            _createdObjects.Clear();
        }

        [Test]
        public void ControllerHost_SelfRegistersAndUnregistersBundle()
        {
            GameObject host = CreateInactiveObject("ControllerHost");
            host.AddComponent(DeckController);
            host.AddComponent(InputController);
            host.AddComponent(LayoutController);
            host.AddComponent(PointerInputSource);
            host.AddComponent(HapticFeedbackProvider);
            host.AddComponent(LevelStartBridge);
            host.AddComponent(WinBridge);
            host.AddComponent(ControllerHost);

            host.SetActive(true);

            Assert.IsTrue(TryGetControllerHost(out object bundle, out string error), error);
            Assert.NotNull(Prop(bundle, "DeckController"));
            Assert.NotNull(Prop(bundle, "InputController"));
            Assert.NotNull(Prop(bundle, "LayoutController"));
            Assert.NotNull(Prop(bundle, "PointerInputSource"));
            Assert.NotNull(Prop(bundle, "HapticFeedbackProvider"));
            Assert.NotNull(Prop(bundle, "LevelStartBridge"));
            Assert.NotNull(Prop(bundle, "WinBridge"));

            host.SetActive(false);

            Assert.IsFalse(TryGetControllerHost(out _, out _));
        }

        [Test]
        public void DragLayer_SelfRegistersAndUnregistersTransform()
        {
            GameObject dragLayer = CreateInactiveObject("DragParent");
            dragLayer.AddComponent(DragLayer);

            dragLayer.SetActive(true);

            Assert.AreSame(dragLayer.transform, Prop(FeatureRegistration, "DragLayer"));

            dragLayer.SetActive(false);

            Assert.IsNull(Prop(FeatureRegistration, "DragLayer"));
        }

        [Test]
        public void BoardCamera_SelfRegistersAndConvertsScreenToWorld()
        {
            GameObject cameraObject = CreateInactiveObject("SolitaireBoardCamera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            Component controller = cameraObject.AddComponent(BoardCameraController);

            cameraObject.SetActive(true);

            Assert.AreSame(controller, Prop(FeatureRegistration, "BoardCamera"));
            object[] args = { new Vector2(10f, 10f), null };
            Assert.IsTrue((bool)Invoke(controller, "TryScreenToWorld", args));
            Assert.AreEqual(0f, ((Vector3)args[1]).z);

            cameraObject.SetActive(false);

            Assert.IsNull(Prop(FeatureRegistration, "BoardCamera"));
        }

        [Test]
        public void DuplicateSlotRegistration_FailsFast()
        {
            GameObject first = CreateInactiveObject("StockSlot_A");
            GameObject second = CreateInactiveObject("StockSlot_B");
            Component firstSlot = first.AddComponent(SlotAnchor);
            Component secondSlot = second.AddComponent(SlotAnchor);
            Call(firstSlot, "Configure", EnumValue(PileType, "Stock"), 0, EnumValue(CardSuit, "Hearts"));
            Call(secondSlot, "Configure", EnumValue(PileType, "Stock"), 0, EnumValue(CardSuit, "Hearts"));

            StaticCall(FeatureRegistration, "RegisterSlot", firstSlot);

            TargetInvocationException exception =
                Assert.Throws<TargetInvocationException>(() => StaticCall(FeatureRegistration, "RegisterSlot", secondSlot));
            Assert.IsInstanceOf<InvalidOperationException>(exception.InnerException);
        }

        private bool TryGetControllerHost(out object bundle, out string error)
        {
            object[] args = { null, null };
            bool result = (bool)Invoke(FeatureRegistration, "TryGetControllerHost", args);
            bundle = args[0];
            error = (string)args[1];
            return result;
        }

        private GameObject CreateInactiveObject(string name)
        {
            var gameObject = new GameObject(name);
            gameObject.SetActive(false);
            _createdObjects.Add(gameObject);
            return gameObject;
        }

        private static object Call(object target, string methodName, params object[] args)
        {
            return Invoke(target, methodName, args);
        }

        private static object Invoke(object target, string methodName, object[] args)
        {
            Type type = target as Type ?? target.GetType();
            object instance = target is Type ? null : target;
            return type.GetMethod(methodName).Invoke(instance, args);
        }

        private static object StaticCall(Type type, string methodName, params object[] args)
        {
            return type.GetMethod(methodName).Invoke(null, args);
        }

        private static object Prop(object target, string propertyName)
        {
            Type type = target as Type ?? target.GetType();
            object instance = target is Type ? null : target;
            return type.GetProperty(propertyName).GetValue(instance);
        }

        private static object EnumValue(Type enumType, string name)
        {
            return Enum.Parse(enumType, name);
        }

        private static Type RequiredType(string fullName)
        {
            Type type = Type.GetType(fullName + ", Assembly-CSharp");
            Assert.NotNull(type, fullName);
            return type;
        }

        private static readonly Type FeatureRegistration = RequiredType("_Game.Scripts.Project.SolitaireModule.Runtime.SolitaireFeatureRegistration");
        private static readonly Type DeckController = RequiredType("_Game.Scripts.Project.SolitaireModule.Controllers.SolitaireDeckController");
        private static readonly Type InputController = RequiredType("_Game.Scripts.Project.SolitaireModule.Controllers.SolitaireInputController");
        private static readonly Type LayoutController = RequiredType("_Game.Scripts.Project.SolitaireModule.Controllers.SolitaireLayoutController");
        private static readonly Type PointerInputSource = RequiredType("_Game.Scripts.Project.SolitaireModule.Controllers.SolitairePointerInputSource");
        private static readonly Type HapticFeedbackProvider = RequiredType("_Game.Scripts.Project.SolitaireModule.Controllers.SolitaireHapticFeedbackProvider");
        private static readonly Type LevelStartBridge = RequiredType("_Game.Scripts.Project.SolitaireModule.Controllers.SolitaireLevelStartBridge");
        private static readonly Type WinBridge = RequiredType("_Game.Scripts.Project.SolitaireModule.Controllers.SolitaireWinBridge");
        private static readonly Type ControllerHost = RequiredType("_Game.Scripts.Project.SolitaireModule.Runtime.SolitaireModuleControllerHost");
        private static readonly Type DragLayer = RequiredType("_Game.Scripts.Project.SolitaireModule.Views.SolitaireDragLayer");
        private static readonly Type BoardCameraController = RequiredType("_Game.Scripts.Project.SolitaireModule.Controllers.SolitaireBoardCameraController");
        private static readonly Type SlotAnchor = RequiredType("_Game.Scripts.Project.SolitaireModule.Views.SolitaireSlotAnchor");
        private static readonly Type PileType = RequiredType("_Game.Scripts.Project.SolitaireModule.Data.SolitairePileType");
        private static readonly Type CardSuit = RequiredType("_Game.Scripts.Project.SolitaireModule.Data.CardSuit");
    }
}
