import React from 'react';
import './App.css';
import 'litegraph.js/css/litegraph.css';
// import RuleEditor from "./RuleEditor";
import './graph-editor/bizEdge'
import './graph-editor/bizExNode'
import createGraph, {Graph, Link, Node, NodeId} from 'ngraph.graph';
import findSubgraphIsomorphisms from "./graphtools/subgraphIsomorphism";
import {GraphEditor} from "./graph-editor/GraphEditor";
import toJson from "ngraph.tojson";
import {EdgeConfig, NodeConfig } from '@antv/g6';

export interface NodeData {
    type: string;
    ruleId?: number;
    g6NodeData: NodeConfig
}

export interface LinkData {
    ruleId?: number;
    g6EdgeConfig: EdgeConfig
}

function toRuleGraph(g: Graph<NodeData, LinkData>) {
    const seenIds = new Set<number>();
    const nodeIdsToRuleIds = new Map<NodeId, number>();

    const renamedNodes: {id: number, data: NodeData}[] = []
    g.forEachNode(node => {
        if (node.data.ruleId == null) {
            throw Error("Rule has node without rule id.")
        }

        else if (seenIds.has(node.data.ruleId)) {
            throw Error("Rule has duplicate id.")
        }

        seenIds.add(node.data.ruleId)
        nodeIdsToRuleIds.set(node.id, node.data.ruleId);

        renamedNodes.push({
            id: node.data.ruleId,
            data: node.data
        })
    })

    const renamedEdges: {from: number, to: number, data: LinkData}[] = []
    g.forEachLink(link => {
        renamedEdges.push({
            from: nodeIdsToRuleIds.get(link.fromId)!,
            to: nodeIdsToRuleIds.get(link.toId)!,
            data: link.data
        })
    })

    const result = createGraph()
    for (const renamedNode of renamedNodes) {
        result.addNode(renamedNode.id, renamedNode.data)
    }

    for (const renamedEdge of renamedEdges) {
        result.addLink(renamedEdge.from, renamedEdge.to, renamedEdge.data)
    }

    return result;
}

function nGraphToObject(g: Graph<NodeData, LinkData>): object {
    return JSON.parse(toJson(g))
}

function applyRule(
    hostGraph: Graph<NodeData, LinkData>,
    RuleL: Graph<NodeData, LinkData>,
    RuleR: Graph<NodeData, LinkData>,
    maxMatches: number = 1
): Graph<NodeData, LinkData>[] {
    function dataMatcher(lhs: NodeData, rhs: NodeData): boolean {
        return lhs.type === rhs.type;
    }

    const morphisms = findSubgraphIsomorphisms(
        hostGraph,
        RuleL,
        maxMatches,
        dataMatcher
    )

    function genNodeLabelFun(g: Graph<NodeData, LinkData>): (nodeId: NodeId) => number | undefined {
        function lVG(nodeId: NodeId): number | undefined {
            const node = g.getNode(nodeId)

            return node?.data.ruleId
        }

        return lVG;
    }

    function genEdgeLabelFun(g: Graph<NodeData, LinkData>): (from: NodeId, to: NodeId) => number | undefined {
        function lEG(from: NodeId, to: NodeId): number | undefined {
            let edge = g.getLink(from, to);
            if (edge == null) {
                edge = g.getLink(to, from);
            }

            return edge?.data.ruleId
        }

        return lEG;
    }

    const lVL = genNodeLabelFun(RuleL)
    const lVR = genNodeLabelFun(RuleR)
    const lEL = genEdgeLabelFun(RuleL)
    const lER = genEdgeLabelFun(RuleR)

    const results: Graph<NodeData, LinkData>[] = []
    for (const morphism of morphisms) {
        const copy = deepCopyGraph(hostGraph)

        const getHostLink = (lEdge: Link<LinkData>) => {
            const fromHostId = morphism.get(lEdge.fromId)!
            const toHostId = morphism.get(lEdge.toId)!

            let hostLink = copy.getLink(fromHostId, toHostId)
            if (hostLink == null) {
                hostLink = copy.getLink(toHostId, fromHostId)
            }

            return hostLink!;
        }

        // Step 1
        RuleL.forEachNode(lNode => {
            if (lVR(lNode.id) == null) {
                const hostId = morphism.get(lNode.id)!

                const edgesToRemove = copy.getLinks(hostId)
                edgesToRemove?.forEach(edgeToRemove => {
                    copy.removeLink(getHostLink(edgeToRemove))
                })

                copy.removeNode(hostId)
            }
        })

        RuleL.forEachLink(lEdge => {
            if (lER(lEdge.fromId, lEdge.toId) == null) {
                const linkToRemove = getHostLink(lEdge)
                console.log(`Removing:`)
                console.log(linkToRemove)

                copy.removeLink(linkToRemove)
            }
        })

        // Step 2
        RuleL.forEachNode(lNode => {
            const rNode = RuleR.getNode(lNode.id)

            if (rNode != null) {
                const hostId = morphism.get(lNode.id)!

                const hostNode = copy.getNode(hostId)!

                hostNode.data.type = rNode.data.type;
            }
        })

        // Step 3
        RuleR.forEachNode(rNode => {
            if (lVL(rNode.id) == null) {
                copy.addNode(rNode.id, rNode.data)
            }
        })

        RuleR.forEachLink(rEdge => {
            if (lEL(rEdge.fromId, rEdge.toId) == null) {
                const fromHostId = morphism.get(rEdge.fromId) || rEdge.fromId
                const toHostId = morphism.get(rEdge.toId) || rEdge.toId

                copy.addLink(fromHostId, toHostId, rEdge.data)
            }
        })

        results.push(copy)
    }

    return results
}

