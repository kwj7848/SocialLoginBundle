using UnityEngine;
using System;

#if UNITY_EDITOR
namespace UnityEditor
{
    [CustomPropertyDrawer(typeof(ReadonlyAttribute), true)]
    public class ReadonlyAttributeDrawer : PropertyDrawer
    {
        ReadonlyAttribute Atr => attribute as ReadonlyAttribute;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool disableNow = true;
            switch (Atr.Option)
            {
                case ReadOnlyOption.EditMode:
                    disableNow =!Application.isPlaying;
                    break;

                case ReadOnlyOption.PlayMode:
                    disableNow = Application.isPlaying;
                    break;
            }
            using (var scope = new EditorGUI.DisabledGroupScope(disableNow))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }
    }
}
#endif

[AttributeUsage(AttributeTargets.Field)]
public class ReadonlyAttribute : PropertyAttribute
{
    public ReadOnlyOption Option { get; set; } = ReadOnlyOption.Always;

    public ReadonlyAttribute() { }
    public ReadonlyAttribute(ReadOnlyOption when) => Option = when;
}

public enum ReadOnlyOption
{
    Always,
    EditMode,
    PlayMode
}