import {EditorContextProps} from "gg-editor/lib/components/EditorContext";
import {GraphData, TreeGraphData} from "@antv/g6/lib/types";
import React from "react";
import {withEditorContext} from "gg-editor";
import { saveAs } from 'file-saver'
import fromJson from 'ngraph.fromjson';
import toJson from 'ngraph.tojson';
import {LinkData, NodeData} from "../App";
import { Graph } from "ngraph.graph";
import {nGraphFromG6} from "./nGraphFromG6";
import {fileDialog} from "file-select-dialog";

interface SaverProps extends EditorContextProps {
    onGraphChanged?: (g6Graph: GraphData | TreeGraphData) => unknown;
    onGraphUploaded?: (nGraph: Graph<NodeData, LinkData>) => unknown;
}

class SaverRaw extends React.Component<SaverProps, any> {
    constructor(props: SaverProps) {
        super(props);

        this.handleGraphChange = this.handleGraphChange.bind(this);
        this.downloadGraph = this.downloadGraph.bind(this);
        this.uploadGraph = this.uploadGraph.bind(this);
        this.inspect = this.inspect.bind(this);

        this.props.graph?.on('onAfterExecuteCommand', this.handleGraphChange)
        this.props.graph?.on('afterconnect', this.handleGraphChange)
    }

    inspect() {
        console.log(JSON.parse(toJson(nGraphFromG6(this.props.graph?.save()! as GraphData))))
    }

    handleGraphChange(): GraphData {
        const graph = this.props.graph!.save();

        if (this.props.onGraphChanged != null) {
            this.props.onGraphChanged(graph)
        }

        return graph as GraphData;
    }

    async uploadGraph() {
        if (this.props.onGraphUploaded != null) {
            const fileList = await fileDialog({accept: '.json'})

            if (fileList.length > 0) {
                const file = fileList[0];

                const dotEncoding = await file.text()

                const nGraph = fromJson<NodeData, LinkData>(dotEncoding)

                this.props.onGraphUploaded(nGraph)
            }
        }
    }

    downloadGraph() {
        const g6Graph = this.handleGraphChange()
        const nGraph = nGraphFromG6(g6Graph)

        const dotEncoding = toJson(nGraph)

        const blob = new Blob([dotEncoding], {type: "text/plain;charset=utf-8"})

        saveAs(blob, "graph.json")
    }

    render() {
        return <div
            style={{
                flex: 1
            }}
        >
            {
                this.props.onGraphUploaded != null && <button onClick={this.uploadGraph}>Load</button>
            }
            <button onClick={this.downloadGraph}>Save</button>
            <button onClick={this.inspect}>Inspect</button>
        </div>;
    }
}

export const Saver = withEditorContext(SaverRaw);