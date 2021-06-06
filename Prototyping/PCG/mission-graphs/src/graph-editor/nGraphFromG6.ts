import {GraphData} from "@antv/g6/lib/types";
import {LinkData, NodeData} from "../App";

import createGraph, {Graph} from 'ngraph.graph';

export function nGraphFromG6(g6Graph: GraphData, useRuleIds: boolean = true): Graph<NodeData, LinkData> {
    const g6Nodes = g6Graph.nodes || [];

    const nGraph: Graph<NodeData, LinkData> = createGraph()

    for (const node of g6Nodes) {
        let rawLabel = "Unlabeled"
        if (node.label != null) {
            if (typeof node.label === 'string') {
                rawLabel = node.label
            } else if (node.label.text != null) {
                rawLabel = node.label.text
            }
        }

        const split = rawLabel.split(":")
        const type = split[split.length - 1]
        let ruleId: number | undefined = undefined;
        if (useRuleIds && split.length > 1) {
            if (split[0] !== 'λ') {
                ruleId = parseInt(split[0])

                if (isNaN(ruleId)) {
                    ruleId = undefined;
                }
            }
        }

        nGraph.addNode(node.id, {
            type: type,
            ruleId: ruleId,
            g6NodeData: node
        })
    }

    const g6Edges = g6Graph.edges || []
    for (const edge of g6Edges) {
        nGraph.addLink(edge.source!, edge.target!, {ruleId: edge.ruleId as number | undefined, g6EdgeConfig: edge})
    }

    return nGraph;
}