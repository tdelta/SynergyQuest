export {}

//import createGraph, { Graph } from 'ngraph.graph'
//
//
//type GraphMappingPair = [number, number]
//type GraphMapping = GraphMappingPair[]
//
//class State {
//    private static readonly NULL_NODE: number = -1;
//
//    public readonly g1: Graph;
//    public readonly g2: Graph;
//
//    public readonly mapping: GraphMapping;
//
//    private core_1: number[]
//    private core_2: number[]
//
//    private in_1: number[]
//    private out_1: number[]
//
//    private in_2: number[]
//    private out_2: number[]
//
//    constructor(g1: Graph, g2: Graph, mapping: GraphMapping = []) {
//        this.g1 = g1;
//        this.g2 = g2;
//
//        this.mapping = mapping;
//
//        this.core_1 = new Array<number>(this.g1.getNodeCount()).fill(State.NULL_NODE)
//        this.core_2 = new Array<number>(this.g2.getNodeCount()).fill(State.NULL_NODE)
//
//        this.in_1 = new Array<number>(this.g1.getNodeCount()).fill(0)
//        this.out_1 = new Array<number>(this.g1.getNodeCount()).fill(0)
//
//        this.in_2 = new Array<number>(this.g2.getNodeCount()).fill(0)
//        this.out_2 = new Array<number>(this.g2.getNodeCount()).fill(0)
//    }
//}
//
//const emptyState = null
//
//function M(s: State): GraphMapping {
//    return s.mapping;
//}
//
//function P(s: State): GraphMapping {
//    const g = createGraph()
//}
//
//// F_syn(s, n, m)
//function isSyntacticallyFeasible(s: State, candidate: GraphMappingPair): boolean {
//
//}
//
//// F_sem(s, n, m)
//function isSemanticallyFeasible(s: State, candidate: GraphMappingPair): boolean {
//
//}
//
//// F(s, n, m)
//function isFeasible(s: State, candidate: GraphMappingPair): boolean {
//    return isSyntacticallyFeasible(s, candidate) && isSemanticallyFeasible(s, candidate)
//}
//
//function doesItCoverNodes(m: GraphMapping, g: Graph): boolean {
//
//}
//
//function extendState(s: State, candidate: GraphMappingPair): State {
//    const extendedMapping = s.mapping.slice()
//    extendedMapping.push(candidate)
//
//    return new State(extendedMapping)
//}
//
//export default function Match(
//    g1: Graph,
//    g2: Graph,
//    s: State = new State(),
//): GraphMapping | null {
//    if (doesItCoverNodes(M(s), g2)) {
//        return M(s)
//    }
//
//    else {
//        const candidates = P(s)
//
//        for (const candidate of candidates) {
//            if (isFeasible(s, candidate)) {
//                const newS = extendState(s)
//
//                const maybeMatch = Match(g1, g2, newS)
//                if (maybeMatch != null) {
//                    return maybeMatch
//                }
//            }
//        }
//
//        // TODO restore data structures
//
//        return null
//    }
//}