namespace _Algorithms {
    using System;
    using System.Collections.Generic;

	//https://en.wikipedia.org/wiki/Planarity_testing
	//https://www.boost.org/doc/libs/1_75_0/boost/graph/planar_detail/boyer_myrvold_impl.hpp
	//https://en.wikipedia.org/wiki/Biconnected_component
    public class BoyerMyrvold<T> {
        public List<List<int>> ExtractPlanarFaces(IGraph<T> graph){
            var embedding = PlanarEmbedding(graph);
            return embedding == null ? null : ExtractPlanarFaces(graph, embedding);
        }
        public List<List<int>> ExtractPlanarFaces(IGraph<T> graph, (int,int)[][] embedding){
            var faces = new List<List<int>>();
            foreach((int prev, int next) in graph.Edges.TraversePlanarEmbedding(embedding))
                if(prev == -1) faces.Add(new List<int>());
                else if(next != -1) faces[faces.Count - 1].Add(prev);
            return faces;
        }

		private IGraph<T> graph;
        private int[] verticesByDepth;
		private Queue<int> selfLoops = new Queue<int>();
		private Stack<(int,bool upper,bool lower)> mergeStack = new Stack<(int,bool,bool)>();

		private int[] leastAncestor;
		private int[] parent;
		private int[] depth;
		private int[] lowPoint;
		private LinkedList<VertexFace>[] pertinentRoots;
		private VertexFace[] faces;
		private VertexFace[] childFaces;
		private Queue<int>[] backEdges;
		private int[] backEdgeFlag;
		private int[] visited;
		private bool[] flipped;
		private int[] canonicalChild;
		private LinkedList<int>[] separatedChildList;
		private LinkedListNode<int>[] separatedNodeInParentList;

