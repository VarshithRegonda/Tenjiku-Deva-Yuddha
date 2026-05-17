import React, { useEffect, useState, useCallback } from 'react';
import { StyleSheet, Text, View, ScrollView, TouchableOpacity, Image, Platform, useWindowDimensions } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import Reanimated, { useSharedValue, useAnimatedStyle, withRepeat, withTiming, interpolate, Easing } from 'react-native-reanimated';
import { FontAwesome5, MaterialCommunityIcons, Ionicons, Entypo } from '@expo/vector-icons';
import * as Haptics from 'expo-haptics';

import { BUILD_PROFILES, resolvePlatformTarget } from '@/src/esports/config';
import { createOrGetProfile, linkPlatform, grantSeasonXp, type CrossProgressionProfile } from '@/src/esports/account';

import { C, F, S, web } from './components/theme';
import { injectGlobalCSS, triggerBattleVFX } from './components/vfx';

interface LogEntry { id: number; text: string; type: 'system' | 'combat' | 'build' | 'network'; }
interface Building { id: string; name: string; cost: number; type: 'farm' | 'gurukul' | 'temple' | 'barracks'; count: number; }
interface GridCell { id: number; buildingId: string | null; direction: string; }
interface PendingSyncAction { id: number; type: 'battle' | 'build'; payload: Record<string, unknown>; createdAt: number; }

interface GameState {
  PlayerName: string; KingdomName: string; KingdomTier: 'Village' | 'Town' | 'City' | 'Kingdom';
  GlobalRank: string; WinRate: number; Population: number; Suvarna: number; Anna: number;
  Shakti: number; Sainya: number; DietPref: 'Veg' | 'Non-Veg'; SessionStartTime: number;
  DharmaStreak: number; MentalFocus: number; Buildings: Building[]; CityGrid: GridCell[];
  BattleLog: LogEntry[]; IsMultiplayer: boolean; ConnectedPlayers: number; UnlockedVedas: string[];
}

const TRANSLATIONS = {
  EN: { territory: "TERRITORY", sabha: "SABHA (DARBAR)", war: "WAR COMMAND", health: "DHARMA HEALTH", pro: "PRO LEAGUE", vitality: "VITALITY INDEX", credits: "GOLD", actives: "ACTIVES (SAI)", rank: "RANKING", guide: "ANCIENT GUIDE", battle: "INITIATE DHARMA YUDDHA", train: "TRAIN SAINYA", wellness: "DHARMA & MANAS WELLNESS", lore: "LORE CODEX" },
  HI: { territory: "क्षेत्र", sabha: "सभा दरबार", war: "युद्ध कमान", health: "धर्म स्वास्थ्य", pro: "प्रो लीग", vitality: "जीवन शक्ति", credits: "स्वर्ण (SUV)", actives: "सैनिक (SAI)", rank: "रैंकिंग", guide: "प्राचीन मार्गदर्शिका", battle: "धर्म युद्ध शुरू करें", train: "सैनिक प्रशिक्षण", wellness: "धर्म और मानस कल्याण", lore: "कथा" },
  SAN: { territory: "क्षेत्रम्", sabha: "सभा", war: "युद्धम्", health: "धर्म स्वास्थ्य", pro: "वरिष्ठ सभा", vitality: "प्राण शक्ति", credits: "सुवर्ण", actives: "सैनिक", rank: "क्रम", guide: "प्राचीन ज्ञान", battle: "धर्म युद्धम् आरम्भ", train: "सैनिक शिक्षणम्", wellness: "मनः स्वास्थ्यम्", lore: "शास्त्र" },
  ES: { territory: "TERRITORIO", sabha: "SABHA (CORTE)", war: "MANDO DE GUERRA", health: "SALUD DHARMA", pro: "LIGA PRO", vitality: "VITALIDAD", credits: "SUV (SUV)", actives: "SAI (SAI)", rank: "RANGO", guide: "GUÍA ANCIANA", battle: "INICIAR DHARMA YUDDHA", train: "ENTRENAR SAINYA", wellness: "BIENESTAR DHARMA", lore: "LORE" }
};

