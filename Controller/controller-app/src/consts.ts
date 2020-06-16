import { PlayerColor } from 'controller-client-lib';

export interface ColorData {
  name: string;
  light: string;
  dark: string;
}

export const port: number = 4242;

export const colors: { [color in PlayerColor]: ColorData; } =
  {
    0: { name: 'Blue', light: '#6ab7ff', dark: '#1e88e5'},
    1: { name: 'Yellow', light: '#ffff6b', dark: '#fdd835'},
    2: { name: 'Red', light: '#ff867c', dark: '#e53935'},
    3: { name: 'Green', light: '#76d275', dark: '#43a047'},
    4: { name: 'color undefined', light: '#888', dark: '#444'}
  };
