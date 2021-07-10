using System;
using de.unika.ipd.grGen.Action_RewriteRules;
using de.unika.ipd.grGen.grShell;
using de.unika.ipd.grGen.lgsp;
using de.unika.ipd.grGen.libGr;
using de.unika.ipd.grGen.Model_GraphModel;
using de.unika.ipd.grGen.Model_Mission;
using BelongsTo = de.unika.ipd.grGen.Model_GraphModel.BelongsTo;
using Edge = de.unika.ipd.grGen.Model_GraphModel.Edge;
using Lock = de.unika.ipd.grGen.Model_Mission.Lock;
using Next = de.unika.ipd.grGen.Model_GraphModel.Next;
using Node = de.unika.ipd.grGen.Model_GraphModel.Node;
using Task = de.unika.ipd.grGen.Model_Mission.Task;

namespace PCG.MissionGraphs.Debugging
{
    public class GraphDebugger: IDisposable
    {
        private YCompClient client;
        
        public GraphDebugger(MissionNamedGraph graph)
        {
            var dumpInfo = ConfigureVisualization(graph);
                
            client = YCompClient.CreateYCompClient(
                $"{PathUtils.GetInstallDirectory()}/../ExternalTools/grgen/engine-net-2/bin/ycomp",
                graph,
                "Hierarchical",
                dumpInfo
            );
            client.RegisterLibGrEvents();
        }

        private static DumpInfo ConfigureVisualization(MissionNamedGraph graph)
        {
            var dumpInfo = new DumpInfo(graph.GetElementName);
            
            // Next-Transitions shall be black
            dumpInfo.SetEdgeTypeColor(de.unika.ipd.grGen.Model_Mission.Next.TypeInstance, GrColor.Black);
            // Do not display labels for Next-Transitions
            dumpInfo.SetElemTypeLabel(de.unika.ipd.grGen.Model_Mission.Next.TypeInstance, "");
            
            // Tasks shall be light blue
            dumpInfo.SetNodeTypeColor(Task.TypeInstance, GrColor.LightBlue);
            dumpInfo.SetNodeTypeBorderColor(Task.TypeInstance, GrColor.DarkBlue);
            
            // Locks shall be red hexagons
            dumpInfo.SetNodeTypeColor(Lock.TypeInstance, GrColor.LightRed);
            dumpInfo.SetNodeTypeBorderColor(Lock.TypeInstance, GrColor.DarkRed);
            dumpInfo.SetNodeTypeShape(Lock.TypeInstance, GrNodeShape.Hexagon);
            
            // Keys shall be green circles
            dumpInfo.SetNodeTypeColor(de.unika.ipd.grGen.Model_Mission.Key.TypeInstance, GrColor.LightGreen);
            dumpInfo.SetNodeTypeBorderColor(de.unika.ipd.grGen.Model_Mission.Key.TypeInstance, GrColor.DarkGreen);
            dumpInfo.SetNodeTypeShape(de.unika.ipd.grGen.Model_Mission.Key.TypeInstance, GrNodeShape.Circle);
            
            // BelongsTo-edges of keys shall be dashed and grey
            dumpInfo.SetEdgeTypeColor(de.unika.ipd.grGen.Model_Mission.BelongsTo.TypeInstance, GrColor.Grey);
            dumpInfo.SetEdgeTypeLineStyle(de.unika.ipd.grGen.Model_Mission.BelongsTo.TypeInstance, GrLineStyle.Dashed);

            return dumpInfo;
        }

        public void Refresh()
        {
            client.UpdateDisplay();
            client.Sync();
        }

        public void Dispose()
        {
            client.UnregisterLibGrEvents();
        }
    }
}