using System.Collections.Generic;
using System.Linq;
using GameGC.CommonEditorUtils.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MTPS.Inventory.Editor
{
    [CustomEditor(typeof(Inventory))]
    public class InventoryEditor : UnityEditor.Editor
    {
        private string[] names;
        private int Selected;

        StyleLength w50
        {
            get
            {
                var style = new StyleLength();
                style.value = new Length(50, LengthUnit.Percent);
                return style;
            }
        }

        private void OnEnable() => names = (target as Inventory).EDItorREF.KeysArray.Select(w => w.name).ToArray();

        private void OnValidate()
        {
            names = (target as Inventory).EDItorREF.KeysArray.Select(w => w.name).ToArray();
            if(choises != null)
                choises.choices = new List<string>(names);
        }

        private PopupField<string> choises;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var horizontalScope = new VisualElement();
            horizontalScope.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);

            var button = new Button(Switch);
            button.text = "Equip GUN:";
            button.tooltip = "Equip gun to be selected as first on play";
            button.style.width = w50;
            horizontalScope.Add(button);


            choises = new PopupField<string>(names.ToList(), Selected)
            {
                style =
                {
                    width = w50,
                    paddingRight = new StyleLength(6),
                    marginRight = 0,
                    marginLeft = 0
                }
            };
            choises.RegisterValueChangedCallback(OnPopupValueChanged);


            horizontalScope.Add(choises);

            root.Add(horizontalScope);

            GCUIElementsUtils.FillDefaultInspectorWithExclude(root, serializedObject, this, new[] {"m_Script"});
            return root;
        }

        private void OnPopupValueChanged(ChangeEvent<string> evt)
        {
            var target = evt.target as PopupField<string>;
            Selected = target.index;
        }

        private void Switch()
        {
            (target as Inventory).EquipImmediateEditor(Selected);
        }
        //public override void OnInspectorGUI()
        //{
        //   GUILayout.BeginHorizontal();
        //   if (GUILayout.Button("Switch GUN:"))
        //   {
        //      (target as WeaponSwitch).SwitchImmediateEditor(Selected);
        //   }
        //   Selected = EditorGUILayout.Popup(Selected, names);
        //   GUILayout.EndHorizontal();
        //   base.OnInspectorGUI();
        //}
    }
}