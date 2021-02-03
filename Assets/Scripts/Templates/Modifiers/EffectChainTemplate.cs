namespace _Template {
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Chain", menuName = "Template/Action/Modifier/Chain")]
    public class EffectChainTemplate : ActionModifierTemplate {
        [SerializeField] public bool fallthrough;
        [SerializeField] public bool diagonal;
        [SerializeField] public int[] tiers = new int[0];
        public override int TierCount => tiers.Length;
        public override string Description(int tier) => --tier < 0 ? "" : $"chain {tiers[tier]}";

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
            var actions = new List<List<IAction>>();
            var indices = new int[columns * rows];

            var center = location.GetTile(index);
            var helper = new GridHelper(){
                canTraverse = (int tile) => {
                    int i = indices[tile] - 1;
                    if(i == -1){
                        List<IAction> tileActions = new List<IAction>(node.Propagate(agent, source, target, tile));
                        actions.Add(tileActions);
                        indices[tile] = actions.Count;
                        i = actions.Count - 1;
                    }
                    return actions[i].Count != 0;
                },
                limit = tiers[tier],
                diagonal = diagonal
            };
            var path = helper.LongestPath(location.template.columns, location.template.rows, center.x, center.y);
            foreach(int tile in path)
                foreach(var action in actions[indices[tile] - 1])
                    yield return action;
        }
    }

    public class GridHelper {
        public delegate bool CanTraverse(int index);
        public CanTraverse canTraverse;
        public bool diagonal;
        public int limit;
        private int columns;
        private int rows;
        private bool[] visisted;
        private int maxDepth;
        private List<int> path;
        public IList<int> LongestPath(int columns, int rows, int c, int r){
            this.columns = columns; this.rows = rows;
            visisted = new bool[columns * rows];
            maxDepth = -1;
            path = new List<int>();
            Search(c, r, 0);
            return path;
        }
        private int Search(int c, int r, int depth){
            if(limit != 0 && depth >= limit) return depth - 1;
            if(c < 0 || r < 0 || c >= columns || r >= rows) return depth - 1;
            if(visisted[r + c * rows] || !canTraverse(r + c * rows)) return depth - 1;
            visisted[r + c * rows] = true;
            int maxBranch = depth;
            if(maxBranch > maxDepth){
                maxDepth = maxBranch;
                path.Add(r + c * rows);
            }
            maxBranch = Math.Max(maxBranch, Search(c, r - 1, depth + 1));
            maxBranch = Math.Max(maxBranch, Search(c, r + 1, depth + 1));
            maxBranch = Math.Max(maxBranch, Search(c + 1, r, depth + 1));
            maxBranch = Math.Max(maxBranch, Search(c - 1, r, depth + 1));
            if(diagonal){
                maxBranch = Math.Max(maxBranch, Search(c - 1, r - 1, depth + 1));
                maxBranch = Math.Max(maxBranch, Search(c - 1, r + 1, depth + 1));
                maxBranch = Math.Max(maxBranch, Search(c + 1, r - 1, depth + 1));
                maxBranch = Math.Max(maxBranch, Search(c + 1, r + 1, depth + 1));
            }
            if(maxBranch == maxDepth) path[depth] = r + c * rows;
            visisted[r + c * rows] = false;
            return maxBranch;
        }
    }
}