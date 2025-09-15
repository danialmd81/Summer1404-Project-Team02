import {definePreset} from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';

export const CustomPreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: '#e6f4ef',
      100: '#cce9df',
      200: '#99d3bf',
      300: '#66bda0',
      400: '#3ecf8e',
      500: '#28a46f',
      600: '#1f8257',
      700: '#16613f',
      800: '#0d3f27',
      900: '#006239',
      950: '#00452a',
    },
    colorScheme: {
      root: {
        surface: {
          0: '#121212',
          50: '#1e1e1e',
          100: '#2a2a2a',
          200: '#333333',
          300: '#3d3d3d',
          400: '#474747',
          500: '#525252',
          600: '#636363',
          700: '#757575',
          800: '#8a8a8a',
          900: '#a1a1a1',
          950: '#cfcfcf',
        },
        text: {
          color: '#ffffff',
          secondary: '#e0e0e0',
          muted: '#b3b3b3',
        },
        primary: {
          color: '{primary.600}',
          inverseColor: '#ffffff',
          hoverColor: '{primary.900}',
          activeColor: '{primary.800}'
        },
        secondary: {
          color: '{primary.100}',
          inverseColor: '#ffffff',
          hoverColor: '{primary.900}',
          activeColor: '{primary.800}'
        },
        highlight: {
          background: 'rgba(255, 204, 128, .16)',
          focusBackground: 'rgba(255, 204, 128, .24)',
          color: 'rgba(255,255,255,.87)',
          focusColor: 'rgba(255,255,255,.87)'
        }
      },
    },
  },
});
