import React from 'react';
import { StyleSheet, Text, View, ScrollView, SafeAreaView, TouchableOpacity, Dimensions, Image, Easing, ImageSourcePropType, ImageBackground, Platform, Alert, Animated as BaseAnimated } from 'react-native';
import Reanimated, { useSharedValue, useAnimatedStyle, withRepeat, withTiming, withSequence, withDelay, interpolate, Extrapolate } from 'react-native-reanimated';
import { FontAwesome5, MaterialCommunityIcons, Ionicons, Entypo, Feather } from '@expo/vector-icons';
import { useWindowDimensions } from 'react-native';

const BG_URLS = {
  Hero: 'https://images.unsplash.com/photo-1542751371-adc38448a05e?q=80&w=1200',
  Villain: 'https://images.unsplash.com/photo-1511512578047-dfb367046420?q=80&w=1200',
  Dashboard: 'https://images.unsplash.com/photo-1614850523296-d8c1af93d400?q=80&w=1200'
};

interface LogEntry {
  id: number;
  text: string;
  type: 'system' | 'combat' | 'build' | 'network';
}

interface Building {
  id: string;
  name: string;
  cost: number;
  type: 'farm' | 'gurukul' | 'temple' | 'barracks';
  count: number;
}

interface GridCell {
  id: number;
  buildingId: string | null;
  direction: string;
}


interface GameState {
  PlayerName: string;
  KingdomName: string;
  KingdomTier: 'Village' | 'Town' | 'City' | 'Kingdom';
  GlobalRank: string;
  WinRate: number;
  Population: number;
  Suvarna: number;
  Anna: number;
  Shakti: number;
  Sainya: number;
  DietPref: 'Veg' | 'Non-Veg';
  SessionStartTime: number;
  DharmaStreak: number;
  MentalFocus: number;
  Buildings: Building[];
  CityGrid: GridCell[];
  BattleLog: LogEntry[];
  IsMultiplayer: boolean;
  ConnectedPlayers: number;
}

