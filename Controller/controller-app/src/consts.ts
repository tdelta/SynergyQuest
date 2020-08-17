import { PlayerColor, Button } from 'controller-client-lib';

import RedAvatar from './gfx/avatars/red.png';
import BlueAvatar from './gfx/avatars/blue.png';
import GreenAvatar from './gfx/avatars/green.png';
import YellowAvatar from './gfx/avatars/yellow.png';
import AnyAvatar from './gfx/avatars/any.png';

import RedPlatform from './gfx/playerControlledPlatform/red.png';
import BluePlatform from './gfx/playerControlledPlatform/blue.png';
import GreenPlatform from './gfx/playerControlledPlatform/green.png';
import YellowPlatform from './gfx/playerControlledPlatform/yellow.png';
import AnyPlatform from './gfx/playerControlledPlatform/any.png';

import BombImage from './gfx/bomb.png';

export interface ColorData {
  name: string;
  image: string | undefined;
  light: string;
  dark: string;
}

export const port: number = 8000;
export const diagnosticsPort: number = 8000;

export const itemButtons = new Set<Button>([Button.UseBomb]);
export const primaryButtons = new Set<Button>([Button.Attack]);

export const colors: { [color in PlayerColor]: ColorData } = {
  0: {
    name: 'Red',
    image: undefined,
    light: '#ff867c',
    dark: '#e53935',
  },
  1: {
    name: 'Blue',
    image: undefined,
    light: '#6ab7ff',
    dark: '#1e88e5',
  },
  2: {
    name: 'Green',
    image: undefined,
    light: '#76d275',
    dark: '#43a047',
  },
  3: {
    name: 'Yellow',
    image: undefined,
    light: '#ffff6b',
    dark: '#fdd835',
  },
  4: {
    name: 'color undefined',
    image: undefined,
    light: '#888',
    dark: '#444',
  },
};

export const buttonStyles: { [button in Button]: ColorData } = {
  0: {
    name: 'Attack',
    image: undefined,
    light: '#ff6f60',
    dark: '#E53935',
  },
  1: {
    name: 'Pull',
    image: undefined,
    light: '#6ab7ff',
    dark: '#1e88e5',
  },
  2: {
    name: 'Carry',
    image: undefined,
    light: '#4ebaaa',
    dark: '#00897B',
  },
  3: {
    name: 'Press',
    image: undefined,
    light: '#ff844c',
    dark: '#F4511E',
  },
  4: {
    name: 'Throw',
    image: undefined,
    light: '#4ebaaa',
    dark: '#00897B',
  },
  5: {
    name: 'Read',
    image: undefined,
    light: '#ffe54c',
    dark: '#ffb300',
  },
  6: {
    name: 'Open',
    image: undefined,
    light: '#ec407a',
    dark: '#c2185b',
  },
  7: {
    name: 'Use Bomb',
    image: BombImage,
    light: '#c158dc',
    dark: '#8E24AA',
  },
  8: {
    name: 'Exit',
    image: undefined,
    light: '#c158dc',
    dark: '#8E24AA',
  },
};

export const avatars: { [color in PlayerColor]: string } = {
  0: RedAvatar,
  1: BlueAvatar,
  2: GreenAvatar,
  3: YellowAvatar,
  4: AnyAvatar,
};

export const platforms: { [color in PlayerColor]: string } = {
  0: RedPlatform,
  1: BluePlatform,
  2: GreenPlatform,
  3: YellowPlatform,
  4: AnyPlatform,
};
