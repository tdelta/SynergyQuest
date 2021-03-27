using System;
using System.Collections.Generic;

namespace Utils
{
    public static partial class TilemapBFS
    {
        /**
         * <summary>
         * ADT representing different options on how a BFS algorithm can continue the traversal of the tilemap.
         * It is returned by <see cref="TraversalVisitor{ResultT}.decide"/>.
         * </summary>
         *
         * <remarks>
         * When visiting a node, a tilemap BFS algorithm
         * (that is, a object/class that implements <see cref="TraversalVisitor{ResultT}"/>)
         * can either
         * 
         * * Branch: Continue on a subset of child nodes
         * * Prune: Dont explore the neighbors but continue
         * * Terminate: With a result
         *
         * These options are chosen by returning the corresponding instance of this ADT.
         * </remarks>
         */
        public abstract class TraversalDecision<ResultT>
        {
            public sealed class Prune : TraversalDecision<ResultT>
            {
                public override Optional<VisitResultT> visit<VisitResultT>(Visitor<VisitResultT> visitor)
                {
                    return visitor.onPrune(this);
                }
            }

            public sealed class Terminate: TraversalDecision<ResultT>
            {
                public readonly ResultT result;

                public Terminate(ResultT result)
                {
                    this.result = result;
                }

                public override Optional<VisitResultT> visit<VisitResultT>(Visitor<VisitResultT> visitor)
                {
                    return visitor.onTerminate(this);
                }
            }

            public sealed class Branch: TraversalDecision<ResultT>
            {
                public readonly List<Node> toTraverse;

                public Branch(List<Node> toTraverse)
                {
                    this.toTraverse = toTraverse;
                }

                public override Optional<VisitResultT> visit<VisitResultT>(Visitor<VisitResultT> visitor)
                {
                    return visitor.onBranch(this);
                }
            }
            
            /**
             * Visitor pattern for <see cref="TraversalDecision{ResultT}"/>
             */
            public class Visitor<VisitResultT>
            {
                public Func<Prune, Optional<VisitResultT>> onPrune = _ => Optional.None<VisitResultT>();
                public Func<Branch, Optional<VisitResultT>> onBranch = _ => Optional.None<VisitResultT>();
                public Func<Terminate, Optional<VisitResultT>> onTerminate = _ => Optional.None<VisitResultT>();
            }

            public abstract Optional<VisitResultT> visit<VisitResultT>(Visitor<VisitResultT> visitor);

        }
    }
}