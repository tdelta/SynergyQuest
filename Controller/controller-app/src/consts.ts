import { PlayerColor, Button } from 'controller-client-lib';
import RedAvatar from './gfx/avatars/red.png';
import BlueAvatar from './gfx/avatars/blue.png';
import GreenAvatar from './gfx/avatars/green.png';
import YellowAvatar from './gfx/avatars/yellow.png';
import AnyAvatar from './gfx/avatars/any.png';

export interface ColorData {
  name: string;
  light: string;
  dark: string;
}

export const port: number = 4242;

export const colors: { [color in PlayerColor]: ColorData } = {
  0: { name: 'Red', light: '#ff867c', dark: '#e53935' },
  1: { name: 'Blue', light: '#6ab7ff', dark: '#1e88e5' },
  2: { name: 'Green', light: '#76d275', dark: '#43a047' },
  3: { name: 'Yellow', light: '#ffff6b', dark: '#fdd835' },
  4: { name: 'color undefined', light: '#888', dark: '#444' },
};

export const buttonStyles: { [button in Button]: ColorData } = {
  0: { name: 'Attack', light: '#ff6f60', dark: '#E53935' },
  1: { name: 'Pull', light: '#6ab7ff', dark: '#1e88e5' },
  2: { name: 'Carry', light: '#4ebaaa', dark: '#00897B' },
  3: { name: 'Press', light: '#ff844c', dark: '#F4511E' },
  4: { name: 'Trow Bomb', light: '#c158dc', dark: '#8E24AA' },
  5: { name: 'Read', light: '#ffe54c', dark: '#ffb300' },
  6: { name: 'Open', light: '#ec407a', dark: '#c2185b' },
};

export const avatars: { [color in PlayerColor]: string } = {
  0: RedAvatar,
  1: BlueAvatar,
  2: GreenAvatar,
  3: YellowAvatar,
  4: AnyAvatar,
};
