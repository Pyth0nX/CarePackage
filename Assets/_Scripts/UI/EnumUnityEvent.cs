using CarePackage.Main;
using UnityEditor;
using UnityEngine;

public class EnumUnityEvent : MonoBehaviour
{
    public SceneEvent onSceneSelected;

    public void LoadScene(ECarePackageScenes scene)
    {
        SceneController.Instance.LoadScene(scene);
    }
}

[System.Serializable]
public class SceneEvent : UnityEngine.Events.UnityEvent<CarePackage.Main.ECarePackageScenes> {}

[UnityEditor.CustomPropertyDrawer(typeof(SceneEvent))]
public class SceneUnityEventDrawer : UnityEditorInternal.UnityEventDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == UnityEditor.SerializedPropertyType.Enum)
        {
            UnityEditor.EditorGUI.PropertyField(position, property, GUIContent.none);
        }
        else base.OnGUI(position, property, label);
    }
}