using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Card))]
public class CardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty cardName        = serializedObject.FindProperty("cardName");
        SerializedProperty description     = serializedObject.FindProperty("description");
        SerializedProperty cost            = serializedObject.FindProperty("cost");
        SerializedProperty artwork         = serializedObject.FindProperty("artwork");
        SerializedProperty effectType      = serializedObject.FindProperty("effectType");
        SerializedProperty effectValue     = serializedObject.FindProperty("effectValue");
        SerializedProperty spawnPrefab     = serializedObject.FindProperty("spawnPrefab");
        SerializedProperty customEffect    = serializedObject.FindProperty("customEffect");

        EditorGUILayout.PropertyField(cardName);
        EditorGUILayout.PropertyField(description);
        EditorGUILayout.PropertyField(cost);
        EditorGUILayout.PropertyField(artwork);
        EditorGUILayout.PropertyField(effectType);

        CardEffectType selected = (CardEffectType)effectType.enumValueIndex;

        switch (selected)
        {
            case CardEffectType.DealDamage:
            case CardEffectType.GainMana:
            case CardEffectType.DrawCards:
                EditorGUILayout.PropertyField(effectValue);
                break;

            case CardEffectType.SpawnObject:
                EditorGUILayout.PropertyField(spawnPrefab);
                break;

            case CardEffectType.Custom:
                EditorGUILayout.PropertyField(customEffect);
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
