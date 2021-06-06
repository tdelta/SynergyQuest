declare module "subgraph-isomorphism" {
    export function getIsomorphicSubgraphs(
        hostGraph: number[][],
        matchingGraph: number[][],
        maxMatches: number,
        similarity: (P: number[][], G: number[][], p: number, g: number) => boolean
    ): number[][][]
}

declare module "ngraph.fromdot" {
    import { Graph } from 'ngraph.graph';

    export default function fromDot<NodeData, LinkData>(dotEncoding: string): Graph<NodeData, LinkData>;
}

declare module "ngraph.todot" {
    import { Graph } from 'ngraph.graph';

    export default function toDot<NodeData, LinkData>(g: Graph<NodeData, LinkData>): string;
}

declare module "ngraph.fromjson" {
    import { Graph } from 'ngraph.graph';

    export default function fromJson<NodeData, LinkData>(jsonEncoding: string): Graph<NodeData, LinkData>;
}

declare module "ngraph.tojson" {
    import { Graph } from 'ngraph.graph';

    export default function toJson<NodeData, LinkData>(g: Graph<NodeData, LinkData>): string;
}

