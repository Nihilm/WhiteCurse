// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class GameManager : MonoBehaviour {
//     void Start(){
//         //Time.timeScale = 0;
//     }
//     private void Awake(){
//         // if(_instance != null){
//         //     Destroy(gameObject);
//         //     throw new InvalidOperationException("[GameManager] Cannot instantiate second instance of singleton object!");
//         // }else{
//         //     _instance = this;
//         //     DontDestroyOnLoad(gameObject);
//         // }
//     }
//     private void OnDestroy(){
//         //if(_instance == this) _instance = null;
//     }
//     //public event GameStateChangeEvent onGameStateChanged;

//     private List<AsyncOperation> _loadOperations = new List<AsyncOperation>();
//     private void onLoadOperationComplete(AsyncOperation operation){
//         if(!_loadOperations.Contains(operation)) return;
//         _loadOperations.Remove(operation);
//         if(_loadOperations.Count == 0){

//         }
//     }

//     public void LoadScene(string scene){
//         AsyncOperation operation = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
//         if(operation == null) throw new ArgumentException("Failed to load scene!", scene);
//         operation.completed += onLoadOperationComplete;
//         _loadOperations.Add(operation);
//     }
//     public void UnloadScene(string scene){
//         AsyncOperation operation = SceneManager.UnloadSceneAsync(scene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
//     }

//     //  private void Start()
//     // {
//     //     if (!DarkestDungeonManager.SkipTransactions)
//     //         StartCoroutine(SceneLoading());
// 	// }

//     // private IEnumerator SceneLoading()
//     // {
//     //     Resources.UnloadUnusedAssets();
//     //     GC.Collect();

//     //     yield return new WaitForEndOfFrame();
//     //     DarkestDungeonManager.ScreenFader.Appear();
//     //     yield return new WaitForSeconds(1f);
//     //     async = SceneManager.LoadSceneAsync(DarkestDungeonManager.LoadingInfo.NextScene);
//     //     async.allowSceneActivation = false;

//     //     while (!async.isDone)
//     //     {
//     //         if (async.progress >= 0.8f)
//     //             break;
//     //         yield return null;
//     //     }
//     //     yield return new WaitForSeconds(0.5f);

//     //     while (DarkestSoundManager.NarrationQueue.Count > 0)
//     //         yield return null;

//     //     DarkestDungeonManager.ScreenFader.Fade();
//     //     yield return new WaitForSeconds(1f);
//     //     async.allowSceneActivation = true;
//     // }
// }


// /*
// using System.IO;
// using UnityEngine;
// public class AvatarSelection : MonoBehaviour
// {
//     private AvatarData avatarData;
//     private void Awake()
//     {
//         avatarData = LoadData();
//     }
//     private void Start()
//     {
//         for (int i = 0; i < avatarData.avatars.Count; i++)
//         {
//             Debug.Log("Avatar Name: " + avatarData.avatars[i].AvatarName);
//             Debug.Log("Avatar Id: " + avatarData.avatars[i].AvatarId);
//             Debug.Log("Avatar description: " + avatarData.avatars[i].Description);
//         }
//     }
//     private void OnDisable()
//     {
//         SaveData();
//     }
//     void SaveData()
//     {
//         string json = JsonUtility.ToJson(avatarData);
//         File.WriteAllText(Application.persistentDataPath + Path.DirectorySeparatorChar + "AvatarData.txt", json);
//     }
//     AvatarData LoadData()
//     {
//         AvatarData data = null;
//         if (File.Exists(Application.persistentDataPath + Path.DirectorySeparatorChar + "AvatarData.txt"))
//         {
//             data = ScriptableObject.CreateInstance<AvatarData>();
//             string json = File.ReadAllText(Application.persistentDataPath + Path.DirectorySeparatorChar + "AvatarData.txt");
//             JsonUtility.FromJsonOverwrite(json, data);
//         }
//         else
//         {
//             data = Resources.Load<AvatarData>("Avatar Data");
//         }
//         return data;
//     }
// }

 
// public interface IUseStats
// {
//     Stats Stats { get; set; }
//     void TakeDamage(float amount);
// }
 
// public class Character : MonoBehaviour, IUseStats
// {
//     public Stats StatsPrefab; // asset file (preset)
//     public Stats Stats { get; set; } // unique, for runtime
 
//     public void Awake()
//     {
//         Stats = Instantiate(StatsPrefab);
//     }
 
//     public void TakeDamage(float amount)
//     {
//         Stats.Health -= amount;
//     }
// }
 
// public class BadGuy : MonoBehaviour, IUseStats
// {
//     public Stats StatsPrefab;
//     public Stats Stats { get; set; }
 
//     public GameObject OtherGuy;
 
//     public void Awake()
//     {
//         Stats = Instantiate(StatsPrefab);
//     }
 
//     public void Start()
//     {
//         OtherGuy?.GetComponent<IUseStats>()?.TakeDamage(Stats.MyDamage);
//     }
 
//     public void TakeDamage(float amount)
//     {
//         Stats.Health -= amount;
//     }
// }
// */