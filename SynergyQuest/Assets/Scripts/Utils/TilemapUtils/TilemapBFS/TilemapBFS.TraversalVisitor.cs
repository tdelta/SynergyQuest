namespace Utils
{
    public static partial class TilemapBFS
    {
        /**
         * Implement this interface and pass the object to <see cref="TilemapBFS.traverse{ResultT}"/> to realize a BFS
         * algorithm.
         */
        public interface TraversalVisitor<ResultT>
        {
            /**
             * Given a node in the tilemap, decides how to continue the algorithm.
             * One can either
             *
             * * Branch: Continue on a subset of child nodes
             * * Prune: Dont explore the neighbors but continue
             * * Terminate: With a result
             *
             * See <see cref="TraversalDecision{ResultT}"/>.
             */
            TraversalDecision<ResultT> decide(Node node);

            /**
             * Allows to return a result even when the algorithm never produced a
             * <see cref="TraversalDecision{ResultT}.Terminate"/> result.
             *
             * It is called, when all nodes have been pruned and none remains in the BFS waitlist.
             */
            Optional<ResultT> getPartialResult();
        }
    }
}