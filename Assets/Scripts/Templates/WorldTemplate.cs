namespace _Template {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    [CreateAssetMenu(fileName = "World", menuName = "Template/World")]
    public class WorldTemplate : ScriptableObject {
        [SerializeField] private WorldTemplate template;
        [SerializeField] private AgentTemplate[] agents = new AgentTemplate[0];

        [System.Serializable] public class AreaGraph : TopologyGraph{[SerializeField] public AreaTemplate[] nodes;}
        [SerializeField] public AreaGraph areas;

        [NonSerialized,HideInInspector] public WorldState state;
        void OnEnable(){
            Debug.Log($"Loading {this.GetType().Name}");
            state = new WorldState(){
                template = this,
                areas = Array.ConvertAll(areas.nodes, template => template?.Create()),
                agents = Array.ConvertAll(agents, template => template.Create())
            };

            //string data = LoadData($"{this.GetType().Name}.json");
            //if(data != null) JsonUtility.FromJsonOverwrite(data, this);
        }
        void OnDisable(){
            Debug.Log($"Saving {this.GetType().Name}");
            //string content = JsonUtility.ToJson(state, true);
            state = null;
            //SaveData($"{this.GetType().Name}.json", content);
        }
        #region Storage
        private static string baseDirectory = "C:/Users/user/Desktop/Git/External/Save"; //Application.persistentDataPath
        private static string LoadData(string filename){
            string filepath = $"{baseDirectory}/{filename}";
            if(!File.Exists(filepath)) return null;
            return File.ReadAllText(filepath);
        }
        private static void SaveData(string filename, string content){
            string filepath = $"{baseDirectory}/{filename}";
            File.WriteAllText(filepath, content);
        }
        #endregion
    }

    [Serializable] public class WorldState {
        [SerializeField] public int seed;
        [SerializeField] public WorldTemplate template;
        [SerializeReference] public IAgent[] agents = new IAgent[0];
        [SerializeReference] public AreaState[] areas = new AreaState[0];
        [SerializeField] public List<IAction> actions;
        [NonSerialized] public Queue<IAction> queue = new Queue<IAction>();
        public IEnumerator Update(){
            foreach(var agent in agents) agent.Enter(this);
            for(int turn = 0; true; turn++){
                while(queue.Count != 0) actionEvent?.Invoke(queue.Dequeue());
                yield return new WaitForEndOfFrame();
            }
        }
        public void AddAction(IAction action){
            action.Apply(this);
            queue.Enqueue(action);
        }
        public event System.Action<IAction> actionEvent;
    }
}