import { PlayerColor } from 'controller-client-lib';

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
