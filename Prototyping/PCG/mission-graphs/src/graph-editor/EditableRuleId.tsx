export {}
/*
import React from 'react';
import ReactDOM from 'react-dom';
import G6 from '@antv/g6';
import global from 'gg-editor';
import { constants } from 'gg-editor';
import { Util, withEditorContext } from 'gg-editor';
import {EditorContextProps} from "gg-editor/lib/components/EditorContext";
import {LabelStateEvent} from "gg-editor/lib/common/interfaces";

const { GraphMode, EditorEvent, GraphNodeEvent, LabelState } = constants;
const { isMind, getSelectedNodes } = Util;

interface EditableRuleIdProps extends EditorContextProps {
    ruleIdClassName?: string;
    ruleIdMaxWidth?: number;
}

interface EditableRuleIdState {
    visible: boolean;
}

class EditableRuleId extends React.Component<EditableRuleIdProps, EditableRuleIdState> {
    el: HTMLDivElement | null = null;

    static defaultProps = {
        labelClassName: 'node-rule-id',
        labelMaxWidth: 100,
    };

    state = {
        visible: false,
    };

    componentDidMount() {
        const graph = this.props.graph!;

        graph.on(EditorEvent.onLabelStateChange, ({ labelState }: LabelStateEvent) => {
            if (labelState === LabelState.Show) {
                this.showEditableLabel();
            } else {
                this.hideEditableLabel();
            }
        });

        graph.on(GraphNodeEvent.onNodeDoubleClick, () => {
            this.showEditableLabel();
        });
    }

    update = () => {
        const { executeCommand } = this.props;
        const graph = this.props.graph!;

        const node = getSelectedNodes(graph)[0];
        const model = node.getModel();

        const { textContent: label } = this.el!;

        if (label === model.label) {
            return;
        }

        executeCommand('update', {
            id: model.id,
            updateModel: {
                label,
            },
            forceRefreshLayout: isMind(graph),
        });
    };

    showEditableLabel = () => {
        (global as any).plugin.editableLabel.state = 'show';

        this.setState(
            {
                visible: true,
            },
            () => {
                const { el } = this;

                if (el) {
                    el.focus();
                    document.execCommand('selectAll', false, undefined);
                }
            },
        );
    };

    hideEditableLabel = () => {
        (global as any).plugin.editableLabel.state = 'hide';

        this.setState({
            visible: false,
        });
    };

    handleBlur = () => {
        this.update();
        this.hideEditableLabel();
    };

    handleKeyDown = (e: React.KeyboardEvent<HTMLDivElement>) => {
        e.stopPropagation();

        const { key } = e;

        if (['Tab'].includes(key)) {
            e.preventDefault();
        }

        if (['Enter', 'Escape', 'Tab'].includes(key)) {
            this.update();
            this.hideEditableLabel();
        }
    };

    render() {
        const { labelClassName, labelMaxWidth } = this.props;
        const graph = this.props.graph!;

        const mode = graph.getCurrentMode();
        const zoom = graph.getZoom();

        if (mode === GraphMode.Readonly) {
            return null;
        }

        const node = getSelectedNodes(graph)[0];

        if (!node) {
            return null;
        }

        const model = node.getModel();
        const group = node.getContainer();

        const label = model.label;
        const labelShape = group.findByClassName(labelClassName!);

        if (!labelShape) {
            return null;
        }

        const { visible } = this.state;

        if (!visible) {
            return null;
        }

        // Get the label offset
        const { x: relativeX, y: relativeY } = labelShape.getBBox();
        const { x: absoluteX, y: absoluteY } = G6.Util.applyMatrix(
            {
                x: relativeX,
                y: relativeY,
            },
            node.getContainer().getMatrix(),
        );

        const { x: left, y: top } = graph.getCanvasByPoint(absoluteX, absoluteY);

        // Get the label size
        const { width, height } = labelShape.getBBox();

        // Get the label font
        const font = labelShape.attr('font');

        const style: React.CSSProperties = {
            position: 'absolute',
            top,
            left,
            width: 'auto',
            height: 'auto',
            minWidth: width,
            minHeight: height,
            maxWidth: labelMaxWidth,
            font,
            background: 'white',
            border: '1px solid #1890ff',
            outline: 'none',
            transform: `scale(${zoom})`,
            transformOrigin: 'left top',
        };

        return ReactDOM.createPortal(
            <div
                ref={el => {
                    this.el = el;
                }}
                style={style}
                contentEditable
                onBlur={this.handleBlur}
                onKeyDown={this.handleKeyDown}
                suppressContentEditableWarning
            >
                {label}
            </div>,
            graph.get('container'),
        );
    }
}

export default withEditorContext<EditableRuleIdProps>(EditableRuleId);


 */