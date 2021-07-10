#if UNITY_EDITOR
using System.CodeDom.Compiler;
using UnityEditor;
using UnityEngine;

namespace PCG.MissionGraphs.Debugging
{
    public class MissionGraphDebuggerUI: EditorWindow
    {
        private int numTasks = 8;
        private int numRuleApplications = -1;
        
        [MenuItem("SynergyQuest Tools/PCG/Debug Mission Graphs")]
        public static void DebugMissionGraph()
        {
            GetWindow<MissionGraphDebuggerUI>();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Number of tasks:");
            numTasks = EditorGUILayout.IntField(numTasks);
            
            GUILayout.Label("Max. rule applications (unbounded if negative value):");
            numRuleApplications = EditorGUILayout.IntField(numRuleApplications);

            if (GUILayout.Button("Generate Mission Graph"))
            {
                if (numTasks < 0)
                {
                    Debug.LogError("Number of tasks can not be negative.");
                }

                else
                {
                    OnGenerateButton();
                }
            }
        }

        private void OnGenerateButton()
        {
            var graph = GraphBuilder.CreateGraph();
            using (var debugger = new GraphDebugger(graph))
            {
                GraphBuilder.InitGraph(graph, numTasks);
                debugger.Refresh();
                GraphBuilder.RewriteMissionGraph(graph, numRuleApplications);
                debugger.Refresh();
            }
        }
    }
}

#endif
