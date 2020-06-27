import { MenuAction } from 'controller-client-lib';

/**
 * Assign a human readable name to every menu action
 */
export const menuActionStrings = new Map<MenuAction, string>();
menuActionStrings.set(MenuAction.StartGame, 'Start Game');
menuActionStrings.set(MenuAction.QuitGame, 'Quit Game');
menuActionStrings.set(MenuAction.PauseGame, 'Pause Game');
menuActionStrings.set(MenuAction.ResumeGame, 'Resume Game');