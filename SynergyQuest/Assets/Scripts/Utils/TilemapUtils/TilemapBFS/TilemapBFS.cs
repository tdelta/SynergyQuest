using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Utils
{
    public static partial class TilemapBFS
    {
        /**
         * <summary>
         * Generic breadth-first search algorithm to traverse the tiles of the game tilemap.
         * </summary>
         * <remarks>
         * To apply, implement <see cref="TraversalVisitor{ResultT}"/>.
         * For an example application, see <see cref="FindReachableFreeTiles"/>.
         * </remarks>
         */
        public static Optional<ResultT> traverse<ResultT>(this Tilemap tilemap, Vector2Int start, TraversalVisitor<ResultT> traversalVisitor)
        {
            // Keep a set of seen nodes, so that we will not visit them again
            var seen = new HashSet<Vector2Int>();
            
            // We start traversal at this root node
            var root = new Node(tilemap, start, seen);
            
            // The algorithm continues as long as there are nodes which are on the waitlist or until the traversalVisitor
            // returns TraversalDecision.Terminate
            //
            // (nodes can be put on the waitlist, when the traversalVisitor returns TraversalDecision.Branch)
            var waitlist = new Queue<Node>();
            
            waitlist.Enqueue(root);
            seen.Add(root.cellPosition2D);

            while (waitlist.Any())
            {
                var currentNode = waitlist.Dequeue();

                var decision = traversalVisitor.decide(currentNode);
                var maybeResult = decision.visit(new TraversalDecision<ResultT>.Visitor<ResultT>()
                {
                    // If the visitor decides to branch on the neighbors, put them on the waitlist
                    onBranch = branch =>
                    {
                        foreach (var node in branch.toTraverse)
                        {
                            waitlist.Enqueue(node);
                            seen.Add(node.cellPosition2D);
                        }

                        return Optional<ResultT>.None();
                    },
                    
                    // Terminate, if the visitor returns a Terminate decision
                    onTerminate = terminate => Optional.Some(terminate.result)
                });

                if (maybeResult.IsSome())
                {
                    return maybeResult;
                }
            }

            // If there remain no nodes to explore and the visitor didnt decide to terminate yet
            // (the remaining nodes all got Pruned)
            // then terminate with a partial result if possible
            return traversalVisitor.getPartialResult();
        }
    }
}