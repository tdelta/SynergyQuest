import React, { CSSProperties } from 'react';

import './SpeechBubble.css';

export class SpeechBubble extends React.Component<Props, State> {
  render() {
    const target = this.props.target.current;
    if (target != null) {
      const right =
        document.documentElement.clientWidth -
        target.offsetLeft -
        target.offsetWidth / 2;
      const top = target.offsetTop + target.offsetHeight / 2;

      return (
        <>
          <div
            className='speechBubble'
            style={{
              ...this.props.style,
              position: 'absolute',
              right: right,
              top: top,
            }}
          >
            {this.props.children}
          </div>
        </>
      );
    } else {
      throw new Error('Target does not exist.');
    }
  }
}

interface Props {
  style: CSSProperties;
  target: React.RefObject<HTMLElement>;
}

interface State {}
