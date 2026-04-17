import { DarkTheme, DefaultTheme, ThemeProvider } from '@react-navigation/native';
import { useFonts } from 'expo-font';
import { Slot } from 'expo-router';
import * as SplashScreen from 'expo-splash-screen';
import { useEffect } from 'react';
import { StatusBar } from 'expo-status-bar';
import 'react-native-reanimated';
import { Platform } from 'react-native';

import { useColorScheme } from '@/hooks/use-color-scheme';

SplashScreen.preventAutoHideAsync();

export default function RootLayout() {
  const colorScheme = useColorScheme();
  
  // Only manually load SpaceMono, let @expo/vector-icons handle its own fonts on web
  // This prevents 'fontfaceobserver' timeout errors.
  const [loaded, error] = useFonts({
    ...(Platform.OS !== 'web' ? {
      'MaterialIcons': require('@expo/vector-icons/build/vendor/react-native-vector-icons/Fonts/MaterialIcons.ttf'),
      'FontAwesome5Free-Solid': require('@expo/vector-icons/build/vendor/react-native-vector-icons/Fonts/FontAwesome5_Solid.ttf'),
    } : {})
  });

  useEffect(() => {
    // On web, we hide splash immediately if fonts are taking too long
    if (loaded || error || Platform.OS === 'web') {
      SplashScreen.hideAsync();
    }
  }, [loaded, error]);

  // Non-blocking for Web to prevent timeout errors from stopping the app
  if (!loaded && !error && Platform.OS !== 'web') {
    return null;
  }

  return (
    <ThemeProvider value={colorScheme === 'dark' ? DarkTheme : DefaultTheme}>
      <Slot />
      <StatusBar style="auto" />
    </ThemeProvider>
  );
}
