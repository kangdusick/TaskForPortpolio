#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad] // 유니티가 로드될 때 이 클래스 초기화
public class CustomLoadScene : ScriptableObject
{
    const string ScenePath = "Assets/1_kds/Scenes/";
    [MenuItem("Tools/OpenScenes/Login %#q")] // Ctrl+shift+w
    private static void Open_Login_Scene()
    {
        var SceneName = nameof(Open_Login_Scene).Split("_")[1];
        EditorSceneManager.OpenScene($"{ScenePath}{SceneName}.unity");
    }
    [MenuItem("Tools/OpenScenes/Lobby %#w")] // Ctrl+shift+w
    private static void Open_Lobby_Scene()
    {
        var SceneName = nameof(Open_Lobby_Scene).Split("_")[1];
        EditorSceneManager.OpenScene($"{ScenePath}{SceneName}.unity");
    }
    [MenuItem("Tools/OpenScenes/Game %#e")] // Ctrl+shift+w
    private static void Open_Game_Scene()
    {
        var SceneName = nameof(Open_Game_Scene).Split("_")[1];
        EditorSceneManager.OpenScene($"{ScenePath}{SceneName}.unity");
    }
    //static CustomLoadScene()
    //{
    //    EditorApplication.playModeStateChanged += LoadDefaultScene;
    //}

    //private static void LoadDefaultScene(PlayModeStateChange state)
    //{
    //    // 플레이 모드가 시작될 때 실행
    //    if (state == PlayModeStateChange.ExitingEditMode)
    //    {
    //        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo(); // 현재 씬의 변경 사항이 있다면 저장할지 묻기
    //        EditorSceneManager.OpenScene($"{ScenePath}Login.unity"); // "Login" 씬 로드
    //    }

    //    // 플레이 모드가 종료될 때 실행
    //    if (state == PlayModeStateChange.EnteredEditMode)
    //    {
    //        // 필요한 경우 여기에 코드 추가
    //    }
    //}
}
#endif