export {}
//import {
//    INodeInputSlot,
//    INodeOutputSlot,
//    IWidget,
//    LGraphCanvas,
//    LGraphNode,
//    LiteGraph,
//    LLink,
//    Vector2
//} from "litegraph.js";
//import Graph from "../Graph";
//
//export class GrammarNode extends LGraphNode {
//    static type = "grammar/main"
//    static title = "GrammarNode"
//
//    public properties = { kind: "T" }
//
//    constructor(title?: string) {
//        super(title);
//
//        this.addWidget("text", "kind", this.properties.kind, GrammarNode.setKind)
//
//        this.addInput("","grammar/main");
//        this.addOutput("","grammar/main");
//    }
//
//    private static setKind(
//        this: IWidget<any, any>,
//        value: string,
//        _graphCanvas: LGraphCanvas,
//        node: LGraphNode,
//        _pos: Vector2,
//        _event?: MouseEvent
//    ) {
//        node.properties.kind = value
//        console.log(node.properties.kind)
//    }
//
//    onConnectionsChange(type: number, slotIndex: number, isConnected: boolean, link: LLink, ioSlot: INodeOutputSlot | INodeInputSlot) {
//        // Ensure there is always exactly one free input
//
//        let freeInputs = 0;
//        for (let i = 0; i < this.inputs.length; ) {
//            if (!this.isInputConnected(i)) {
//                if (freeInputs > 0) {
//                    this.removeInput(i)
//                    continue
//                }
//
//                else {
//                    ++freeInputs
//                }
//            }
//
//            ++i
//        }
//
//        if (freeInputs === 0) {
//            this.addInput("", "grammar/main")
//        }
//
//        return true;
//    }
//}
//
////register in the system
//LiteGraph.registerNodeType(GrammarNode.type, GrammarNode);