export default function GameDashboard() {
  const [gameState, setGameState] = React.useState<GameState>({
    PlayerName: "ARJUNA_PRO",
    KingdomName: "ARYAVARTA_PRIME",
    KingdomTier: 'Village',
    GlobalRank: "IMMORTAL I",
    WinRate: 98,
    Population: 150,
    Suvarna: 5000,
    Anna: 2000,
    Shakti: 100,
    Buildings: [
      { id: '1', name: 'Krishi Farm', cost: 100, type: 'farm', count: 0 },
      { id: '2', name: 'Gurukul', cost: 250, type: 'gurukul', count: 0 },
      { id: '3', name: 'Mandira', cost: 500, type: 'temple', count: 0 },
      { id: '4', name: 'Sainya Barracks', cost: 300, type: 'barracks', count: 0 },
    ],
    CityGrid: [
      { id: 0, buildingId: null, direction: 'NW (Vayuvya)' },
      { id: 1, buildingId: null, direction: 'N (Uttara)' },
      { id: 2, buildingId: null, direction: 'NE (Ishanya)' },
      { id: 3, buildingId: null, direction: 'W (Pashchima)' },
      { id: 4, buildingId: null, direction: 'Center (Brahma)' },
      { id: 5, buildingId: null, direction: 'E (Purva)' },
      { id: 6, buildingId: null, direction: 'SW (Nairutya)' },
      { id: 7, buildingId: null, direction: 'S (Dakshina)' },
      { id: 8, buildingId: null, direction: 'SE (Agneya)' },
    ],
    BattleLog: [{ id: Date.now(), text: "E-SPORTS MODE INITIALIZED: Welcome Commander.", type: "system" as const }],
    IsMultiplayer: false,
    ConnectedPlayers: 1,
    Sainya: 20,
    DietPref: 'Veg',
    SessionStartTime: Date.now(),
    DharmaStreak: 3,
    MentalFocus: 100
  });

  const [language, setLanguage] = React.useState<'EN' | 'HI' | 'SAN' | 'ES'>('EN');
  const [isMounted, setIsMounted] = React.useState(false);

  React.useEffect(() => {
    setIsMounted(true);
  }, []);

  const TRANSLATIONS = {
    EN: {
      territory: "TERRITORY", sabha: "SABHA (DARBAR)", war: "WAR COMMAND", health: "DHARMA HEALTH", pro: "PRO LEAGUE",
      vitality: "VITALITY INDEX", credits: "CREDITS (SUV)", actives: "ACTIVES (SAI)", rank: "RANKING",
      guide: "ANCIENT GUIDE", battle: "INITIATE DHARMA YUDDHA", train: "TRAIN SAINYA", wellness: "DHARMA & MANAS WELLNESS"
    },
    HI: {
      territory: "क्षेत्र", sabha: "सभा दरबार", war: "युद्ध कमान", health: "धर्म स्वास्थ्य", pro: "प्रो लीग",
      vitality: "जीवन शक्ति", credits: "स्वर्ण (SUV)", actives: "सैनिक (SAI)", rank: "रैंकिंग",
      guide: "प्राचीन मार्गदर्शिका", battle: "धर्म युद्ध शुरू करें", train: "सैनिक प्रशिक्षण", wellness: "धर्म और मानस कल्याण"
    },
    SAN: {
      territory: "क्षेत्रम्", sabha: "सभा", war: "युद्धम्", health: "धर्म स्वास्थ्य", pro: "वरिष्ठ सभा",
      vitality: "प्राण शक्ति", credits: "सुवर्ण", actives: "सैनिक", rank: "क्रम",
      guide: "प्राचीन ज्ञान", battle: "धर्म युद्धम् आरम्भ", train: "सैनिक शिक्षणम्", wellness: "मनः स्वास्थ्यम्"
    },
    ES: {
      territory: "TERRITORIO", sabha: "SABHA (CORTE)", war: "MANDO DE GUERRA", health: "SALUD DHARMA", pro: "LIGA PRO",
      vitality: "VITALIDAD", credits: "SUV (SUV)", actives: "SAI (SAI)", rank: "RANGO",
      guide: "GUÍA ANCIANA", battle: "INICIAR DHARMA YUDDHA", train: "ENTRENAR SAINYA", wellness: "BIENESTAR DHARMA"
    }
  };

  const t = (key: keyof typeof TRANSLATIONS['EN']) => TRANSLATIONS[language][key] || key;

  const [activeTab, setActiveTab] = React.useState<'VastuBuilder' | 'Multiplayer' | 'DharmaYuddha' | 'VedicHealth' | 'Sabha'>('VastuBuilder');
  const [isTrailerMode, setIsTrailerMode] = React.useState(false);
  const [showCredits, setShowCredits] = React.useState(false);
  const [selectedCell, setSelectedCell] = React.useState<number | null>(null);
  const [isCinemaMode, setIsCinemaMode] = React.useState(false);
  const [isScanning, setIsScanning] = React.useState(false);
  const [showGuide, setShowGuide] = React.useState(false);
  const [battleResult, setBattleResult] = React.useState<{ result: 'win' | 'loss', msg: string } | null>(null);

  // Reanimated HUD State
  const glowValue = useSharedValue(0.4);
  const scanLine = useSharedValue(-200);
  const rotation = useSharedValue(0);

  React.useEffect(() => {
    glowValue.value = withRepeat(withTiming(1, { duration: 2500 }), -1, true);
    scanLine.value = withRepeat(withTiming(800, { duration: 5000 }), -1, false);
    rotation.value = withRepeat(withTiming(360, { duration: 20000 }), -1, false);
  }, []);

  const animatedGlow = useAnimatedStyle(() => ({
    opacity: glowValue.value,
    shadowOpacity: interpolate(glowValue.value, [0.4, 1], [0.1, 0.6]),
    transform: [{ scale: interpolate(glowValue.value, [0.4, 1], [1, 1.02]) }]
  }));

  const animatedScan = useAnimatedStyle(() => ({
    transform: [{ translateY: scanLine.value }],
    opacity: interpolate(scanLine.value, [0, 300, 600], [0, 0.5, 0])
  }));

  const animatedOrb = useAnimatedStyle(() => ({
    transform: [{ rotate: `${rotation.value}deg` }]
  }));

  const { width } = useWindowDimensions();
  const isDesktop = width > 1024; // Pro Desktop standard
  const isTablet = width > 768 && width <= 1024;

  const fadeAnim = React.useRef(new BaseAnimated.Value(1)).current;
  const slideAnim = React.useRef(new BaseAnimated.Value(0)).current;

  // Universal Keyboard Shortcuts (Desktop/Laptop)
  React.useEffect(() => {
    if (Platform.OS === 'web') {
      const handleKeyPress = (e: KeyboardEvent) => {
        if (e.key.toLowerCase() === 'b') setActiveTab('VastuBuilder');
        if (e.key.toLowerCase() === 'w') setActiveTab('DharmaYuddha');
        if (e.key.toLowerCase() === 'p') setActiveTab('Multiplayer');
        if (e.key.toLowerCase() === 'm') setIsCinemaMode(!isCinemaMode);
      };
      window.addEventListener('keydown', handleKeyPress);
      return () => window.removeEventListener('keydown', handleKeyPress);
    }
  }, [isCinemaMode]);

  React.useEffect(() => {
    const interval = setInterval(() => {
      setGameState(prev => {
        let vastuShakti = 0;
        let vastuAnna = 0;

        // Dynamic Vastu Bonus scanning
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
          KingdomTier: newTier
        };
      });
    }, 3000);
    return () => clearInterval(interval);
  }, []);

  const trainSainya = () => {
    const barracksCount = gameState.Buildings.find(b => b.type === 'barracks')?.count || 0;
    if (barracksCount === 0) {
      setGameState(prev => ({ ...prev, BattleLog: [{ id: Date.now(), text: "Build Sainya Barracks first!", type: 'build' as const }, ...prev.BattleLog].slice(0, 5) }));
      return;
    }
    const cost = 50;
    if (gameState.Suvarna < cost) return;

    setGameState(prev => ({
      ...prev,
      Suvarna: prev.Suvarna - cost,
      Sainya: prev.Sainya + 10,
      BattleLog: [{ id: Date.now(), text: "Trained 10 Sainya Warriors.", type: 'combat' as const }, ...prev.BattleLog].slice(0, 5)
    }));
  };

  const startBattle = () => {
    const enemyPower = Math.floor(Math.random() * 50) + 10;
    const playerPower = (gameState.Sainya * 1.5) + (gameState.Shakti * 2);

    if (playerPower >= enemyPower) {
      const loot = 200 + Math.floor(Math.random() * 300);
      setGameState(prev => ({
        ...prev,
        Suvarna: prev.Suvarna + loot,
        BattleLog: [{ id: Date.now(), text: `VICTORY! Defeated Invaders. Looted ${loot} Suvarna.`, type: 'combat' as const }, ...prev.BattleLog].slice(0, 5)
      }));
      setBattleResult({ result: 'win', msg: `Victory! Your Dharma and Sainya prevailed.` });
    } else {
      setGameState(prev => ({
        ...prev,
        Sainya: Math.max(0, prev.Sainya - 5),
        BattleLog: [{ id: Date.now(), text: "DEFEAT! Your Sainya retreated.", type: 'combat' as const }, ...prev.BattleLog].slice(0, 5)
      }));
      setBattleResult({ result: 'loss', msg: `Defeat. You need more Sainya or Shakti.` });
    }
    setTimeout(() => setBattleResult(null), 3000);
  };

  const renderDharmaYuddha = () => (
    <View style={styles.warRoom}>
      <Text style={styles.sectionHeader}>{t('war')}</Text>

      <View style={styles.armyCard}>
        <View style={styles.statRow}>
          <MaterialCommunityIcons name="sword-cross" size={40} color="#FF4444" />
          <View>
            <Text style={styles.armyTitle}>{t('actives')}</Text>
            <Text style={styles.armyCount}>{gameState.Sainya} Warriors</Text>
          </View>
        </View>

        <View style={styles.powerGauge}>
          <View style={styles.powerInfo}>
            <Text style={styles.powerLabel}>{t('credits')} (SUV)</Text>
            <Text style={styles.powerVal}>+{(gameState.Shakti * 2).toFixed(0)} SHAKTI</Text>
          </View>
          <TouchableOpacity style={styles.trainButtonPro} onPress={trainSainya}>
            <Text style={styles.trainTextPro}>{t('train')}</Text>
          </TouchableOpacity>
        </View>
      </View>

      <TouchableOpacity style={styles.battleButtonPro} onPress={startBattle}>
        <Text style={styles.battleButtonTextPro}>{t('battle')}</Text>
      </TouchableOpacity>

      {battleResult && (
        <View style={[styles.battleAlertPro, { backgroundColor: battleResult.result === 'win' ? 'rgba(0, 255, 136, 0.9)' : 'rgba(255, 68, 68, 0.9)' }]}>
          <Text style={styles.alertTextPro}>{battleResult.msg}</Text>
        </View>
      )}
    </View>
  );

  const renderGuide = () => (
    <View style={styles.guideOverlay}>
      <View style={styles.guideContent}>
        <Text style={styles.guideHeader}>📜 Guru's Guidance</Text>
        <ScrollView style={styles.guideScroll}>
          <Text style={styles.guideSub}>1. Vastu Mandala (Building)</Text>
          <Text style={styles.guideText}>• Place Mandira in NE (Ishanya) for Divine Shakti.{"\n"}• Place Krishi Farm in NW (Vayuvya) for maximum Anna.</Text>

          <Text style={styles.guideSub}>2. Resources</Text>
          <Text style={styles.guideText}>• Anna: Feeds your growth and Sainya.{"\n"}• Suvarna: Currency for construction and training.{"\n"}• Shakti: Divine favor gained through Mandiras.</Text>

          <Text style={styles.guideSub}>3. Dharma Yuddha (War)</Text>
          <Text style={styles.guideText}>• Train Sainya in the War Room. Battles reward Suvarna but risk your Sainya. Combine force with Shakti for victory!</Text>
        </ScrollView>
        <TouchableOpacity style={styles.closeGuide} onPress={() => setShowGuide(false)}>
          <Text style={styles.closeGuideText}>I UNDERSTAND, GURU</Text>
        </TouchableOpacity>
      </View>
    </View>
  );

  const startTrailer = () => {
    setIsTrailerMode(true);
    setShowCredits(false);
    setTimeout(() => setShowCredits(true), 15000); // Transitions to credits after main pan
  };

  const renderTrailer = () => (
    <View style={styles.trailerOverlay}>
      {!showCredits ? (
        <View style={styles.trailerMain}>
          <Text style={styles.trailerSub}>TENJIKU DEVA YUDDHA: 8K PREVIEW</Text>
          <Text style={styles.trailerTitle}>THE EVOLUTION OF HUMANITY</Text>
          <View style={styles.trailerPan} />
          <Text style={styles.trailerTagline}>BUILD THE MANDALA. PROTECT THE DHARMA.</Text>
        </View>
      ) : (
        <View style={styles.creditsScreen}>
          <Text style={styles.creditsHeader}>CREATED BY</Text>
          <Text style={styles.studioName}>EVOLUTION STUDIOS</Text>
          <Text style={styles.creditsEvolution}>The Next Stage of Human Achievement.</Text>
          <TouchableOpacity style={styles.closeTrailer} onPress={() => setIsTrailerMode(false)}>
            <Text style={styles.closeTrailerText}>RE-ENTER REALITY</Text>
          </TouchableOpacity>
        </View>
      )}
    </View>
  );

  const performMeditation = () => {
    setGameState(prev => ({
      ...prev,
      MentalFocus: Math.min(100, prev.MentalFocus + 20),
      Shakti: prev.Shakti + 10,
      BattleLog: [{ id: Date.now(), text: "MEDITATION COMPLETE: +10 Shakti (Manas Bonus)", type: 'system' as const }, ...prev.BattleLog].slice(0, 5)
    }));
    Alert.alert("Meditation", "Deep Breathing... Your mind is now focused for battle.");
  };

  const renderVedicHealth = () => (
    <View style={styles.warRoom}>
      <View style={styles.battleHeaderPro}>
        <Text style={styles.sectionHeader}>{t('wellness')}</Text>
        <View style={styles.streakBadge}>
          <MaterialCommunityIcons name="fire" size={14} color="#FFD700" />
          <Text style={styles.streakText}>{gameState.DharmaStreak} DAY STREAK (+15% Shakti)</Text>
        </View>
      </View>

      <View style={styles.armyCard}>
        <Text style={styles.healthHeader}>🧠 Manas (Mind) Training</Text>
        <Text style={styles.guideText}>• Ekagrata (Concentration): Focus on the center icon for 30s to reset "Tilt".{"\n"}• Bhramari: Close ears and hum for 10 reps to lower stress after a defeat.{"\n"}• Cognitive Clarity: Visualize your city's mandala before any Dharma Yuddha.</Text>
        <TouchableOpacity style={styles.meditateBtn} onPress={performMeditation}>
          <Text style={styles.meditateBtnText}>START FOCUS SESSION (2 Min)</Text>
        </TouchableOpacity>
      </View>

      <View style={[styles.armyCard, { borderColor: '#FFD700' }]}>
        <Text style={styles.healthHeader}>🥗 Diet: {gameState.DietPref.toUpperCase()} MODE</Text>
        <View style={styles.healthRow}>
          {gameState.DietPref === 'Veg' ? (
            <Text style={styles.guideText}>• Ashwagandha Milk: Repair nervous system after 6+ hrs.{"\n"}• Almonds & Saffron: Natural cognitive boosters.</Text>
          ) : (
            <Text style={styles.guideText}>• Grilled Protein: Muscle fuel for sitting endurance.{"\n"}• Brahmi Extract: Peak synaptic speed.</Text>
          )}
        </View>
      </View>

      <View style={styles.sessionCard}>
        <View style={styles.focusBar}><View style={[styles.focusInner, { width: `${gameState.MentalFocus}%` }]} /></View>
        <Text style={styles.focusVal}>MENTAL FOCUS INDEX: {gameState.MentalFocus}%</Text>
      </View>
    </View>
  );

  const constructBuilding = (buildingId: string) => {
    if (selectedCell === null) return;

    setGameState(prev => {
      const building = prev.Buildings.find(b => b.id === buildingId);
      if (!building || prev.Suvarna < building.cost) {
        return {
          ...prev,
          BattleLog: [{ id: Date.now(), text: "Insufficient Suvarna!", type: "build" as const }, ...prev.BattleLog].slice(0, 5)
        };
      }

      const newGrid = [...prev.CityGrid];
      const cell = newGrid[selectedCell];

      let bonusText = "";
      if (building.type === 'temple' && cell.direction.includes('NE')) bonusText = " (Vastu Bonus: +Shakti)";
      if (building.type === 'farm' && cell.direction.includes('NW')) bonusText = " (Vastu Bonus: +Anna)";
      if (building.type === 'barracks' && cell.direction.includes('SW')) bonusText = " (Vastu Bonus: +Defense)";

      newGrid[selectedCell] = { ...cell, buildingId };

      return {
        ...prev,
        Suvarna: prev.Suvarna - building.cost,
        Buildings: prev.Buildings.map(b => b.id === buildingId ? { ...b, count: b.count + 1, cost: Math.floor(b.cost * 1.3) } : b),
        CityGrid: newGrid,
        BattleLog: [{ id: Date.now(), text: `Constructed ${building.name} in ${cell.direction}${bonusText}.`, type: "build" as const }, ...prev.BattleLog].slice(0, 5)
      };
    });
    setSelectedCell(null);
  };

  const renderVastuBuilder = () => (
    <BaseAnimated.View style={[styles.buildGrid, { opacity: fadeAnim, transform: [{ translateY: slideAnim }] }]}>
      {!isCinemaMode && (
        <View style={styles.statsPanel}>
          <View style={styles.statBox}>
            <Text style={styles.statLabel}>TIER</Text>
            <Text style={styles.statValue}>{gameState.KingdomTier.toUpperCase()}</Text>
          </View>
          <View style={styles.statBox}>
            <Text style={styles.statLabel}>VITALITY</Text>
            <Text style={styles.statValue}>{Math.floor(gameState.Anna)}</Text>
          </View>
        </View>
      )}

      <View style={styles.gridHeader}>
        <View>
          <Text style={styles.sectionHeader}>VASTU PURUSHA MANDALA</Text>
          <Text style={styles.gridSubHeader}>Strategic City Matrix • Tier {gameState.KingdomTier}</Text>
        </View>
        <TouchableOpacity style={styles.cinemaToggle} onPress={() => setIsCinemaMode(!isCinemaMode)}>
          <Reanimated.View style={animatedGlow}>
            <Ionicons name={isCinemaMode ? "eye-off" : "aperture"} size={22} color="#00F0FF" />
          </Reanimated.View>
          <Text style={styles.cinemaText}>{isCinemaMode ? "REALITY MODE" : "COMMAND VIEW"}</Text>
        </TouchableOpacity>
      </View>

      <View style={styles.mandalaContainer}>
        <View style={styles.mandalaGrid}>
          {/* Background Atma Orb */}
          <Reanimated.View style={[styles.atmaOrb, animatedOrb]} />

          {gameState.CityGrid.map((cell, idx) => {
            const building = gameState.Buildings.find(b => b.id === cell.buildingId);
            const isSelected = selectedCell === idx;

            return (
              <TouchableOpacity
                key={cell.id}
                style={[styles.gridCell, isSelected && styles.gridCellSelected]}
                onPress={() => setSelectedCell(isSelected ? null : idx)}
              >
                {building ? (
                  <View style={styles.cellContent}>
                    {building.type === 'farm' && <MaterialCommunityIcons name="corn" size={40} color="#FFD700" />}
                    {building.type === 'gurukul' && <FontAwesome5 name="book-reader" size={30} color="#FFD700" />}
                    {building.type === 'temple' && <MaterialCommunityIcons name="temple-hindu" size={40} color="#FFD700" />}
                    {building.type === 'barracks' && <MaterialCommunityIcons name="shield-sword" size={40} color="#FFD700" />}
                    {!isCinemaMode && <Text style={styles.cellBuildingName}>{building.name}</Text>}
                  </View>
                ) : (
                  <View style={styles.emptyCell}>
                    <Text style={styles.directionTag}>{cell.direction.split(' ')[0]}</Text>
                    {!isCinemaMode && <Ionicons name="add-circle-outline" size={24} color="rgba(255,215,0,0.3)" />}
                  </View>
                )}
              </TouchableOpacity>
            );
          })}
        </View>

        {selectedCell !== null && (
          <View style={styles.buildPicker}>
            <Text style={styles.pickerTitle}>Build in {gameState.CityGrid[selectedCell].direction}</Text>
            <ScrollView horizontal showsHorizontalScrollIndicator={false}>
              {gameState.Buildings.map(b => (
                <TouchableOpacity key={b.id} style={styles.pickerCard} onPress={() => constructBuilding(b.id)}>
                  <View style={styles.pickerIcon}>
                    {b.type === 'farm' && <MaterialCommunityIcons name="corn" size={24} color="#FFD700" />}
                    {b.type === 'gurukul' && <FontAwesome5 name="book-reader" size={20} color="#FFD700" />}
                    {b.type === 'temple' && <MaterialCommunityIcons name="temple-hindu" size={24} color="#FFD700" />}
                    {b.type === 'barracks' && <MaterialCommunityIcons name="shield-sword" size={24} color="#FFD700" />}
                  </View>
                  <Text style={styles.pickerName}>{b.name}</Text>
                  <Text style={styles.pickerCost}>💰 {b.cost}</Text>
                </TouchableOpacity>
              ))}
            </ScrollView>
          </View>
        )}
      </View>

      {!isCinemaMode && (
        <View style={styles.logSection}>
          <Text style={styles.logHeader}>Dharma Log</Text>
          {gameState.BattleLog.map(log => (
            <Text key={log.id} style={[styles.logText, { color: log.type === 'build' ? '#FFD700' : '#888' }]}>
              {log.text}
            </Text>
          ))}
        </View>
      )}
    </BaseAnimated.View>
  );

  const renderProLeague = () => {
    const leaderboard = [
      { rank: 1, name: "KRISHNA_77", elo: 2850, status: "LIVE", region: "IND" },
      { rank: 2, name: "YUDHISTHIRA_DEV", elo: 2710, status: "IDLE", region: "IND" },
      { rank: 3, name: "ASHWATHAMA_X", elo: 2690, status: "IN BATTLE", region: "GLB" },
      { rank: 4, name: "ARJUNA_PRO (YOU)", elo: 2450, status: "ACTIVE", region: "IND" },
      { rank: 5, name: "BHISHMA_PITA", elo: 2420, status: "IDLE", region: "IND" },
    ];

    return (
      <View style={styles.warRoom}>
        <View style={styles.battleHeaderPro}>
          <Text style={styles.sectionHeader}>Global Pro Leaderboard</Text>
          <View style={[styles.proBadge, { backgroundColor: '#FFD700' }]}>
            <Text style={[styles.proBadgeText, { color: '#000' }]}>SEASON 1</Text>
          </View>
        </View>

        <View style={styles.leaderboardCard}>
          <View style={styles.leaderboardHeader}>
            <Text style={styles.lbHeaderCol}>RANK</Text>
            <Text style={[styles.lbHeaderCol, { flex: 2 }]}>COMMANDER</Text>
            <Text style={styles.lbHeaderCol}>ELO</Text>
            <Text style={styles.lbHeaderCol}>STATUS</Text>
          </View>
          {leaderboard.map((player) => (
            <View key={player.rank} style={[styles.lbRow, player.name.includes('YOU') && styles.lbRowActive]}>
              <Text style={styles.lbRankText}>#{player.rank}</Text>
              <View style={{ flex: 2, flexDirection: 'row', alignItems: 'center', gap: 8 }}>
                <View style={styles.regionFlag}><Text style={{ fontSize: 8, color: '#000', fontWeight: 'bold' }}>{player.region}</Text></View>
                <Text style={styles.lbNameText}>{player.name}</Text>
              </View>
              <Text style={styles.lbEloText}>{player.elo}</Text>
              <View style={[styles.statusTag, { backgroundColor: player.status === 'LIVE' ? 'rgba(255, 68, 68, 0.2)' : 'rgba(255,255,255,0.05)' }]}>
                <Text style={[styles.statusText, { color: player.status === 'LIVE' ? '#FF4444' : '#666' }]}>{player.status}</Text>
              </View>
            </View>
          ))}
        </View>

        <TouchableOpacity style={styles.shareChannelBtn}>
          <FontAwesome5 name="youtube" size={16} color="#000" />
          <Text style={styles.shareChannelText}>CONNECT YOUTUBE CHANNEL</Text>
        </TouchableOpacity>
      </View>
    );
  };

  const renderSabha = () => {
    const ministers = [
      { name: "Birbal", role: "Finance (Arthashastra)", impact: "+20% Suvarna" },
      { name: "Tansen", role: "Culture (Gandharva)", impact: "+15% Shakti" },
      { name: "Todar Mal", role: "Land (Vastu)", impact: "+10% Build Speed" }
    ];

    return (
      <View style={styles.warRoom}>
        <Text style={styles.sectionHeader}>Sabha: The Royal Darbar</Text>
        <View style={styles.ministerGrid}>
          {ministers.map((m, i) => (
            <View key={i} style={styles.ministerCard}>
              <MaterialCommunityIcons name="crown" size={30} color="#FFD700" />
              <Text style={styles.pickerName}>{m.name}</Text>
              <Text style={styles.lbHeaderCol}>{m.role}</Text>
              <Text style={styles.lbEloText}>{m.impact}</Text>
            </View>
          ))}
        </View>

        <View style={styles.petitionCard}>
          <Text style={styles.healthHeader}>📜 Citizen Petition</Text>
          <Text style={styles.guideText}>"The farmers of Vayuvya seek a tax relief due to the recent monsoon delay. How shall we proceed?"</Text>
          <View style={styles.petitionActions}>
            <TouchableOpacity style={styles.trainButtonPro} onPress={() => Alert.alert("Sabha", "Dharma +10: You showed mercy.")}>
              <Text style={styles.trainTextPro}>GRANT RELIEF</Text>
            </TouchableOpacity>
            <TouchableOpacity style={[styles.trainButtonPro, { backgroundColor: '#FF4444' }]} onPress={() => Alert.alert("Sabha", "Suvarna +500: Treasury enriched.")}>
              <Text style={styles.trainTextPro}>DENY RELIEF</Text>
            </TouchableOpacity>
          </View>
        </View>
      </View>
    );
  };

  if (!isMounted) return null;

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.tournamentTicker}>
        <Text style={styles.tickerText}>🏆 LIVE TOURNAMENT: ARYAVARTA PRIME OPEN | PRIZE: 1.5M SUVARNA | CURRENT LEAD: KRISHNA_77</Text>
      </View>

      <View style={isDesktop ? styles.desktopLayout : styles.mobileLayout}>
        <View style={isDesktop ? styles.sidebar : styles.topNav}>
          <View style={styles.logoContainer}>
            <Image source={{ uri: 'https://images.unsplash.com/photo-1614027164847-1b2809eb18bc?q=80&w=200' }} style={styles.proLogo} />
            <View>
              <Text style={styles.headerTitlePro}>EVOLUTION</Text>
              <Text style={styles.headerSubPro}>STUDIOS • DEVA YUDDHA</Text>
            </View>
          </View>

          <View style={styles.navSection}>
            <TouchableOpacity onPress={() => setActiveTab('VastuBuilder')} style={[styles.navItemPro, activeTab === 'VastuBuilder' && styles.navItemActivePro]}>
              <Entypo name="grid" size={16} color={activeTab === 'VastuBuilder' ? "#00F0FF" : "#666"} />
              <Text style={[styles.navTextPro, activeTab === 'VastuBuilder' && styles.navTextActivePro]}>{t('territory')}</Text>
            </TouchableOpacity>
            <TouchableOpacity onPress={() => setActiveTab('Sabha')} style={[styles.navItemPro, activeTab === 'Sabha' && styles.navItemActivePro]}>
              <MaterialCommunityIcons name="account-group" size={16} color={activeTab === 'Sabha' ? "#FFD700" : "#666"} />
              <Text style={[styles.navTextPro, activeTab === 'Sabha' && styles.navTextActivePro]}>{t('sabha')}</Text>
            </TouchableOpacity>
            <TouchableOpacity onPress={() => setActiveTab('DharmaYuddha')} style={[styles.navItemPro, activeTab === 'DharmaYuddha' && styles.navItemActivePro]}>
              <MaterialCommunityIcons name="radar" size={16} color={activeTab === 'DharmaYuddha' ? "#FF4444" : "#666"} />
              <Text style={[styles.navTextPro, activeTab === 'DharmaYuddha' && styles.navTextActivePro]}>{t('war')}</Text>
            </TouchableOpacity>
            <TouchableOpacity onPress={() => setActiveTab('VedicHealth')} style={[styles.navItemPro, activeTab === 'VedicHealth' && styles.navItemActivePro]}>
              <MaterialCommunityIcons name="leaf" size={16} color={activeTab === 'VedicHealth' ? "#00FF88" : "#666"} />
              <Text style={[styles.navTextPro, activeTab === 'VedicHealth' && styles.navTextActivePro]}>{t('health')}</Text>
            </TouchableOpacity>
            <TouchableOpacity onPress={() => setActiveTab('Multiplayer')} style={[styles.navItemPro, activeTab === 'Multiplayer' && styles.navItemActivePro]}>
              <MaterialCommunityIcons name="trophy-variant" size={16} color={activeTab === 'Multiplayer' ? "#FFD700" : "#666"} />
              <Text style={[styles.navTextPro, activeTab === 'Multiplayer' && styles.navTextActivePro]}>{t('pro')}</Text>
            </TouchableOpacity>
          </View>

          <View style={styles.langSwitcher}>
            {['EN', 'HI', 'SAN', 'ES'].map(lang => (
              <TouchableOpacity key={lang} onPress={() => setLanguage(lang as any)} style={[styles.langBtn, language === lang && styles.langBtnActive]}>
                <Text style={[styles.langText, language === lang && styles.langTextActive]}>{lang}</Text>
              </TouchableOpacity>
            ))}
          </View>

          <View style={styles.proProfile}>
            <Text style={styles.proRankLabel}>{t('rank')}</Text>
            <Reanimated.Text style={[styles.proRankVal, animatedGlow]}>{gameState.GlobalRank}</Reanimated.Text>
          </View>

          <TouchableOpacity onPress={() => setShowGuide(true)} style={styles.guideBtnSidePro}>
            <Text style={styles.guideBtnTextPro}>📜 {t('guide')}</Text>
          </TouchableOpacity>
        </View>

        <View style={styles.content}>
          {Platform.OS === 'web' && <View style={styles.webDivineMesh} />}
          <ScrollView>
            <View style={[styles.mainContainerPro, { maxWidth: isDesktop ? (isCinemaMode ? 1400 : 1200) : '100%' }]}>
              {/* Top Pro Resource Monitor */}
              <View style={styles.topResourceRowPro}>
                <Reanimated.View style={[styles.resMiniPro, animatedGlow]}>
                  <Text style={styles.resLabelPro}>{t('vitality')}</Text>
                  <Text style={styles.resValPro}>{Math.floor(gameState.Anna)}</Text>
                </Reanimated.View>
                <Reanimated.View style={[styles.resMiniPro, animatedGlow, { borderLeftColor: '#FFD700', borderColor: 'rgba(255, 215, 0, 0.2)' }]}>
                  <Text style={styles.resLabelPro}>{t('credits')}</Text>
                  <Text style={[styles.resValPro, { color: '#FFD700' }]}>{Math.floor(gameState.Suvarna)}</Text>
                </Reanimated.View>
                <Reanimated.View style={[styles.resMiniPro, animatedGlow, { borderLeftColor: '#FF4444', borderColor: 'rgba(255, 68, 68, 0.2)' }]}>
                  <Text style={styles.resLabelPro}>{t('actives')}</Text>
                  <Text style={[styles.resValPro, { color: '#FF4444' }]}>{gameState.Sainya}</Text>
                </Reanimated.View>
              </View>

              {activeTab === 'VastuBuilder' && renderVastuBuilder()}
              {activeTab === 'Sabha' && renderSabha()}
              {activeTab === 'DharmaYuddha' && renderDharmaYuddha()}
              {activeTab === 'VedicHealth' && renderVedicHealth()}
              {activeTab === 'Multiplayer' && renderProLeague()}

              {/* Divine Scanline HUD Overlay */}
              <Reanimated.View
                pointerEvents="none"
                style={[styles.scanLineOverlay, animatedScan]}
              />
            </View>
          </ScrollView>

          {/* YouTube Video Export Button and Trailer Launch */}
          <View style={styles.exportControls}>
            <TouchableOpacity
              style={[styles.exportBtn, isCinemaMode && styles.exportBtnActive]}
              onPress={() => Alert.alert("Export", "CINEMATIC VIDEO GENERATING... Ready for YouTube upload in 30s.")}
            >
              <View style={styles.recDot} />
              <Text style={styles.exportBtnText}>RECORD</Text>
            </TouchableOpacity>

            <TouchableOpacity
              style={styles.trailerLaunchBtn}
              onPress={startTrailer}
            >
              <Entypo name="video" size={14} color="#FFD700" />
              <Text style={styles.trailerLaunchText}>8K TRAILER</Text>
            </TouchableOpacity>
          </View>
        </View>
      </View>
      {showGuide && renderGuide()}
      {isTrailerMode && renderTrailer()}
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#020408' },
  desktopLayout: { flex: 1, flexDirection: 'row' },
  mobileLayout: { flex: 1, flexDirection: 'column' },
  sidebar: {
    width: 300,
    backgroundColor: 'rgba(18, 22, 29, 0.95)',
    borderRightWidth: 1,
    borderRightColor: 'rgba(0, 240, 255, 0.2)',
    padding: 25,
    ...Platform.select({
      web: { backdropFilter: 'blur(20px)' }
    })
  },
  topNav: { backgroundColor: '#12161D', borderBottomWidth: 1, borderBottomColor: '#B8860B', padding: 20 },
  headerTitle: { color: '#FFD700', fontSize: 24, fontWeight: '900', letterSpacing: 1 },
  headerSubtitle: { color: '#888', fontSize: 13, textTransform: 'uppercase', marginBottom: 30 },
  navItem: { padding: 16, borderRadius: 12, marginBottom: 8 },
  navItemActive: { backgroundColor: 'rgba(184, 134, 11, 0.12)', borderLeftWidth: 4, borderLeftColor: '#FFD700' },
  navText: { color: '#E6EDF3', fontSize: 15, fontWeight: '700' },
  content: { flex: 1 },
  mainContainer: { padding: Platform.OS === 'web' ? 40 : 20, maxWidth: 1200, alignSelf: 'center', width: '100%' },

  topResourceRow: { flexDirection: 'row', gap: 20, marginBottom: 30 },
  resMini: { backgroundColor: '#1C2128', padding: 15, borderRadius: 12, flex: 1, borderWidth: 1, borderColor: '#30363D' },
  resLabel: { color: '#888', fontSize: 10, fontWeight: '900', letterSpacing: 1 },
  resVal: { color: '#E6EDF3', fontSize: 20, fontWeight: 'bold', marginTop: 5 },

  buildGrid: { flex: 1 },
  statsPanel: { flexDirection: 'row', gap: 15, marginBottom: 30 },
  statBox: {
    backgroundColor: 'rgba(5, 7, 10, 0.7)',
    padding: 24,
    borderRadius: 16,
    borderLeftWidth: 4,
    borderLeftColor: '#00F0FF',
    flex: 1,
    borderWidth: 1,
    borderColor: 'rgba(0, 240, 255, 0.15)',
    ...Platform.select({
      web: { backdropFilter: 'blur(15px)', cursor: 'pointer' }
    }),
    shadowColor: '#00F0FF',
    shadowOffset: { width: 0, height: 0 },
    shadowOpacity: 0.2,
    shadowRadius: 20,
    boxShadow: '0 0 20px rgba(0, 240, 255, 0.2)',
    transform: [{ skewX: '-10deg' }]
  },
  statLabel: { color: 'rgba(0, 240, 255, 0.6)', fontSize: 10, fontWeight: '900', letterSpacing: 2.5, textTransform: 'uppercase' },

  gridSubHeader: { color: '#666', fontSize: 11, fontWeight: 'bold', marginTop: 4, letterSpacing: 1 },
  statValue: {
    // Your existing properties (e.g., fontSize, color)
    fontSize: 16,
    color: 'white',

    // Replace the single 'textShadow' line with these three:
    textShadowColor: 'rgba(0, 0, 0, 0.5)',
    textShadowOffset: { width: 2, height: 2 },
    textShadowRadius: 4,
  },
  gridHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 },
  cinemaToggle: { flexDirection: 'row', alignItems: 'center', gap: 8, backgroundColor: 'rgba(255, 215, 0, 0.1)', paddingHorizontal: 15, paddingVertical: 8, borderRadius: 20 },
  cinemaText: { color: '#FFD700', fontSize: 12, fontWeight: 'bold' },

  sectionHeader: { color: '#FFF', fontSize: 22, fontWeight: 'bold' },
  mandalaContainer: { position: 'relative', marginBottom: 40 },
  mandalaGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 15,
    aspectRatio: 1,
    width: '100%',
    maxWidth: 600,
    alignSelf: 'center',
    padding: 20,
    backgroundColor: 'rgba(18, 22, 29, 0.98)',
    borderRadius: 32,
    borderWidth: 1,
    borderColor: 'rgba(0, 240, 255, 0.3)',
    transform: [{ perspective: 1500 }, { rotateX: '24deg' }, { rotateZ: '-3deg' }],
    overflow: 'hidden',
    ...Platform.select({
      web: { shadowColor: '#00F0FF', shadowRadius: 50, shadowOpacity: 0.1 }
    })
  },
  atmaOrb: {
    position: 'absolute',
    top: '20%',
    left: '20%',
    width: 300,
    height: 300,
    borderRadius: 150,
    backgroundColor: 'rgba(0, 240, 255, 0.04)',
    borderWidth: 1,
    borderColor: 'rgba(0, 240, 255, 0.08)',
    zIndex: -1
  },
  gridCell: {
    width: '30.5%',
    aspectRatio: 1,
    backgroundColor: 'rgba(10, 13, 18, 0.8)',
    borderRadius: 20,
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 1,
    borderColor: 'rgba(255, 255, 255, 0.05)',
    shadowColor: '#00F0FF',
    shadowOpacity: 0.1,
    shadowRadius: 15,
    boxShadow: '0 0 15px rgba(0, 240, 255, 0.1)'
  },
  gridCellSelected: {
    borderColor: '#FFD700',
    backgroundColor: 'rgba(255, 215, 0, 0.1)',
    borderWidth: 2
  },
  cellContent: { alignItems: 'center', gap: 5 },
  cellBuildingName: { color: '#888', fontSize: 10, fontWeight: 'bold', textAlign: 'center' },
  emptyCell: { alignItems: 'center', opacity: 0.6 },
  directionTag: { color: '#586069', fontSize: 9, fontWeight: '900', marginBottom: 4 },

  buildPicker: {
    position: 'absolute',
    bottom: -10,
    left: 0,
    right: 0,
    backgroundColor: '#12161D',
    padding: 20,
    borderRadius: 24,
    borderWidth: 1,
    borderColor: '#B8860B',
    elevation: 10,
    zIndex: 100
  },
  pickerTitle: { color: '#FFF', fontSize: 16, fontWeight: 'bold', marginBottom: 15 },
  pickerCard: { backgroundColor: '#1C2128', padding: 15, borderRadius: 16, marginRight: 15, width: 120, alignItems: 'center', borderWidth: 1, borderColor: '#30363D' },
  pickerIcon: { marginBottom: 10 },
  pickerName: { color: '#FFF', fontSize: 12, fontWeight: 'bold', marginBottom: 5 }, 
  pickerCost: { color: '#FFD700', fontSize: 10, fontWeight: 'bold' },

  logSection: { marginTop: 20, backgroundColor: 'rgba(0,0,0,0.3)', padding: 20, borderRadius: 16 },
  logHeader: { color: '#FFF', fontSize: 14, fontWeight: 'bold', marginBottom: 10, opacity: 0.8 },
  logText: { fontSize: 12, color: '#ccc', marginBottom: 5, lineHeight: 18 },

  tournamentTicker: { backgroundColor: '#FFD700', paddingVertical: 6 },
  tickerText: { color: '#000', fontSize: 10, fontWeight: '900', textAlign: 'center', letterSpacing: 1 },

  logoContainer: { flexDirection: 'row', alignItems: 'center', gap: 12, marginBottom: 40 },
  proLogo: { width: 44, height: 44, borderRadius: 10, borderColor: '#00F0FF', borderWidth: 1 },
  headerTitlePro: { color: '#FFF', fontSize: 24, fontWeight: '900', letterSpacing: 2 },
  headerSubPro: { color: '#00F0FF', fontSize: 10, fontWeight: 'bold' },

  navSection: { gap: 10 },
  navItemPro: { flexDirection: 'row', alignItems: 'center', gap: 12, padding: 14, borderRadius: 12, backgroundColor: 'rgba(255,255,255,0.02)' },
  navItemActivePro: { backgroundColor: 'rgba(0, 240, 255, 0.08)', borderWidth: 1, borderColor: 'rgba(0, 240, 255, 0.3)' },
  navTextPro: { color: '#666', fontSize: 12, fontWeight: 'bold', letterSpacing: 1 },
  navTextActivePro: { color: '#FFF' },

  proProfile: { marginTop: 'auto', padding: 20, backgroundColor: 'rgba(0,0,0,0.3)', borderRadius: 16, marginBottom: 20 },
  proRankLabel: { color: '#666', fontSize: 9, fontWeight: 'bold' },
  proRankVal: { color: '#FFD700', fontSize: 16, fontWeight: '900', marginTop: 4 },

  guideBtnSidePro: { padding: 15, borderRadius: 12, borderWidth: 1, borderColor: 'rgba(255,215,0,0.3)', alignItems: 'center' },
  guideBtnTextPro: { color: '#FFD700', fontSize: 12, fontWeight: 'bold' },

  mainContainerPro: { padding: Platform.OS === 'web' ? 40 : 20, maxWidth: 1200, alignSelf: 'center', width: '100%' },
  topResourceRowPro: { flexDirection: 'row', flexWrap: 'wrap', gap: 15, marginBottom: 30 },
  resMiniPro: {
    minWidth: 100,
    flex: 1,
    padding: 16,
    borderRadius: 4,
    backgroundColor: 'rgba(10, 13, 18, 0.9)',
    borderLeftWidth: 4,
    borderLeftColor: '#00F0FF',
    borderWidth: 1,
    borderColor: 'rgba(0, 240, 255, 0.2)',
    transform: [{ skewX: '-10deg' }],
  },
  resLabelPro: { color: 'rgba(0, 240, 255, 0.6)', fontSize: 8, fontWeight: 'bold', marginBottom: 6, letterSpacing: 1.5, textTransform: 'uppercase' },
  resValPro: { color: '#FFF', fontSize: 24, fontWeight: '900', letterSpacing: 1 },

  battleHeaderPro: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 },
  proBadge: { paddingHorizontal: 10, paddingVertical: 4, borderRadius: 4, backgroundColor: '#FF4444' },
  proBadgeText: { color: '#FFF', fontSize: 8, fontWeight: '900' },
  proIconContainer: { width: 60, height: 60, borderRadius: 12, backgroundColor: 'rgba(0, 240, 255, 0.05)', justifyContent: 'center', alignItems: 'center', borderWidth: 1, borderColor: 'rgba(0, 240, 255, 0.2)' },
  rankBadge: { marginLeft: 'auto', alignItems: 'flex-end' },
  rankLabel: { color: '#666', fontSize: 9, fontWeight: 'bold' },
  rankValue: { color: '#00FF88', fontSize: 20, fontWeight: '900' },

  powerInfo: { flex: 1 },
  powerVal: { color: '#FFF', fontSize: 18, fontWeight: 'bold' },
  trainButtonPro: { backgroundColor: '#00F0FF', paddingHorizontal: 25, paddingVertical: 14, borderRadius: 4, transform: [{ skewX: '-15deg' }] },
  trainTextPro: { color: '#000', fontWeight: '900', fontSize: 13, letterSpacing: 2, transform: [{ skewX: '15deg' }] },
  battleButtonPro: { backgroundColor: '#FF4444', padding: 22, borderRadius: 4, alignItems: 'center', transform: [{ skewX: '-10deg' }] },
  battleButtonTextPro: { color: '#FFF', fontWeight: '900', fontSize: 20, letterSpacing: 4, transform: [{ skewX: '10deg' }] },
  battleAlertPro: { padding: 20, borderRadius: 12, borderWidth: 1, marginTop: 20, alignItems: 'center', backgroundColor: 'rgba(5,7,10,0.8)' },
  alertTextPro: { fontWeight: '900', fontSize: 14, letterSpacing: 1 },

  leaderboardCard: { backgroundColor: '#1C2128', borderRadius: 24, padding: 15, borderWidth: 1, borderColor: '#30363D' },
  leaderboardHeader: { flexDirection: 'row', paddingBottom: 15, borderBottomWidth: 1, borderBottomColor: '#30363D', marginBottom: 10 },
  lbHeaderCol: { flex: 1, color: '#666', fontSize: 10, fontWeight: 'bold' },
  lbRow: { flexDirection: 'row', alignItems: 'center', paddingVertical: 12, borderBottomWidth: 1, borderBottomColor: 'rgba(255,255,255,0.05)' },
  lbRowActive: { backgroundColor: 'rgba(0, 240, 255, 0.05)', borderRadius: 12, marginHorizontal: -5, paddingHorizontal: 5 },
  lbRankText: { flex: 1, color: '#888', fontWeight: 'bold' },
  lbNameText: { color: '#FFF', fontWeight: 'bold', fontSize: 13 },
  lbEloText: { flex: 1, color: '#FFD700', fontWeight: '900' },
  regionFlag: { backgroundColor: '#FFD700', paddingHorizontal: 4, paddingVertical: 2, borderRadius: 2 },
  statusTag: { paddingHorizontal: 10, paddingVertical: 4, borderRadius: 20 },
  statusText: { fontSize: 9, fontWeight: 'bold' },

  shareChannelBtn: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', gap: 10, backgroundColor: '#FFD700', padding: 18, borderRadius: 16, marginTop: 20 },
  shareChannelText: { color: '#000', fontWeight: '900', fontSize: 14 },

  exportBtn: { position: 'absolute', bottom: Platform.OS === 'ios' ? 40 : 20, right: 20, backgroundColor: '#12161D', paddingHorizontal: 25, paddingVertical: 15, borderRadius: 30, borderWidth: 1, borderColor: '#FF4444', flexDirection: 'row', alignItems: 'center', gap: 10, elevation: 5, shadowColor: '#000', shadowOpacity: 0.3, shadowRadius: 10 },
  exportBtnActive: { borderColor: '#00FF88', backgroundColor: 'rgba(0, 255, 136, 0.1)' },
  exportBtnText: { color: '#FFF', fontWeight: 'bold', fontSize: 12 },
  recDot: { width: 10, height: 10, borderRadius: 5, backgroundColor: '#FF4444' },

  healthHeader: { color: '#FFD700', fontSize: 16, fontWeight: 'bold', marginBottom: 15 },
  healthRow: { padding: 5 },
  sessionCard: { backgroundColor: 'rgba(0,0,0,0.5)', padding: 20, borderRadius: 16, alignItems: 'center', marginTop: 10 },
  sessionLabel: { color: '#666', fontSize: 9, fontWeight: 'bold' },
  sessionVal: { color: '#FFF', fontSize: 24, fontWeight: '900', marginTop: 5 },

  streakBadge: { flexDirection: 'row', alignItems: 'center', gap: 6, backgroundColor: 'rgba(255, 215, 0, 0.15)', paddingHorizontal: 12, paddingVertical: 6, borderRadius: 20 },
  streakText: { color: '#FFD700', fontSize: 10, fontWeight: '900' },
  meditateBtn: { backgroundColor: '#00F0FF', padding: 15, borderRadius: 12, marginTop: 15, alignItems: 'center' },
  meditateBtnText: { color: '#000', fontWeight: 'bold', fontSize: 12 },
  focusBar: { height: 6, backgroundColor: '#1C2128', width: '80%', borderRadius: 3, marginTop: 10, overflow: 'hidden' },
  focusInner: { height: '100%', backgroundColor: '#00F0FF', shadowColor: '#00F0FF', shadowRadius: 10, shadowOpacity: 0.8, boxShadow: '0 0 10px #00F0FF' },
  focusVal: { color: '#00F0FF', fontSize: 12, fontWeight: 'bold', marginTop: 5 },


  trailerOverlay: { ...StyleSheet.absoluteFillObject, backgroundColor: '#000', zIndex: 10000, justifyContent: 'center', alignItems: 'center' },
  trailerMain: { alignItems: 'center', padding: 40 },
  trailerTitle: { color: '#FFF', fontSize: 42, fontWeight: '900', letterSpacing: 8, textAlign: 'center', marginBottom: 20 },
  trailerSub: { color: '#FFD700', fontSize: 12, fontWeight: 'bold', letterSpacing: 3, marginBottom: 10 },
  trailerTagline: { color: '#00F0FF', fontSize: 14, fontWeight: 'bold', opacity: 0.8 },
  trailerPan: { width: 1, height: 100, backgroundColor: 'rgba(255,215,0,0.3)', marginVertical: 30 },

  creditsScreen: { alignItems: 'center', padding: 40 },
  creditsHeader: { color: '#888', fontSize: 13, fontWeight: 'bold', letterSpacing: 5, marginBottom: 10 },
  studioName: { color: '#FFD700', fontSize: 32, fontWeight: '900', letterSpacing: 2, marginBottom: 20 },
  creditsEvolution: { color: '#FFF', fontSize: 16, fontWeight: 'bold', opacity: 0.6, fontStyle: 'italic' },
  closeTrailer: { marginTop: 50, padding: 15, borderRadius: 12, borderWidth: 1, borderColor: '#333' },
  closeTrailerText: { color: '#666', fontSize: 12, fontWeight: 'bold' },

  exportControls: { position: 'absolute', bottom: 30, right: 30, flexDirection: 'row', gap: 10 },
  trailerLaunchBtn: { backgroundColor: 'rgba(255, 215, 0, 0.1)', paddingHorizontal: 20, paddingVertical: 15, borderRadius: 30, borderWidth: 1, borderColor: '#FFD700', flexDirection: 'row', alignItems: 'center', gap: 10 },
  trailerLaunchText: { color: '#FFD700', fontWeight: 'bold', fontSize: 12 },

  warRoom: { flex: 1 },
  armyCard: { backgroundColor: '#1C2128', padding: 25, borderRadius: 24, borderWidth: 1, borderColor: '#30363D', marginBottom: 20 },
  statRow: { flexDirection: 'row', alignItems: 'center', gap: 20, marginBottom: 20 },
  armyTitle: { color: '#888', fontSize: 13, fontWeight: 'bold' },
  armyCount: { color: '#FFF', fontSize: 32, fontWeight: '900' },
  powerGauge: { borderTopWidth: 1, borderTopColor: '#30363D', paddingTop: 20, flexDirection: 'row', alignItems: 'center' },
  powerLabel: { color: '#00F0FF', fontSize: 10, fontWeight: 'bold', marginBottom: 6 },

  guideOverlay: { ...StyleSheet.absoluteFillObject, backgroundColor: 'rgba(0,0,0,0.85)', justifyContent: 'center', alignItems: 'center', zIndex: 9999, padding: 20 },
  guideContent: { backgroundColor: '#12161D', width: '100%', maxWidth: 500, borderRadius: 32, padding: 30, borderWidth: 1, borderColor: '#FFD700' },
  guideHeader: { color: '#FFD700', fontSize: 24, fontWeight: 'bold', marginBottom: 20, textAlign: 'center' },
  guideScroll: { maxHeight: 400 },
  guideSub: { color: '#FFD700', fontSize: 16, fontWeight: 'bold', marginTop: 15, marginBottom: 5 },
  guideText: { color: '#CCC', fontSize: 14, lineHeight: 22 },
  closeGuide: { backgroundColor: '#FFD700', padding: 18, borderRadius: 16, marginTop: 20, alignItems: 'center' },
  closeGuideText: { color: '#000', fontWeight: 'bold', letterSpacing: 1 },
  ministerGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: 15, marginTop: 20 },
  ministerCard: { flex: 1, minWidth: 150, backgroundColor: '#12161D', padding: 20, borderRadius: 20, borderWidth: 1, borderColor: 'rgba(255,215,0,0.2)', alignItems: 'center' },
  petitionCard: { marginTop: 30, backgroundColor: 'rgba(255, 215, 0, 0.05)', padding: 30, borderRadius: 24, borderWidth: 1, borderColor: '#FFD700' },
  petitionActions: { flexDirection: 'row', gap: 15, marginTop: 25 },
  langSwitcher: { flexDirection: 'row', gap: 8, marginVertical: 20, flexWrap: 'wrap' },
  langBtn: { paddingHorizontal: 10, paddingVertical: 6, borderRadius: 6, backgroundColor: 'rgba(255,255,255,0.05)', borderWidth: 1, borderColor: '#333' },
  langBtnActive: { borderColor: '#00F0FF', backgroundColor: 'rgba(0, 240, 255, 0.1)' },
  langText: { color: '#666', fontSize: 10, fontWeight: 'bold' },
  langTextActive: { color: '#FFF' },
  scanLineOverlay: {
    position: 'absolute',
    left: 0,
    right: 0,
    height: 120,
    backgroundColor: 'rgba(0, 240, 255, 0.05)',
    borderBottomWidth: 2,
    borderBottomColor: 'rgba(0, 240, 255, 0.2)',
    zIndex: 9999
  },
  webDivineMesh: {
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    backgroundColor: '#020408',
    opacity: 0.5,
    zIndex: -2,
  },
});
