import { PlayerColor, Button } from 'controller-client-lib';

export interface ColorData {
  name: string;
  light: string;
  dark: string;
}

export const port: number = 4242;

export const colors: { [color in PlayerColor]: ColorData; } =
  {
    0: { name: 'Red', light: '#ff867c', dark: '#e53935'},
    1: { name: 'Blue', light: '#6ab7ff', dark: '#1e88e5'},
    2: { name: 'Green', light: '#76d275', dark: '#43a047'},
    3: { name: 'Yellow', light: '#ffff6b', dark: '#fdd835'},
    4: { name: 'color undefined', light: '#888', dark: '#444'},
  };

export const buttonStyles: { [button in Button]: ColorData; } =
  {
    0: { name: 'Attack', light: '#e53935', dark: '#ef5350'},
    1: { name: 'Pull', light: '#039Be5', dark: '#29b6f6'},
    2: { name: 'Press', light: '#ffa726', dark: '#fb8c00'},
  };
