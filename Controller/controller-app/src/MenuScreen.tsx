import React from 'react';
import { MenuAction } from 'controller-client-lib';
import './ConnectScreen.css';
import { ReactComponent as Logo } from './gfx/logo_web.svg';
import { menuActionStrings } from './EnumStrings';

export class MenuScreen extends React.Component<
  MenuScreenProbs,
  MenuScreenState
> {
  render() {
    let scrollContent = <>Please follow the instructions on the game screen</>;
    if (this.props.enabledMenuActions.size > 0) {
      const buttonList = Array.from(this.props.enabledMenuActions).map(
        action => {
          const click = (_: any) => this.props.triggerMenuAction(action);

          return (
            <>
              <button className='pixelbutton' onClick={click}>
                {menuActionStrings.get(action)}
              </button>
              <br />
            </>
          );
        }
      );

      scrollContent = (
        <div style={{ padding: '2em', textAlign: 'center' }}>{buttonList}</div>
      );
    }

    return (
      <div className='container'>
        <div className='columnContainer'>
          <Logo id='logo' />
          <div id='contentContainer'>
            <div className='scroll'>{scrollContent}</div>
          </div>
        </div>
      </div>
    );
  }
}

interface MenuScreenState {}

interface MenuScreenProbs {
  enabledMenuActions: Set<MenuAction>;
  triggerMenuAction: (action: MenuAction) => void;
}
