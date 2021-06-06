import { Graph, Node, Link, NodeId } from "ngraph.graph";
import { getIsomorphicSubgraphs } from 'subgraph-isomorphism';

type Morphism = Map<NodeId, NodeId>

type AdjacencyMatrix = number[][]

function toAdjacencyMatrix<NodeData, LinkData>(g: Graph<NodeData, LinkData>): [AdjacencyMatrix, Map<number, NodeId>, Map<NodeId, number>] {
    const matrix = new Array<Array<number>>(g.getNodeCount());
    for (let i = 0; i < matrix.length; ++i) {
        matrix[i] = new Array<number>(matrix.length).fill(0)
    }

    const idToIdxMapping = new Map<NodeId, number>();
    let maxIdxSeen = 0

    function getIdx(nodeId: NodeId): number {
        const savedIdx = idToIdxMapping.get(nodeId)

        if (savedIdx != null) {
            return savedIdx
        }

        else {
            const idx = maxIdxSeen
            idToIdxMapping.set(nodeId, idx)

            maxIdxSeen += 1;

            return idx;
        }
    }

    g.forEachNode(node => {
        getIdx(node.id)
    })

    g.forEachLink((link: Link<LinkData>) => {
        const fromIdx = getIdx(link.fromId)
        const toIdx = getIdx(link.toId)

        matrix[fromIdx][toIdx] = 1
        matrix[toIdx][fromIdx] = 1
    })

    const idxToIdMapping = new Map<number, NodeId>(Array.from<any, any>(idToIdxMapping, a => a.reverse()))

    return [matrix, idxToIdMapping, idToIdxMapping]
}

function initializeMetaDataArrays<NodeData, LinkData>(g: Graph, idxMapping: Map<NodeId, number>): [Array<NodeData>, Array<number>] {
    const nodeDataArray = Array<NodeData>(g.getNodeCount())
    const nodeDegreeArray = Array<number>(g.getNodeCount())

    g.forEachNode((node: Node<NodeData>) => {
        const nodeIdx = idxMapping.get(node.id)!

        nodeDataArray[nodeIdx] = node.data
        nodeDegreeArray[nodeIdx] = (node.links || []).length
    })

    return [nodeDataArray, nodeDegreeArray]
}

function convertMorphism(rawMorphism: number[][], hostIdxToIdMapping: Map<number, NodeId>, matchingIdxToIdMapping: Map<number, NodeId>): Morphism {
    const result = new Map<NodeId, NodeId>();

    for (let matchingIdx = 0; matchingIdx < rawMorphism.length; ++matchingIdx) {
        const hostIdentities = rawMorphism[matchingIdx]

        for (let hostIdx = 0; hostIdx < hostIdentities.length; ++hostIdx) {
            if (hostIdentities[hostIdx] > 0) {
                const matchingId = matchingIdxToIdMapping.get(matchingIdx)!
                const hostId = hostIdxToIdMapping.get(hostIdx)!

                result.set(matchingId, hostId)

                break;
            }
        }
    }

    return result;
}

export default function findSubgraphIsomorphisms<NodeData, LinkData>(
    hostGraph: Graph<NodeData, LinkData>,
    matchingGraph: Graph<NodeData, LinkData>,
    maxMatches: number,
    dataMatcher: (hostNode: NodeData, matchingNode: NodeData) => boolean = (lhs, rhs) => lhs === rhs
): Morphism[] {
    if (matchingGraph.getNodeCount() === 0) {
        return [new Map<NodeId, NodeId>()]
    }
    else if (hostGraph.getNodeCount() === 0) {
        return [];
    }

    const [hostAdjacencyM, hostIdxToIdMapping, hostIdToIdxMapping] = toAdjacencyMatrix<NodeData, LinkData>(hostGraph)
    const [matchingAdjacencyM, matchingIdxToIdMapping, matchingIdToIdxMapping] = toAdjacencyMatrix<NodeData, LinkData>(matchingGraph)

    const [hostNodeData, hostDegrees] = initializeMetaDataArrays<NodeData, LinkData>(hostGraph, hostIdToIdxMapping)
    const [matchingNodeData, matchingDegrees] = initializeMetaDataArrays<NodeData, LinkData>(matchingGraph, matchingIdToIdxMapping)

    function similarity(P: AdjacencyMatrix, G: AdjacencyMatrix, p: number, g: number): boolean {
        const p_i_deg = matchingDegrees[p];
        const g_j_deg = hostDegrees[g];

        const p_i_data = matchingNodeData[p];
        const g_j_data = hostNodeData[g];

        return (p_i_deg <= g_j_deg) && dataMatcher(g_j_data, p_i_data);
    }

    const rawMorphisms: (number[][])[] = getIsomorphicSubgraphs(hostAdjacencyM, matchingAdjacencyM, maxMatches, similarity)

    return rawMorphisms.map(rawMorphism => convertMorphism(rawMorphism, hostIdxToIdMapping, matchingIdxToIdMapping))
}