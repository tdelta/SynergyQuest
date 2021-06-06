import React from "react";
import {GraphData, TreeGraphData} from "@antv/g6/lib/types";
import {nGraphFromG6} from "./nGraphFromG6";
import GGEditor, {EditableLabel, Flow, Item, ItemPanel} from "gg-editor";
import {g6FromNGraph} from "./g6FromNGraph";
import {Saver} from "./Saver";
import {LinkData, NodeData} from "../App";
import {Graph} from 'ngraph.graph'

interface GraphEditorProps {
    graph: Graph<NodeData, LinkData>,
    isRule?: boolean,
    onGraphEdited?: (graph: Graph<NodeData, LinkData>) => unknown
    onGraphUploaded?: (graph: Graph<NodeData, LinkData>) => unknown
}

export class GraphEditor extends React.Component<GraphEditorProps, {}> {
    constructor(props: GraphEditorProps) {
        super(props);

        this.handleGraphSaved = this.handleGraphSaved.bind(this);
        this.handleUploadedGraph = this.handleUploadedGraph.bind(this);
    }

    private handleGraphSaved(g6Graph: GraphData | TreeGraphData) {
        const isRule = this.props.isRule || false;
        const nGraph = nGraphFromG6(g6Graph as GraphData, isRule)

        nGraph.forEachNode(node => {
            console.log(node)
        })

        if (this.props.onGraphEdited != null) {
            this.props.onGraphEdited(nGraph);
        }
    }

    private handleUploadedGraph(graph: Graph<NodeData, LinkData>) {
        if (this.props.onGraphUploaded != null) {
            this.props.onGraphUploaded(graph)
        }
    }

    render() {
        const isRule = this.props.isRule || false;

        return (
            <div
                style={{
                    width: "100%",
                    height: "100%",
                    display: "flex"
                }}
            >
                <GGEditor
                    style={{
                        display: "flex",
                        flex: 1
                    }}
                >
                    <ItemPanel style={{width: "2vw"}}>
                        <Item
                            model={{
                                type: 'circle',
                                size: 50,
                                label: 'λ:N'
                            }}
                        >
                            <div draggable={true}>Node</div>
                        </Item>
                    </ItemPanel>
                    <Flow
                        style={{
                            display: "flex",
                            width: "15vw"
                        }}
                        data={g6FromNGraph(this.props.graph, isRule)}
                        graphConfig={{
                            defaultNode: {
                                shape: "bizExNode"
                            },
                            defaultEdge: {
                                shape: "bizEdge"
                            }
                        }}
                    />
                    <EditableLabel/>
                    <Saver onGraphChanged={this.handleGraphSaved}
                           onGraphUploaded={this.props.onGraphUploaded != null ? this.handleUploadedGraph : undefined}/>
                </GGEditor>
            </div>
        );
    }
}