function deepCopyGraph(g: Graph<NodeData, LinkData>): Graph<NodeData, LinkData> {
    const copy = createGraph()

    g.forEachNode((node: Node<NodeData>) => {
        copy.addNode(node.id, node.data)
    })

    g.forEachLink(link => copy.addLink(link.fromId, link.toId, link.data))

    return copy;
}

class App extends React.Component<AppProps, AppState> {
    //ruleEditors: React.RefObject<RuleEditor>[] = [];
    constructor(props: AppProps) {
        super(props);

        this.state = {
            graph: createGraph(),
            ruleL: createGraph(),
            ruleR: createGraph(),
            result: createGraph(),
            ruleApplicationCount: 1
        }

        this.subGraphIsomorphismTest = this.subGraphIsomorphismTest.bind(this);
        this.applyRule = this.applyRule.bind(this);
    }

    private applyRule() {
        const L = toRuleGraph(this.state.ruleL)
        const R = toRuleGraph(this.state.ruleR)

        function applyNTimes(hostGraph: Graph<NodeData, LinkData>, n: number): Graph<NodeData, LinkData> {
            if (n <= 0) {
                return hostGraph;
            }

            const results = applyRule(
                hostGraph, L, R
            )

            let result = hostGraph;
            if (results.length > 0) {
                result = results[0]
            }

            return applyNTimes(result, n-1)
        }

        const result = applyNTimes(this.state.graph, this.state.ruleApplicationCount)

        this.setState({result: result})
    }

    private subGraphIsomorphismTest() {
        function dataMatcher(lhs: NodeData, rhs: NodeData): boolean {
            return lhs.type === rhs.type;
        }

        const morphisms = findSubgraphIsomorphisms(
            this.state.graph,
            this.state.ruleL,
            1,
            dataMatcher
        )

        if (morphisms.length > 0) {
            const morphism = morphisms[0]

            console.log(Array.from(morphism.entries()))
        }
        console.log(`Is there a morphism? ${morphisms.length > 0}`)
    }

    render() {
        return (<>
            <div style={{display: "flex", width: "100%", height: "90%"}}>
                <div style={{flex: 1}}>
                    <GraphEditor graph={this.state.graph} onGraphEdited={graph => this.setState({graph: graph})} onGraphUploaded={graph => this.setState({graph: graph})}/>
                </div>
                <div style={{flex: 1}}>
                    <GraphEditor graph={this.state.ruleL} isRule={true} onGraphEdited={graph => this.setState({ruleL: graph})} onGraphUploaded={graph => this.setState({ruleL: graph})}/>
                </div>
                <div style={{flex: 1}}>
                    <GraphEditor graph={this.state.ruleR} isRule={true} onGraphEdited={graph => this.setState({ruleR: graph})} onGraphUploaded={graph => this.setState({ruleR: graph})}/>
                </div>
                <div style={{flex: 1}}>
                    <GraphEditor graph={this.state.result} onGraphEdited={graph => this.setState({result: graph})}/>
                </div>
            </div>
            <button onClick={this.subGraphIsomorphismTest}>L isomorphic to subgraph of G?</button>
            <input type="number" value={this.state.ruleApplicationCount} onChange={e => this.setState({ruleApplicationCount: parseInt(e.target.value)})}/>
            <button onClick={this.applyRule}>Rule!</button>
        </>)
    }
}

interface AppProps {}
interface AppState {
    graph: Graph<NodeData, LinkData>
    ruleL: Graph<NodeData, LinkData>
    ruleR: Graph<NodeData, LinkData>
    result: Graph<NodeData, LinkData>
    ruleApplicationCount: number
}

export default App;
