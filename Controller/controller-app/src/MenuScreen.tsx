import React from 'react';
import { MenuAction } from 'controller-client-lib';
import './ConnectScreen.css';
import { ReactComponent as Logo } from './gfx/logo_web.svg';
import { menuActionStrings } from './EnumStrings';

/**
 * Menu actions are always displayed in this order
 */
const menuActionDisplayOrder = [
  MenuAction.StartGame,
  MenuAction.PauseGame,
  MenuAction.ResumeGame,
  MenuAction.QuitGame,
  MenuAction.Next,
  MenuAction.Back,
  MenuAction.No,
  MenuAction.Yes,
];

export class MenuScreen extends React.Component<
  MenuScreenProbs,
  MenuScreenState
> {
  render() {
    let scrollContent = <>Please follow the instructions on the game screen</>;
    if (this.props.enabledMenuActions.size > 0) {
      const menuActions = Array.from(this.props.enabledMenuActions);
      // Sort the available menu actions to display them in the intended order
      menuActions.sort((left, right) => {
        const leftIdx = menuActionDisplayOrder.indexOf(left);
        const rightIdx = menuActionDisplayOrder.indexOf(right);

        return leftIdx - rightIdx;
      });

      const buttonList = Array.from(menuActions).map(action => {
        const click = (_: any) => this.props.triggerMenuAction(action);

        return (
          <>
            <button
              style={{
                marginTop: '10px',
                marginBottom: '10px',
              }}
              className='pixelbutton'
              onClick={click}
            >
              {menuActionStrings.get(action)}
            </button>
            <br />
          </>
        );
      });

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