        public (int,int)[][] PlanarEmbedding(IGraph<T> graph){
            this.graph = graph;
			leastAncestor = new int[graph.NodeCount];
			parent = new int[graph.NodeCount];
			depth = new int[graph.NodeCount];
			lowPoint = new int[graph.NodeCount];

			int step = 0;
			foreach((int source, int target, bool visited) in graph.TraverseDFS<T>()){
				if(source == -1){
					parent[target] = target;
					leastAncestor[target] = step;
				}else if(target == -1){
					lowPoint[parent[source]] = Math.Min(lowPoint[parent[source]], lowPoint[source]);
				}else if(!visited){
					parent[target] = source;
					leastAncestor[target] = depth[source];
				}else if(target != parent[source]){
					lowPoint[source] = Math.Min(lowPoint[source], depth[target]);
					leastAncestor[source] = Math.Min(leastAncestor[source], depth[target]);
				}
				if(!visited && target != -1)
					lowPoint[target] = depth[target] = step++;
			}

            selfLoops.Clear(); mergeStack.Clear();
			pertinentRoots = new LinkedList<VertexFace>[graph.NodeCount];
			faces = new VertexFace[graph.NodeCount];
			childFaces = new VertexFace[graph.NodeCount];
			backEdges = new Queue<int>[graph.NodeCount];
			backEdgeFlag = new int[graph.NodeCount];
			visited = new int[graph.NodeCount];
			flipped = new bool[graph.NodeCount];
			canonicalChild = new int[graph.NodeCount];
			separatedChildList = new LinkedList<int>[graph.NodeCount];
			separatedNodeInParentList = new LinkedListNode<int>[graph.NodeCount];

			for(int i = 0; i < graph.NodeCount; i++){
				backEdges[i] = new Queue<int>();
				backEdgeFlag[i] = graph.NodeCount + 1;
				visited[i] = int.MaxValue;
				flipped[i] = false;
				canonicalChild[i] = i;
				separatedChildList[i] = new LinkedList<int>();
				pertinentRoots[i] = new LinkedList<VertexFace>();
				faces[i] = new VertexFace(){ anchor = i };
				childFaces[i] = new VertexFace(){ anchor = parent[i] };
				if(i == parent[i]) continue;
				
				(int, int) edge = (parent[i], i);
				faces[i].edges.AddLast(edge);
				childFaces[i].edges.AddLast(edge);
				faces[i].left = faces[i].right = parent[i];
				childFaces[i].left = childFaces[i].right = i;
			}

			foreach(int vertex in lowPoint.BucketSort(lowPoint.Length)){
				if(vertex == parent[vertex]) continue;
				separatedNodeInParentList[vertex] = separatedChildList[parent[vertex]].AddLast(vertex);
			}
			verticesByDepth = depth.BucketSort(depth.Length);
			// This is the main algorithm: starting with a DFS tree of embedded
			// edges (which, since it's a tree, is planar), iterate through all
			// vertices by reverse DFS number, attempting to embed all backedges
			// connecting the current vertex to vertices with higher DFS numbers.
			//
			// The walkup is a procedure that examines all such backedges and sets
			// up the required data structures so that they can be searched by the
			// walkdown in linear time. The walkdown does the actual work of
			// embedding edges and flipping bicomps, and can identify when it has
			// come across a kuratowski subgraph.
			for(int i = verticesByDepth.Length - 1; i >= 0; i--){
				int vertex = verticesByDepth[i];
				// The point of the walkup is to follow all backedges from v to
				// vertices with higher DFS numbers, and update pertinent_roots
				// for the bicomp roots on the path from backedge endpoints up
				// to v. This will set the stage for the walkdown to efficiently
				// traverse the graph of bicomps down from v.
				foreach(int neighbour in graph.Neighbours(vertex)) Walkup(vertex, neighbour);
				if(!Walkdown(vertex)) return null;
			}
			Cleanup();

			(int,int)[][] embedding = new (int, int)[graph.NodeCount][];
			for(int i = 0; i < graph.NodeCount; i++){
				embedding[i] = new (int, int)[faces[i].edges.Count];
				int index = 0;
				foreach((int, int) edge in faces[i].edges)
					embedding[i][index++] = edge;
			}
			return embedding;
        }
        private void Walkup(int source, int target){
			if(source == target){ selfLoops.Enqueue(source); return; }
			if(depth[target] < depth[source] || source == parent[target]) return;

			backEdges[target].Enqueue(source);
			int timestamp = backEdgeFlag[target] = depth[source];
			int leadVertex = target;

			while(true){
				// Move to the root of the current bicomp or the first visited
				// vertex on the bicomp by going up each side in parallel
				foreach(int faceVertex in IterateParallel(faces[leadVertex])){
					if(visited[faceVertex] == timestamp) return;

					leadVertex = faceVertex;
					visited[leadVertex] = timestamp;
				}
				// If we've found the root of a bicomp through a path we haven't
				// seen before, update pertinent_roots with a handle to the
				// current bicomp. Otherwise, we've just seen a path we've been
				// up before, so break out of the main while loop.
				int childNode = canonicalChild[leadVertex];
				int parentNode = parent[childNode];

				visited[childFaces[childNode].left] = timestamp;
				visited[childFaces[childNode].right] = timestamp;

				if(lowPoint[childNode] < depth[source] || leastAncestor[childNode] < depth[source]){
					pertinentRoots[parentNode].AddLast(childFaces[childNode]);
				}else{
					pertinentRoots[parentNode].AddFirst(childFaces[childNode]);
				}

				if(parentNode != source && visited[parentNode] != timestamp){
					leadVertex = parentNode;
				}else break;
			}
		}

