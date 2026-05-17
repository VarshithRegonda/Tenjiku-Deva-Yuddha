import { Platform } from 'react-native';

export const C = {
  // Core backgrounds
  bg0:       '#020408',
  bg1:       '#080C12',
  bg2:       '#0D1117',
  bg3:       '#12161D',
  bg4:       '#1C2128',

  // Borders
  border0:   'rgba(255,255,255,0.04)',
  border1:   'rgba(255,255,255,0.08)',
  borderCyan:'rgba(0,240,255,0.25)',
  borderGold:'rgba(255,215,0,0.25)',
  borderRed: 'rgba(255,68,68,0.3)',

  // Accents
  cyan:      '#00F0FF',
  cyanDim:   'rgba(0,240,255,0.12)',
  cyanGlow:  'rgba(0,240,255,0.06)',
  gold:      '#FFD700',
  goldDim:   'rgba(255,215,0,0.12)',
  goldGlow:  'rgba(255,215,0,0.06)',
  red:       '#FF4444',
  redDim:    'rgba(255,68,68,0.12)',
  green:     '#00FF88',
  greenDim:  'rgba(0,255,136,0.12)',
  purple:    '#9B59FF',

  // Text
  textPrimary:   '#E6EDF3',
  textSecondary: '#8B949E',
  textMuted:     '#484F58',
  textInverse:   '#000000',
} as const;

export const F = {
  size: [8, 10, 11, 12, 13, 14, 16, 18, 20, 24, 28, 32, 40, 48] as const,
  weight: { regular: '400', bold: '700', black: '900' } as const,
  // Web font families injected via CSS
  heading:   Platform.OS === 'web' ? "'Cinzel', 'Georgia', serif" : undefined,
  ui:        Platform.OS === 'web' ? "'Rajdhani', 'System', sans-serif" : undefined,
  body:      Platform.OS === 'web' ? "'Inter', 'System', sans-serif" : undefined,
} as const;

export const R = {
  xs: 6, sm: 8, md: 12, lg: 16, xl: 20, xxl: 24, card: 16, pill: 50,
} as const;

export const S = {
  // Spacing scale (multiples of 4)
  s1: 4, s2: 8, s3: 12, s4: 16, s5: 20, s6: 24, s8: 32, s10: 40,
} as const;

/** Returns a web-only box shadow string, or empty string on native */
export const webShadow = (color: string, spread = 20, size = 0) =>
  Platform.OS === 'web'
    ? `0 0 ${spread}px ${color}${size ? `, 0 ${size}px ${spread * 2}px rgba(0,0,0,0.4)` : ''}`
    : undefined;

/** Web-only style object — returns {} on native */
export const web = (style: Record<string, unknown>): Record<string, unknown> =>
  Platform.OS === 'web' ? style : {};
