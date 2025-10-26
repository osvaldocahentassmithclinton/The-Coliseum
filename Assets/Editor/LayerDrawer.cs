using UnityEngine;
using UnityEditor; // ESSENCIAL!

// Diz ao Unity que este script customiza o atributo LayerAttribute
[CustomPropertyDrawer(typeof(LayerAttribute))]
public class LayerDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Verifica se a propriedade é um inteiro
        if (property.propertyType != SerializedPropertyType.Integer)
        {
            EditorGUI.LabelField(position, label.text, "Use [Layer] apenas com variáveis do tipo 'int'.");
            return;
        }

        // Desenha o dropdown nativo do Unity para Layers, usando o valor atual da propriedade
        property.intValue = EditorGUI.LayerField(position, label, property.intValue);
    }
}