		private bool Walkdown(int vertex){
			// This procedure is where all of the action is - pertinent_roots
			// has already been set up by the walkup, so we just need to move
			// down bicomps from v until we find vertices that have been
			// labeled as backedge endpoints. Once we find such a vertex, we
			// embed the corresponding edge and glue together the bicomps on
			// the path connecting the two vertices in the edge. This may
			// involve flipping bicomps along the way.
			mergeStack.Clear();
			while(pertinentRoots[vertex].Count != 0){
				var rootFaceHandle = pertinentRoots[vertex].First.Value;
				pertinentRoots[vertex].RemoveFirst();
				var currentFaceHandle = rootFaceHandle;

				while(true){
					int leftVertex = -1;
					int rightVertex = -1;
					int leftTail = currentFaceHandle.anchor;
					int rightTail = currentFaceHandle.anchor;

					foreach(int faceVertex in IterateFace(currentFaceHandle.left, currentFaceHandle.anchor, true))
					if(Pertinent(faceVertex, vertex) || ExternallyActive(faceVertex, vertex)){
						leftVertex = rightVertex = faceVertex;
						break;
					}else leftTail = faceVertex;

					if(leftVertex == -1 || leftVertex == currentFaceHandle.anchor)
						break;

					foreach(int faceVertex in IterateFace(currentFaceHandle.right, currentFaceHandle.anchor, true))
					if(Pertinent(faceVertex, vertex) || ExternallyActive(faceVertex, vertex)){
						rightVertex = faceVertex;
						break;
					}else rightTail = faceVertex;

					int chosen = -1;
					bool choseLeftUpperPath = false;

					if(InternallyActive(leftVertex, vertex)){
						chosen = leftVertex;
						choseLeftUpperPath = true;
					}else if(InternallyActive(rightVertex, vertex)){
						chosen = rightVertex;
						choseLeftUpperPath = false;
					}else if(Pertinent(leftVertex, vertex)){
						chosen = leftVertex;
						choseLeftUpperPath = true;
					}else if(Pertinent(rightVertex, vertex)){
						chosen = rightVertex;
						choseLeftUpperPath = false;
					}else{
						// If there's a pertinent vertex on the lower face
						// between the first_face_itr and the second_face_itr,
						// this graph isn't planar.
						foreach(int faceVertex in IterateFace(leftVertex, leftTail))
						if(faceVertex == rightVertex) break;
						else if(Pertinent(faceVertex, vertex)) return false;

						// Otherwise, the fact that we didn't find a pertinent
						// vertex on this face is fine - we should set the
						// short-circuit edges and break out of this loop to
						// start looking at a different pertinent root.
						if(leftVertex == rightVertex)
						if(leftTail != vertex){
							int left = faces[leftTail].left;
							int right = faces[leftTail].right;

							int temp = leftTail;
							leftTail = left == leftVertex ? right : left;
							leftVertex = temp;
						}else if(rightTail != vertex){
							int left = faces[rightTail].left;
							int right = faces[rightTail].right;

							int temp = rightTail;
							rightTail = left == rightVertex ? right : left;
							rightVertex = temp;
						}else break;

						canonicalChild[leftVertex] = canonicalChild[rootFaceHandle.left];
						canonicalChild[rightVertex] = canonicalChild[rootFaceHandle.right];

						rootFaceHandle.left = leftVertex;
						rootFaceHandle.right = rightVertex;

						if(faces[leftVertex].left == leftTail){
							faces[leftVertex].left = vertex;
						}else{
							faces[leftVertex].right = vertex;
						}

						if(faces[rightVertex].left == rightTail){
							faces[rightVertex].left = vertex;
						}else{
							faces[rightVertex].right = vertex;
						}

						break;
					}

					// When we unwind the stack, we need to know which direction
					// we came down from on the top face handle
					bool choseLeftLowerPath = (choseLeftUpperPath && faces[chosen].left == leftTail)
											  || (!choseLeftUpperPath && faces[chosen].left == rightTail);

					//If there's a backedge at the chosen vertex, embed it now
					if(backEdgeFlag[chosen] != depth[vertex]){
						mergeStack.Push((chosen, choseLeftUpperPath, choseLeftLowerPath));
						currentFaceHandle = pertinentRoots[chosen].First.Value;
						continue;
					}
					backEdgeFlag[chosen] = graph.NodeCount + 1;

					foreach(int origin in backEdges[chosen]){
						if(choseLeftLowerPath){
							faces[chosen].edges.AddFirst((origin, chosen));
							faces[chosen].left = origin;
						}else{
							faces[chosen].edges.AddLast((origin, chosen));
							faces[chosen].right = origin;
						}
					}
					//Unwind the merge stack to the root, merging all bicomps
					bool followBottom = choseLeftUpperPath;
					while(mergeStack.Count != 0){
						(int mergeVertex, bool nextFollowBottom, bool followTop) = mergeStack.Pop();

						var topHandle = faces[mergeVertex];
						var bottomHandle = pertinentRoots[mergeVertex].First.Value;
						int bottomChild = canonicalChild[bottomHandle.left];

						separatedChildList[parent[bottomChild]].Remove(separatedNodeInParentList[bottomChild]);

						pertinentRoots[mergeVertex].RemoveFirst();

						if(followTop == followBottom) bottomHandle.Flip();
						else flipped[bottomChild] = true;

						if(followTop){
							topHandle.edges.AddFirst(bottomHandle.edges);
							topHandle.left = bottomHandle.left;
						}else{
							topHandle.edges.AddLast(bottomHandle.edges);
							topHandle.right = bottomHandle.right;
						}
						followBottom = nextFollowBottom;
					}

					//Finally, embed all edges (v,w) at their upper end points
					canonicalChild[chosen] = canonicalChild[rootFaceHandle.left];
					foreach(int origin in backEdges[chosen])
						if(followBottom){
							rootFaceHandle.edges.AddFirst((origin,chosen));
							rootFaceHandle.left = rootFaceHandle.anchor == origin ? chosen : origin;
						}else{
							rootFaceHandle.edges.AddLast((origin,chosen));
							rootFaceHandle.right = rootFaceHandle.anchor == origin ? chosen : origin;
						}

					backEdges[chosen].Clear();
					currentFaceHandle = rootFaceHandle;
				}
			}
			return true;
		}