export default function GameDashboard() {
  const [isOnline, setIsOnline] = useState(true);
  const [pendingSyncActions, setPendingSyncActions] = useState<PendingSyncAction[]>([]);
  const [isMounted, setIsMounted] = useState(false);

  const safeStorageSet = (key: string, value: string) => { if (Platform.OS === 'web' && typeof window !== 'undefined' && window.localStorage) window.localStorage.setItem(key, value); };
  const safeStorageGet = (key: string) => { if (Platform.OS === 'web' && typeof window !== 'undefined' && window.localStorage) return window.localStorage.getItem(key); return null; };

  const [gameState, setGameState] = useState<GameState>({
    PlayerName: "ARJUNA_PRO", KingdomName: "ARYAVARTA_PRIME", KingdomTier: 'Village',
    GlobalRank: "IMMORTAL I", WinRate: 98, Population: 150, Suvarna: 5000, Anna: 2000,
    Shakti: 100, Sainya: 20, DietPref: 'Veg', SessionStartTime: Date.now(),
    DharmaStreak: 3, MentalFocus: 100,
    Buildings: [
      { id: '1', name: 'Krishi Farm', cost: 100, type: 'farm', count: 0 },
      { id: '2', name: 'Gurukul', cost: 250, type: 'gurukul', count: 0 },
      { id: '3', name: 'Mandira', cost: 500, type: 'temple', count: 0 },
      { id: '4', name: 'Sainya Barracks', cost: 300, type: 'barracks', count: 0 },
    ],
    CityGrid: [
      { id: 0, buildingId: null, direction: 'NW (Vayuvya)' }, { id: 1, buildingId: null, direction: 'N (Uttara)' }, { id: 2, buildingId: null, direction: 'NE (Ishanya)' },
      { id: 3, buildingId: null, direction: 'W (Pashchima)' }, { id: 4, buildingId: null, direction: 'Center (Brahma)' }, { id: 5, buildingId: null, direction: 'E (Purva)' },
      { id: 6, buildingId: null, direction: 'SW (Nairutya)' }, { id: 7, buildingId: null, direction: 'S (Dakshina)' }, { id: 8, buildingId: null, direction: 'SE (Agneya)' },
    ],
    BattleLog: [{ id: Date.now(), text: "E-SPORTS MODE INITIALIZED: Welcome Commander.", type: "system" as const }],
    IsMultiplayer: false, ConnectedPlayers: 1, UnlockedVedas: [],
  });

  const [language] = useState<'EN' | 'HI' | 'SAN' | 'ES'>('EN');
  const t = (key: keyof typeof TRANSLATIONS['EN']) => TRANSLATIONS[language][key] || key;

  const [activeTab, setActiveTab] = useState<'VastuBuilder' | 'Multiplayer' | 'DharmaYuddha' | 'VedicHealth' | 'Sabha' | 'LoreCodex'>('VastuBuilder');
  const [selectedCell, setSelectedCell] = useState<number | null>(null);
  const [isCinemaMode, setIsCinemaMode] = useState(false);
  const [battleResult, setBattleResult] = useState<{ result: 'win' | 'loss', msg: string } | null>(null);

  const glowValue = useSharedValue(0.4);
  const rotation = useSharedValue(0);
  const chakraBreathe = useSharedValue(1);

  const { width } = useWindowDimensions();
  const isDesktop = width > 1024;
  const isTablet = width > 768 && width <= 1024;
  const activePlatform = resolvePlatformTarget();
  const deviceTier = isDesktop ? 'high' : isTablet ? 'mid' : 'low';
  const activeBuildProfile = BUILD_PROFILES[activePlatform];
  const activePerformanceProfile = activeBuildProfile.performance[deviceTier];
  const bestEffortMode = !isOnline || activePerformanceProfile.targetFps <= 60;
  const [crossProfile, setCrossProfile] = useState<CrossProgressionProfile | null>(null);

  useEffect(() => {
    setIsMounted(true);
    const cleanupCSS = injectGlobalCSS();
    const saved = safeStorageGet('EVOLUTION_SAVE');
    if (saved) setGameState(prev => ({ ...prev, ...JSON.parse(saved) }));
    return cleanupCSS;
  }, []);

  useEffect(() => { if (isMounted) safeStorageSet('EVOLUTION_SAVE', JSON.stringify(gameState)); }, [gameState, isMounted]);

  useEffect(() => {
    glowValue.value = withRepeat(withTiming(1, { duration: 2500 }), -1, true);
    rotation.value = withRepeat(withTiming(360, { duration: 20000 }), -1, false);
    chakraBreathe.value = withRepeat(withTiming(1.08, { duration: 4000, easing: Easing.inOut(Easing.sin) }), -1, true);
  }, []);

  useEffect(() => {
    if (Platform.OS !== 'web' || typeof window === 'undefined') return;
    const updateNetworkStatus = () => setIsOnline(window.navigator.onLine);
    updateNetworkStatus();
    window.addEventListener('online', updateNetworkStatus);
    window.addEventListener('offline', updateNetworkStatus);
    const handleKeyPress = (e: any) => {
      if (e.key.toLowerCase() === 'b') setActiveTab('VastuBuilder');
      if (e.key.toLowerCase() === 'w') setActiveTab('DharmaYuddha');
      if (e.key.toLowerCase() === 'p') setActiveTab('Multiplayer');
      if (e.key.toLowerCase() === 'l') setActiveTab('LoreCodex');
      if (e.key.toLowerCase() === 'm') setIsCinemaMode(prev => !prev);
    };
    window.addEventListener('keydown', handleKeyPress);
    return () => {
      window.removeEventListener('online', updateNetworkStatus);
      window.removeEventListener('offline', updateNetworkStatus);
      window.removeEventListener('keydown', handleKeyPress);
    };
  }, []);

  useEffect(() => {
    const profile = createOrGetProfile('local-player-1', gameState.PlayerName);
    const linked = linkPlatform(profile.playerId, activePlatform) || profile;
    setCrossProfile({ ...linked });
  }, [activePlatform, gameState.PlayerName]);

  useEffect(() => {
    const interval = setInterval(() => {
      setGameState(prev => {
        let vastuShakti = 0, vastuAnna = 0;
        prev.CityGrid.forEach(cell => {
          if (!cell.buildingId) return;
          const b = prev.Buildings.find(build => build.id === cell.buildingId);
          if (b?.type === 'temple' && cell.direction.includes('NE')) vastuShakti += 2;
          if (b?.type === 'farm' && cell.direction.includes('NW')) vastuAnna += 10;
        });
        const farmYield = prev.Buildings.find(b => b.type === 'farm')?.count || 0;
        const popGrowth = Math.floor(farmYield * 1.5) + 1;
        let newTier = prev.KingdomTier;
        if (prev.Population > 1000) newTier = 'Kingdom';
        else if (prev.Population > 500) newTier = 'City';
        else if (prev.Population > 250) newTier = 'Town';
        return {
          ...prev,
          Anna: prev.Anna + (farmYield * 5) + vastuAnna + 2,
          Suvarna: Math.floor(prev.Suvarna + (prev.Population * 0.15)),
          Population: prev.Population + popGrowth,
          Shakti: prev.Shakti + vastuShakti,
          MentalFocus: Math.min(100, prev.MentalFocus + 1),
          KingdomTier: newTier,
          WinRate: Math.max(90, Math.min(99, prev.WinRate + (Math.random() > 0.5 ? 0.1 : -0.1)))
        };
      });
    }, 3000);
    return () => clearInterval(interval);
  }, []);

  const triggerTapFeedback = useCallback(() => {
    if (Platform.OS !== 'web') Haptics.selectionAsync().catch(() => undefined);
  }, []);

  const queueForSync = useCallback((type: PendingSyncAction['type'], payload: Record<string, unknown>) => {
    const action: PendingSyncAction = { id: Date.now() + Math.floor(Math.random() * 1000), type, payload, createdAt: Date.now() };
    setPendingSyncActions(prev => [action, ...prev].slice(0, 50));
  }, []);

  const startBattle = () => {
    triggerTapFeedback();
    if (!isOnline) queueForSync('battle', { timestamp: Date.now(), sainya: gameState.Sainya, shakti: gameState.Shakti });
    const enemyPower = Math.floor(Math.random() * 50) + 10;
    const playerPower = (gameState.Sainya * 1.5) + (gameState.Shakti * 2);

    if (playerPower >= enemyPower) {
      const loot = 200 + Math.floor(Math.random() * 300);
      triggerBattleVFX('win');
      setGameState(prev => ({
        ...prev, Suvarna: prev.Suvarna + loot,
        BattleLog: [{ id: Date.now(), text: `⚡ VICTORY! Defeated Invaders. Looted ${loot} Suvarna.`, type: 'combat' as const }, ...prev.BattleLog].slice(0, 5)
      }));
      setBattleResult({ result: 'win', msg: `🏆 Victory! Your Dharma and Sainya prevailed!` });
      if (crossProfile) {
        const updated = grantSeasonXp(crossProfile.playerId, 150);
        if (updated) setCrossProfile({ ...updated });
      }
    } else {
      triggerBattleVFX('loss');
      setGameState(prev => ({
        ...prev, Sainya: Math.max(0, prev.Sainya - 5),
        BattleLog: [{ id: Date.now(), text: "💀 DEFEAT! Your Sainya retreated.", type: 'combat' as const }, ...prev.BattleLog].slice(0, 5)
      }));
      setBattleResult({ result: 'loss', msg: `⚔️ Defeat. Train more Sainya and Shakti.` });
      if (crossProfile) {
        const updated = grantSeasonXp(crossProfile.playerId, 40);
        if (updated) setCrossProfile({ ...updated });
      }
    }
    setTimeout(() => setBattleResult(null), 3000);
  };

  const constructBuilding = (buildingId: string) => {
    triggerTapFeedback();
    if (selectedCell === null) return;
    if (!isOnline) queueForSync('build', { buildingId, cellId: selectedCell });

    setGameState(prev => {
      const building = prev.Buildings.find(b => b.id === buildingId);
      if (!building || prev.Suvarna < building.cost) {
        return { ...prev, BattleLog: [{ id: Date.now(), text: "Insufficient Suvarna!", type: "build" as const }, ...prev.BattleLog].slice(0, 5) };
      }
      const newGrid = [...prev.CityGrid];
      const cell = newGrid[selectedCell];
      let bonusText = "";
      if (building.type === 'temple' && cell.direction.includes('NE')) bonusText = " (Vastu Bonus: +Shakti)";
      if (building.type === 'farm' && cell.direction.includes('NW')) bonusText = " (Vastu Bonus: +Anna)";
      if (building.type === 'barracks' && cell.direction.includes('SW')) bonusText = " (Vastu Bonus: +Defense)";
      newGrid[selectedCell] = { ...cell, buildingId };

      return {
        ...prev, Suvarna: prev.Suvarna - building.cost,
        Buildings: prev.Buildings.map(b => b.id === buildingId ? { ...b, count: b.count + 1, cost: Math.floor(b.cost * 1.3) } : b),
        CityGrid: newGrid,
        BattleLog: [{ id: Date.now(), text: `Constructed ${building.name} in ${cell.direction}${bonusText}.`, type: "build" as const }, ...prev.BattleLog].slice(0, 5)
      };
    });
    setSelectedCell(null);
  };

  const animatedGlow = useAnimatedStyle(() => ({ opacity: glowValue.value, transform: [{ scale: interpolate(glowValue.value, [0.4, 1], [1, 1.03]) }] }));
  const animatedOrb = useAnimatedStyle(() => ({ transform: [{ rotate: `${rotation.value}deg` }, { scale: chakraBreathe.value }] }));
  const animatedChakraGlow = useAnimatedStyle(() => ({ transform: [{ scale: chakraBreathe.value }], opacity: interpolate(chakraBreathe.value, [1, 1.08], [0.5, 1]) }));

  if (!isMounted) return null;

  return (
    <SafeAreaView style={styles.container} edges={['right', 'left', 'bottom']}>
      <View style={styles.tournamentTicker}>
        <Text style={styles.tickerText} {...web({ className: 'tdv-gold-text tdv-font-heading' })}>
          🏆 LIVE: ARYAVARTA PRIME OPEN ⚡ {activeBuildProfile.tickRateHz}Hz SERVERS ⚡ LEAD: KRISHNA_77 ॐ धर्म युद्ध ⚡ SEASON 1
        </Text>
      </View>
      <View style={[styles.connectionBanner, isOnline ? styles.connectionBannerOnline : styles.connectionBannerOffline]}>
        <Text style={styles.connectionText}>
          {isOnline ? 'ONLINE STABLE' : 'OFFLINE MODE ACTIVE'} • Pending Sync: {pendingSyncActions.length} • {bestEffortMode ? 'BEST-EFFORT PERFORMANCE' : 'FULL VISUAL MODE'}
        </Text>
      </View>

      <View style={isDesktop ? styles.desktopLayout : styles.mobileLayout}>
        <View style={isDesktop ? styles.sidebar : styles.mobileNav}>
          {!isDesktop && (
            <View style={styles.mobileBrand}>
              <Text style={styles.brandTitle} {...web({ className: 'tdv-font-heading' })}>TENJIKU</Text>
            </View>
          )}

          <View style={isDesktop ? styles.navSection : styles.mobileNavInner}>
            {[
              { id: 'VastuBuilder', icon: 'grid', label: t('territory'), color: C.cyan },
              { id: 'Sabha', icon: 'account-group', label: t('sabha'), color: C.gold },
              { id: 'DharmaYuddha', icon: 'sword-cross', label: t('war'), color: C.red },
              { id: 'VedicHealth', icon: 'heartbeat', label: t('health'), color: C.green },
              { id: 'Multiplayer', icon: 'trophy-variant', label: t('pro'), color: C.gold },
              { id: 'LoreCodex', icon: 'book-open-page-variant', label: t('lore'), color: C.cyan }
            ].map(item => (
              <TouchableOpacity key={item.id} onPress={() => setActiveTab(item.id as any)} style={[styles.navItem, activeTab === item.id && styles.navItemActive]} {...web({ className: `tdv-nav-btn ${activeTab === item.id ? 'active' : ''}` })}>
                {item.icon === 'grid' ? <Entypo name="grid" size={20} color={activeTab === item.id ? item.color : C.textMuted} /> :
                 item.icon === 'heartbeat' ? <FontAwesome5 name="heartbeat" size={18} color={activeTab === item.id ? item.color : C.textMuted} /> :
                 <MaterialCommunityIcons name={item.icon as any} size={20} color={activeTab === item.id ? item.color : C.textMuted} />}
                {isDesktop && <Text style={[styles.navText, activeTab === item.id && { color: C.textPrimary }]} {...web({ className: 'tdv-font-ui' })}>{item.label}</Text>}
              </TouchableOpacity>
            ))}
          </View>

          {isDesktop && (
            <View style={styles.sidebarFooter}>
              <View style={styles.proProfile}>
                <View style={styles.heroWrapper}>
                  <Image source={{ uri: 'https://images.unsplash.com/photo-1578632292335-df3abbb0d586?q=80&w=200' }} style={styles.heroImage} />
                  <Reanimated.View style={[styles.heroGlow, animatedChakraGlow]} />
                  <View style={styles.heroOverlay} />
                </View>
                <View style={styles.heroInfo}>
                  <Text style={styles.proRankLabel} {...web({ className: 'tdv-font-ui' })}>COMMANDER</Text>
                  <Text style={styles.heroName} {...web({ className: 'tdv-font-heading tdv-cyan-text' })}>{gameState.PlayerName}</Text>
                  <Text style={styles.proRankVal} {...web({ className: 'tdv-font-ui tdv-gold-text' })}>{gameState.GlobalRank}</Text>
                  <Text style={styles.heroKingdom} {...web({ className: 'tdv-font-body' })}>ॐ {gameState.KingdomName} ॐ</Text>
                </View>
              </View>
            </View>
          )}
        </View>

        <View style={styles.content}>
          <ScrollView contentContainerStyle={styles.mainContainer}>
            <View style={styles.topResourceRow}>
              <View style={[styles.resCard, { borderLeftColor: C.cyan }]} {...web({ className: 'tdv-glass tdv-card-hover' })}>
                <Text style={styles.resLabel} {...web({ className: 'tdv-font-ui' })}>{t('vitality')}</Text>
                <Text style={styles.resVal} {...web({ className: 'tdv-font-heading' })}>{Math.floor(gameState.Anna)}</Text>
              </View>
              <View style={[styles.resCard, { borderLeftColor: C.gold }]} {...web({ className: 'tdv-glass-gold tdv-card-hover' })}>
                <Text style={styles.resLabel} {...web({ className: 'tdv-font-ui' })}>{t('credits')}</Text>
                <Text style={[styles.resVal, { color: C.gold }]} {...web({ className: 'tdv-font-heading' })}>{Math.floor(gameState.Suvarna)}</Text>
              </View>
              <View style={[styles.resCard, { borderLeftColor: C.red }]} {...web({ className: 'tdv-glass-red tdv-card-hover' })}>
                <Text style={styles.resLabel} {...web({ className: 'tdv-font-ui' })}>{t('actives')}</Text>
                <Text style={[styles.resVal, { color: C.red }]} {...web({ className: 'tdv-font-heading' })}>{gameState.Sainya}</Text>
              </View>
            </View>

            {activeTab === 'VastuBuilder' && (
              <View style={styles.tabContent}>
                <View style={styles.headerRow}>
                  <View>
                    <Text style={styles.sectionTitle} {...web({ className: 'tdv-font-heading' })}>VASTU PURUSHA MANDALA</Text>
                    <Text style={styles.sectionSubtitle} {...web({ className: 'tdv-font-ui' })}>Strategic City Matrix • Tier {gameState.KingdomTier}</Text>
                  </View>
                  <TouchableOpacity style={styles.cinemaBtn} onPress={() => setIsCinemaMode(!isCinemaMode)} {...web({ className: 'tdv-btn-primary' })}>
                    <Ionicons name={isCinemaMode ? "eye-off" : "aperture"} size={16} color={C.cyan} />
                    <Text style={styles.cinemaText} {...web({ className: 'tdv-font-ui' })}>{isCinemaMode ? "REALITY MODE" : "COMMAND VIEW"}</Text>
                  </TouchableOpacity>
                </View>

                <View style={[styles.mandalaContainer, isCinemaMode && { opacity: 0.2 }]} {...web({ className: 'tdv-glass' })}>
                  <Reanimated.View style={[styles.atmaOrb, animatedOrb]} />
                  <View style={styles.mandalaGrid}>
                    {gameState.CityGrid.map((cell, idx) => {
                      const building = gameState.Buildings.find(b => b.id === cell.buildingId);
                      const isSelected = selectedCell === idx;
                      return (
                        <TouchableOpacity key={cell.id} style={[styles.gridCell, isSelected && styles.gridCellSelected]} onPress={() => setSelectedCell(isSelected ? null : idx)} {...web({ className: 'tdv-card-hover' })}>
                          {building ? (
                            <View style={styles.cellContent}>
                              {building.type === 'farm' && <MaterialCommunityIcons name="corn" size={32} color={C.gold} />}
                              {building.type === 'gurukul' && <FontAwesome5 name="book-reader" size={24} color={C.gold} />}
                              {building.type === 'temple' && <MaterialCommunityIcons name="temple-hindu" size={32} color={C.gold} />}
                              {building.type === 'barracks' && <MaterialCommunityIcons name="shield-sword" size={32} color={C.gold} />}
                              <Text style={styles.cellName} {...web({ className: 'tdv-font-ui' })}>{building.name}</Text>
                            </View>
                          ) : (
                            <View style={styles.emptyCell}>
                              <Text style={styles.cellDir} {...web({ className: 'tdv-font-ui' })}>{cell.direction.split(' ')[0]}</Text>
                              <Ionicons name="add" size={20} color="rgba(255,215,0,0.3)" />
                            </View>
                          )}
                        </TouchableOpacity>
                      );
                    })}
                  </View>
                </View>

                {selectedCell !== null && (
                  <View style={styles.buildMenu} {...web({ className: 'tdv-glass tdv-slide-in' })}>
                    <Text style={styles.buildTitle} {...web({ className: 'tdv-font-ui' })}>Build in {gameState.CityGrid[selectedCell].direction}</Text>
                    <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={{ gap: S.s3 }}>
                      {gameState.Buildings.map(b => (
                        <TouchableOpacity key={b.id} style={styles.buildCard} onPress={() => constructBuilding(b.id)} {...web({ className: 'tdv-card-hover' })}>
                          <Text style={styles.buildCardName} {...web({ className: 'tdv-font-ui' })}>{b.name}</Text>
                          <Text style={styles.buildCardCost} {...web({ className: 'tdv-font-ui tdv-gold-text' })}>💰 {b.cost}</Text>
                        </TouchableOpacity>
                      ))}
                    </ScrollView>
                  </View>
                )}

                {!isCinemaMode && (
                  <View style={styles.logContainer} {...web({ className: 'tdv-glass' })}>
                    <Text style={styles.logTitle} {...web({ className: 'tdv-font-ui' })}>Dharma Log</Text>
                    {gameState.BattleLog.map(log => (
                      <Text key={log.id} style={[styles.logText, { color: log.type === 'build' ? C.gold : C.textSecondary }]} {...web({ className: 'tdv-font-body' })}>{log.text}</Text>
                    ))}
                  </View>
                )}
              </View>
            )}

            {activeTab === 'DharmaYuddha' && (
              <View style={styles.tabContent} {...web({ className: 'tdv-fade-in' })}>
                <View style={styles.headerRow}>
                  <View>
                    <Text style={styles.sectionTitle} {...web({ className: 'tdv-font-heading' })}>BATTLE SECTOR</Text>
                    <Text style={styles.sectionSubtitle} {...web({ className: 'tdv-font-ui' })}>Kuru-Kshetra Alpha</Text>
                  </View>
                  <View style={styles.winRateBadge} {...web({ className: 'tdv-glass-cyan' })}>
                    <Text style={styles.winRateLabel} {...web({ className: 'tdv-font-ui' })}>WIN PROBABILITY</Text>
                    <Text style={styles.winRateVal} {...web({ className: 'tdv-font-heading' })}>{gameState.WinRate}%</Text>
                  </View>
                </View>

                <View style={styles.battleCard} {...web({ className: 'tdv-glass' })}>
                  <View style={styles.battleStatsRow}>
                    <View>
                      <Text style={styles.battleStatsLabel} {...web({ className: 'tdv-font-ui' })}>AKSHAUHINI POWER</Text>
                      <Text style={styles.battleStatsVal} {...web({ className: 'tdv-font-heading' })}>{gameState.Sainya}K</Text>
                    </View>
                    <MaterialCommunityIcons name="sword-cross" size={48} color={C.red} style={{ opacity: 0.8 }} />
                  </View>
                  <View style={styles.gaugeContainer}>
                    <Text style={styles.gaugeLabel} {...web({ className: 'tdv-font-ui' })}>DHARMA STRENGTH</Text>
                    <View style={styles.gaugeTrack}>
                      <View style={[styles.gaugeFill, { width: `${gameState.MentalFocus}%`, backgroundColor: C.red }]} {...web({ className: 'tdv-pulse-glow' })} />
                    </View>
                  </View>
                </View>

                <TouchableOpacity style={styles.battleBtn} onPress={startBattle} {...web({ className: 'tdv-btn-primary tdv-battle-throb' })}>
                  <Text style={styles.battleBtnText} {...web({ className: 'tdv-font-heading' })}>⚔️ {t('battle')}</Text>
                </TouchableOpacity>

                {battleResult && (
                  <View style={[styles.battleAlert, { backgroundColor: battleResult.result === 'win' ? 'rgba(0, 255, 136, 0.2)' : 'rgba(255, 68, 68, 0.2)', borderColor: battleResult.result === 'win' ? C.green : C.red }]} {...web({ className: 'tdv-glass tdv-slide-in' })}>
                    <Text style={[styles.alertText, { color: battleResult.result === 'win' ? C.green : C.red }]} {...web({ className: 'tdv-font-ui' })}>{battleResult.msg}</Text>
                  </View>
                )}
              </View>
            )}

            {/* Other tabs can be similarly implemented... For brevity, showing placeholders that map to Lore/Health */}
            {(activeTab === 'LoreCodex' || activeTab === 'Sabha' || activeTab === 'VedicHealth' || activeTab === 'Multiplayer') && (
              <View style={styles.tabContent} {...web({ className: 'tdv-fade-in tdv-glass' })}>
                 <Text style={styles.sectionTitle} {...web({ className: 'tdv-font-heading' })}>{t(activeTab === 'LoreCodex' ? 'lore' : activeTab === 'Sabha' ? 'sabha' : activeTab === 'VedicHealth' ? 'wellness' : 'pro')}</Text>
                 <Text style={[styles.sectionSubtitle, {marginTop: S.s2}]} {...web({ className: 'tdv-font-body' })}>System operational. Accessing divine records...</Text>
              </View>
            )}

          </ScrollView>
        </View>
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: C.bg0 },
  desktopLayout: { flex: 1, flexDirection: 'row' },
  mobileLayout: { flex: 1, flexDirection: 'column-reverse' },
  
  tournamentTicker: { backgroundColor: C.bg1, paddingVertical: S.s2, borderBottomWidth: 1, borderBottomColor: C.borderGold, alignItems: 'center' },
  tickerText: { color: C.gold, fontSize: F.size[1], fontWeight: '700', letterSpacing: 2 },
  
  connectionBanner: { paddingVertical: S.s1, paddingHorizontal: S.s3, alignItems: 'center' },
  connectionBannerOnline: { backgroundColor: C.greenDim },
  connectionBannerOffline: { backgroundColor: C.redDim },
  connectionText: { color: C.textPrimary, fontSize: F.size[1], fontWeight: '700', letterSpacing: 1 },

  sidebar: { width: 280, backgroundColor: C.bg2, borderRightWidth: 1, borderRightColor: C.borderCyan, padding: S.s5, justifyContent: 'space-between' },
  mobileNav: { backgroundColor: C.bg2, borderTopWidth: 1, borderTopColor: C.borderCyan, padding: S.s4, paddingBottom: Platform.OS === 'ios' ? S.s8 : S.s4 },
  mobileNavInner: { flexDirection: 'row', justifyContent: 'space-around' },
  mobileBrand: { position: 'absolute', top: -50, left: S.s4 },
  brandTitle: { color: C.textPrimary, fontSize: F.size[6], fontWeight: '900', letterSpacing: 2 },

  navSection: { gap: S.s2 },
  navItem: { flexDirection: 'row', alignItems: 'center', gap: S.s3, padding: S.s4, borderRadius: 12 },
  navItemActive: { backgroundColor: C.cyanDim, borderLeftWidth: 3, borderLeftColor: C.cyan, borderRadius: 8 },
  navText: { color: C.textSecondary, fontSize: F.size[3], fontWeight: '600', letterSpacing: 1 },

  sidebarFooter: { marginTop: 'auto' },
  proProfile: { backgroundColor: C.bg3, borderRadius: 16, overflow: 'hidden', borderWidth: 1, borderColor: C.border1 },
  heroWrapper: { height: 100, position: 'relative' },
  heroImage: { width: '100%', height: '100%', opacity: 0.6 },
  heroOverlay: { ...StyleSheet.absoluteFillObject, backgroundColor: 'rgba(0,0,0,0.4)' },
  heroGlow: { position: 'absolute', bottom: -20, right: -20, width: 60, height: 60, borderRadius: 30, backgroundColor: C.cyan, opacity: 0.2 },
  heroInfo: { padding: S.s4, alignItems: 'center' },
  proRankLabel: { color: C.textSecondary, fontSize: F.size[1], fontWeight: '700', letterSpacing: 1 },
  heroName: { color: C.cyan, fontSize: F.size[6], fontWeight: '900', letterSpacing: 1, marginVertical: S.s1 },
  proRankVal: { color: C.gold, fontSize: F.size[4], fontWeight: '700' },
  heroKingdom: { color: C.textMuted, fontSize: F.size[1], marginTop: S.s1 },

  content: { flex: 1, backgroundColor: C.bg0 },
  mainContainer: { padding: S.s6, maxWidth: 1200, alignSelf: 'center', width: '100%', gap: S.s6 },

  topResourceRow: { flexDirection: 'row', gap: S.s4, flexWrap: 'wrap' },
  resCard: { flex: 1, minWidth: 120, backgroundColor: C.bg3, padding: S.s5, borderRadius: 16, borderWidth: 1, borderColor: C.border1, borderLeftWidth: 4 },
  resLabel: { color: C.textSecondary, fontSize: F.size[1], fontWeight: '700', letterSpacing: 1, textTransform: 'uppercase', marginBottom: S.s1 },
  resVal: { color: C.textPrimary, fontSize: F.size[8], fontWeight: '900' },

  tabContent: { flex: 1, gap: S.s6, padding: S.s4, borderRadius: 24, backgroundColor: C.bg2, borderWidth: 1, borderColor: C.border1 },
  headerRow: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: S.s4 },
  sectionTitle: { color: C.textPrimary, fontSize: F.size[7], fontWeight: '900', letterSpacing: 2 },
  sectionSubtitle: { color: C.textSecondary, fontSize: F.size[3], fontWeight: '500' },
  cinemaBtn: { flexDirection: 'row', alignItems: 'center', gap: S.s2, backgroundColor: C.cyanDim, paddingHorizontal: S.s4, paddingVertical: S.s2, borderRadius: 20, borderWidth: 1, borderColor: C.borderCyan },
  cinemaText: { color: C.cyan, fontSize: F.size[2], fontWeight: '700' },

  mandalaContainer: { padding: S.s6, backgroundColor: C.bg1, borderRadius: 24, borderWidth: 1, borderColor: C.border1, alignItems: 'center', minHeight: 400, justifyContent: 'center' },
  atmaOrb: { position: 'absolute', width: 280, height: 280, borderRadius: 140, backgroundColor: C.cyanGlow, borderWidth: 1, borderColor: C.cyanDim },
  mandalaGrid: { flexDirection: 'row', flexWrap: 'wrap', width: 340, height: 340, gap: 10, transform: [{ rotateX: '20deg' }, { rotateZ: '-5deg' }] },
  gridCell: { width: 106, height: 106, backgroundColor: 'rgba(18,22,29,0.8)', borderRadius: 16, borderWidth: 1, borderColor: C.border1, justifyContent: 'center', alignItems: 'center' },
  gridCellSelected: { borderColor: C.gold, backgroundColor: C.goldDim, borderWidth: 2 },
  cellContent: { alignItems: 'center', gap: S.s1 },
  cellName: { color: C.textSecondary, fontSize: F.size[1], fontWeight: '700' },
  emptyCell: { alignItems: 'center', opacity: 0.5 },
  cellDir: { color: C.textMuted, fontSize: F.size[1], fontWeight: '700', marginBottom: S.s1 },

  buildMenu: { backgroundColor: C.bg3, padding: S.s5, borderRadius: 20, borderWidth: 1, borderColor: C.borderGold },
  buildTitle: { color: C.textPrimary, fontSize: F.size[4], fontWeight: '700', marginBottom: S.s3 },
  buildCard: { backgroundColor: C.bg2, padding: S.s4, borderRadius: 12, borderWidth: 1, borderColor: C.border1, minWidth: 140, alignItems: 'center' },
  buildCardName: { color: C.textPrimary, fontSize: F.size[2], fontWeight: '700', marginBottom: S.s1 },
  buildCardCost: { color: C.gold, fontSize: F.size[1], fontWeight: '700' },

  logContainer: { backgroundColor: C.bg3, padding: S.s5, borderRadius: 16, borderWidth: 1, borderColor: C.border1 },
  logTitle: { color: C.textSecondary, fontSize: F.size[3], fontWeight: '700', marginBottom: S.s3 },
  logText: { fontSize: F.size[2], marginBottom: S.s1, lineHeight: 20 },

  winRateBadge: { backgroundColor: C.cyanDim, padding: S.s3, borderRadius: 12, borderWidth: 1, borderColor: C.borderCyan, alignItems: 'flex-end' },
  winRateLabel: { color: C.textSecondary, fontSize: F.size[1], fontWeight: '700', letterSpacing: 1 },
  winRateVal: { color: C.cyan, fontSize: F.size[7], fontWeight: '900' },

  battleCard: { backgroundColor: C.bg3, padding: S.s6, borderRadius: 24, borderWidth: 1, borderColor: C.border1 },
  battleStatsRow: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: S.s5 },
  battleStatsLabel: { color: C.textSecondary, fontSize: F.size[2], fontWeight: '700', letterSpacing: 1 },
  battleStatsVal: { color: C.textPrimary, fontSize: F.size[9], fontWeight: '900' },
  gaugeContainer: { borderTopWidth: 1, borderTopColor: C.border1, paddingTop: S.s4 },
  gaugeLabel: { color: C.red, fontSize: F.size[1], fontWeight: '700', letterSpacing: 1, marginBottom: S.s2 },
  gaugeTrack: { height: 8, backgroundColor: C.bg1, borderRadius: 4, overflow: 'hidden' },
  gaugeFill: { height: '100%', borderRadius: 4 },

  battleBtn: { backgroundColor: C.red, padding: S.s5, borderRadius: 12, alignItems: 'center', marginTop: S.s4 },
  battleBtnText: { color: C.textInverse, fontSize: F.size[6], fontWeight: '900', letterSpacing: 3 },
  
  battleAlert: { padding: S.s4, borderRadius: 12, borderWidth: 1, alignItems: 'center', marginTop: S.s4 },
  alertText: { fontSize: F.size[3], fontWeight: '700', letterSpacing: 1 },
});
