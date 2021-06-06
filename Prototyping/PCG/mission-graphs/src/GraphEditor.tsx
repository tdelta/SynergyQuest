export {}
//import React, {CSSProperties} from "react"; import Fab from '@material-ui/core/Fab';
//import AddIcon from '@material-ui/icons/Add';
//import {LGraph, LGraphCanvas, LGraphNode, LiteGraph} from "litegraph.js";
//import {GrammarNode} from "./NodeTypes/GrammarNode";
//import Graph from "./Graph";
//
//export default class GraphEditor extends React.Component<GraphEditorProps, GraphEditorState> {
//    rawCanvasRef = React.createRef<HTMLCanvasElement>()
//
//    graph?: LGraph
//
//    constructor(props: GraphEditorProps) {
//        super(props);
//
//        this.handleAdd = this.handleAdd.bind(this)
//    }
//
//    public getGraph(): Graph | undefined {
//        if (this.graph != null) {
//            const typedNodes = (this.graph as any)._nodes as LGraphNode[]
//
//            const nodeToIdx = new Map<LGraphNode, number>()
//            for (let idx = 0; idx < typedNodes.length; ++idx) {
//                nodeToIdx.set(typedNodes[idx], idx)
//            }
//
//            return {
//                node: typedNodes.map(node => node.properties.kind as string),
//                edge: typedNodes.flatMap((node, idx) => node.inputs.map((_, inputSlotIdx) => {
//                    let inputNode = node.getInputNode(inputSlotIdx)
//
//                    return [idx, nodeToIdx.get(inputNode!!)!!] as [number, number]
//                }))
//            }
//        }
//
//        return undefined;
//    }
//
//    handleAdd() {
//        if (this.graph != null) {
//            const node = LiteGraph.createNode(GrammarNode.type);
//            node.pos = [100, 100];
//
//            this.graph.add(node);
//        }
//    }
//
//    render() {
//        return (
//            <div style={{display: 'inline-block', ...this.props.style, position: 'relative'}}>
//                <canvas ref={this.rawCanvasRef} style={{width: '100%', height: '100%'}}/>
//                <Fab color="primary" aria-label="add" style={{position: 'absolute', right: '25px', bottom: '30px'}} onClick={this.handleAdd}>
//                    <AddIcon />
//                </Fab>
//            </div>
//        );
//    }
//
//    componentDidMount() {
//        this.graph = new LGraph();
//        const canvas = new LGraphCanvas(this.rawCanvasRef.current!!, this.graph);
//        canvas.resize()
//
//        this.graph.start()
//    }
//}
//
//interface GraphEditorState {}
//interface GraphEditorProps {
//    style?: CSSProperties
//}