namespace _Template {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    
    [CreateAssetMenu(fileName = "Spread", menuName = "Template/Action/Modifier/Spread")]
    public class EffectSpreadTemplate : ActionModifierTemplate {
        [SerializeField] public bool diagonal;
        [SerializeField] public bool fallthrough;
        [Tooltip("Scatter over area."), SerializeField] public bool proximity;
        [SerializeField] public int[] tiers = new int[0];
        public override int TierCount => tiers.Length;
        public override string Description(int tier) => --tier < 0 ? "" : $"spread {tiers[tier]}";

        public override IEnumerable<IAction> Apply(
            ModifierNode node, int tier,
            IAgent agent, ITarget source, IContainer target, int index
        ){
            if(--tier < 0){
                if(fallthrough) foreach(var action in node.Propagate(agent, source, target, index))
                    yield return action;
                yield break;
            }
            var location = target as LocationState;
            int columns = location.template.columns, rows = location.template.rows;
            int value = tiers[tier];
            int[] distances = new int[location.TileCount];
            var queue = new Queue<int>();
            var neighbours = new Stack<int>();
            queue.Enqueue(index);
            while(queue.Count != 0){
                int tile = queue.Dequeue();
                bool hit = false;
                if(proximity && distances[tile] >= value) continue;
                if(!proximity && value < 0) continue;
                foreach(var action in node.Propagate(agent, source, target, tile)){
                    yield return action; hit = true;
                }
                if(!proximity && hit) value--;
                if(!proximity) hit = value < 0;
                if(!hit) continue;
                int distance = ++distances[tile];
                var (c, r) = location.GetTile(tile);
                if(c - 1 >= 0) neighbours.Push((r) + (c - 1) * rows);
                if(r - 1 >= 0) neighbours.Push((r - 1) + (c) * rows);
                if(c + 1 < columns) neighbours.Push((r) + (c + 1) * rows);
                if(r + 1 < rows) neighbours.Push((r + 1) + (c) * rows);

                if(diagonal && c - 1 >= 0 && r - 1 >= 0) neighbours.Push((r - 1) + (c - 1) * rows);
                if(diagonal && c - 1 >= 0 && r + 1 < rows) neighbours.Push((r + 1) + (c - 1) * rows);
                if(diagonal && c + 1 < columns && r - 1 >= 0) neighbours.Push((r - 1) + (c + 1) * rows);
                if(diagonal && c + 1 < columns && r + 1 < rows) neighbours.Push((r + 1) + (c + 1) * rows);

                while(neighbours.Count != 0){
                    int neighbour = neighbours.Pop();
                    if(distances[neighbour] != 0) continue;
                    distances[neighbour] = distance;
                    queue.Enqueue(neighbour);
                }
            }
        }
    }
}


