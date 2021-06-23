import {LinkData, NodeData} from "../App";

import {Graph} from 'ngraph.graph';
import {EdgeConfig, GraphData, NodeConfig} from "@antv/algorithm/lib/types";

export function g6FromNGraph(nGraph: Graph<NodeData, LinkData>, useRuleIds: boolean = true): GraphData {
    const nodes: NodeConfig[] = []

    nGraph.forEachNode(node => {
        let label: string = node.data.type;
        if (useRuleIds) {
            label = `${node.data.ruleId || 'λ'}:${node.data.type}`
        }

        nodes.push({
            ...node.data.g6NodeData,
            label: label
        });
    })

    const links: EdgeConfig[] = [];

    nGraph.forEachLink(link => {
        links.push({
            ...link.data.g6EdgeConfig,
            source: link.fromId.toString(),
            target: link.toId.toString()
        })
    })

    return {
        nodes: nodes,
        edges: links,
    }
}