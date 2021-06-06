export {}
//import React from "react";
//import GraphEditor from "./GraphEditor";
//import ForwardIcon from '@material-ui/icons/Forward';
//import Rule from "./Rule";
//
//class RuleEditor extends React.Component<RuleEditorProps, RuleEditorState> {
//    inputGraph = React.createRef<GraphEditor>()
//    outputGraph = React.createRef<GraphEditor>()
//
//    public getRule(): Rule | undefined {
//        let lhs = this.inputGraph.current?.getGraph();
//        let rhs = this.outputGraph.current?.getGraph();
//
//        if (lhs != null && rhs != null) {
//            return {
//                lhs: lhs,
//                rhs: rhs
//            }
//        }
//
//        return undefined
//    }
//
//    render() {
//        return (
//            <div style={{display: 'flex', alignItems: 'center'}}>
//                <GraphEditor ref={this.inputGraph} style={{width: '35vw'}}/>
//                <ForwardIcon style={{width: '20px'}}/>
//                <GraphEditor ref={this.outputGraph} style={{width: '35vw'}}/>
//            </div>
//        )
//    }
//}
//
//interface RuleEditorProps {}
//interface RuleEditorState {}
//
//export default RuleEditor;