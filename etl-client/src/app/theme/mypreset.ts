import Aura from '@primeuix/themes/aura';
import {definePreset} from '@primeuix/themes';

export const CustomPreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: '{orange.50}',
      100: '{orange.100}',
      200: '{orange.200}',
      300: '{orange.300}',
      400: '{orange.400}',
      500: '{orange.500}',
      600: '{orange.600}',
      700: '{orange.700}',
      800: '{orange.800}',
      900: '{orange.900}',
      950: '{orange.950}'
    },
    accent: {
      50: '{amber.50}',
      100: '{amber.100}',
      200: '{amber.200}',
      300: '{amber.300}',
      400: '{amber.400}',
      500: '{amber.500}',
      600: '{amber.600}',
      700: '{amber.700}',
      800: '{amber.800}',
      900: '{amber.900}',
      950: '{amber.950}'
    },
    colorScheme: {
      light: {
        primary: {
          color: '{orange.600}',
          inverseColor: '#ffffff',
          hoverColor: '{orange.700}',
          activeColor: '{orange.800}'
        },
        highlight: {
          background: '{amber.200}',
          focusBackground: '{amber.300}',
          color: '{orange.950}',
          focusColor: '{orange.950}'
        }
      },
      dark: {
        primary: {
          color: '{amber.300}',
          inverseColor: '{stone.950}',
          hoverColor: '{amber.400}',
          activeColor: '{amber.500}'
        },
        highlight: {
          background: 'rgba(255, 204, 128, .16)',
          focusBackground: 'rgba(255, 204, 128, .24)',
          color: 'rgba(255,255,255,.87)',
          focusColor: 'rgba(255,255,255,.87)'
        }
      }
    }
  }
});

