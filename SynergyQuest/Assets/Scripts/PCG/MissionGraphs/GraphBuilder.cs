using de.unika.ipd.grGen.Action_RewriteRules;
using de.unika.ipd.grGen.lgsp;
using de.unika.ipd.grGen.Model_Mission;
#if UNITY_EDITOR
using PCG.MissionGraphs.Debugging;
using UnityEditor;
#endif

namespace PCG.MissionGraphs
{
    public class GraphBuilder
    {
        public static MissionNamedGraph CreateGraph()
        {
            return new MissionNamedGraph();
        }
        
        public static void InitGraph(MissionNamedGraph missionGraph, int numTasks)
        {
            LGSPNode previousNode = missionGraph.CreateNodeStart();
            
            for (int i = 0; i < numTasks; ++i)
            {
                var task = missionGraph.CreateNodeTask();
                if (previousNode != null)
                {
                    missionGraph.CreateEdgeNext(previousNode, task);
                }
                previousNode = task;
            }

            End end = missionGraph.CreateNodeEnd();
            missionGraph.CreateEdgeNext(previousNode, end);
        }

        public static void RewriteMissionGraph(MissionNamedGraph graph, int numRuleApplications)
        {
            var actions = new RewriteRulesActions(graph);
            var procEnv = new LGSPGraphProcessingEnvironment(graph, actions);

            bool result = true;
            for (int i = 0; (numRuleApplications < 0 || i < numRuleApplications) && result; ++i)
            {
                result = procEnv.ApplyGraphRewriteSequence("randomRule");
            }
        }
    }
}