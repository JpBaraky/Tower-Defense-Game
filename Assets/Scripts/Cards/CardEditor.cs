using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Card))]
public class CardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty cardName     = serializedObject.FindProperty("cardName");
        SerializedProperty description  = serializedObject.FindProperty("description");
        SerializedProperty cost         = serializedObject.FindProperty("cost");
        SerializedProperty artwork      = serializedObject.FindProperty("artwork");
        SerializedProperty effectType   = serializedObject.FindProperty("effectType");
        SerializedProperty targetType   = serializedObject.FindProperty("targetType");
        SerializedProperty effectValue  = serializedObject.FindProperty("effectValue");
        SerializedProperty spawnPrefab  = serializedObject.FindProperty("spawnPrefab");
        SerializedProperty customEffect = serializedObject.FindProperty("customEffect");

        EditorGUILayout.PropertyField(cardName);
        EditorGUILayout.PropertyField(description);
        EditorGUILayout.PropertyField(cost);
        EditorGUILayout.PropertyField(artwork);

        EditorGUILayout.PropertyField(effectType);

        CardEffectType selected = (CardEffectType)effectType.enumValueIndex;

        if (selected == CardEffectType.Damage)
        {
            EditorGUILayout.PropertyField(effectValue);
            EditorGUILayout.PropertyField(targetType);
        }
        else if (selected == CardEffectType.GainMana ||
                 selected == CardEffectType.DrawCards)
        {
            EditorGUILayout.PropertyField(effectValue);
             EditorGUILayout.PropertyField(targetType);
        }
        else if (selected == CardEffectType.SpawnObject ||
                 selected == CardEffectType.SpawnTower)
        {
            EditorGUILayout.PropertyField(spawnPrefab);
        }
        else if (selected == CardEffectType.Custom)
        {
            EditorGUILayout.PropertyField(customEffect);
            EditorGUILayout.PropertyField(targetType);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