		private void Cleanup(){
			// If the graph isn't biconnected, we'll still have entries
			// in the separated_dfs_child_list for some vertices. Since
			// these represent articulation points, we can obtain a
			// planar embedding no matter what order we embed them in.
			for(int i = 0; i < graph.NodeCount; i++)
			foreach(int child in separatedChildList[i]){
				childFaces[child].Flip();
				faces[i].edges.AddFirst(childFaces[child].edges);
				faces[i].left = childFaces[child].left;
			}
			// Up until this point, we've flipped bicomps lazily by setting
			// flipped[v] to true if the bicomp rooted at v was flipped (the
			// lazy aspect of this flip is that all descendents of that vertex
			// need to have their orientations reversed as well). Now, we
			// traverse the DFS tree by DFS number and perform the actual
			// flipping as needed
			foreach(int vertex in verticesByDepth)
				if(flipped[vertex] == flipped[parent[vertex]]) flipped[vertex] = false;
				else if(flipped[vertex]) faces[vertex].Flip();
				else{
					faces[vertex].Flip();
					flipped[vertex] = true;
				}
			// If there are any self-loops in the graph, they were flagged
			// during the walkup, and we should add them to the embedding now.
			// Adding a self loop anywhere in the embedding could never
			// invalidate the embedding, but they would complicate the traversal
			// if they were added during the walkup/walkdown.
			foreach(var vertex in selfLoops){
				faces[vertex].edges.AddLast((vertex, vertex));
				faces[vertex].right = vertex;
			}
		}

		// w is pertinent with respect to v if there is a backedge (v,w) or if
		// w is the root of a bicomp that contains a pertinent vertex.
		private bool Pertinent(int vertex, int otherVertex){
			return backEdgeFlag[vertex] == depth[otherVertex] || pertinentRoots[vertex].Count != 0;
		}

		// Let a be any proper depth-first search ancestor of v. w is externally
		// active with respect to v if there exists a backedge (a,w) or a
		// backedge (a,w_0) for some w_0 in a descendent bicomp of w.
		private bool ExternallyActive(int vertex, int otherVertex){
			return leastAncestor[vertex] < depth[otherVertex]
			       || (separatedChildList[vertex].Count != 0 &&
			           lowPoint[separatedChildList[vertex].First.Value] < depth[otherVertex]);
		}
		private bool InternallyActive(int vertex, int otherVertex){
			return Pertinent(vertex, otherVertex) && !ExternallyActive(vertex, otherVertex);
		}
		private class VertexFace {
			public SinglyLinkedList<(int,int)> edges = new SinglyLinkedList<(int, int)>();
			public int anchor;
			public int left = -1;
			public int right = -1;
			public void Flip(){
				edges.Reverse();
				int temp = left; left = right; right = temp;
			}
		}
		private IEnumerable<int> IterateFace(int current, int previous, bool returnCurrent = true){
			while(true){
				yield return returnCurrent ? current : previous;
				VertexFace face = faces[current];
				if(face.left == previous){
					previous = current;
					current = face.right;
				}else if(face.right == previous){
					previous = current;
					current = face.left;
				}else{
					yield break;
				}
			}
		}
		private IEnumerable<int> IterateParallel(VertexFace face){
			using(IEnumerator<int> left = IterateFace(face.left, face.anchor, false).GetEnumerator())
			using(IEnumerator<int> right = IterateFace(face.right, face.anchor, false).GetEnumerator()){
				if(!left.MoveNext() || !right.MoveNext()) yield break;
				yield return left.Current;
				if(!left.MoveNext() || !right.MoveNext()) yield break;
				for(bool side = false; true; side = !side)
					if(side){
						yield return left.Current;
						if(!left.MoveNext()) yield break;
					}else{
						yield return right.Current;
						if(!right.MoveNext()) yield break; 
					}
			}
		}
    }
}