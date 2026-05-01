using UnityEngine;

namespace TenjikuDevaYuddha.Core
{
    /// <summary>
    /// Main in-game HUD using Unity's IMGUI system for rapid prototyping.
    /// Displays resources, level, age, and provides access to build menu, Rudra powers, etc.
    /// Will be replaced with UI Toolkit for production.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        // UI state
        private bool _showBuildMenu = false;
        private bool _showRudraPowers = false;
        private bool _showVedaResearch = false;
        private bool _showAvatarProgress = false;
        private bool _showBattleMenu = false;
        private bool _showArmyMenu = false;
        private bool _showDarbarMenu = false;
        private int _darbarTab = 0; // 0=Petitions, 1=Praja, 2=Ministers, 3=Laws
        private string _notification = "";
        private float _notifTimer = 0f;

        private Vector2 _buildMenuScroll;
        private Vector2 _battleMenuScroll;
        private Vector2 _darbarMenuScroll;

        // Styles
        private GUIStyle _headerStyle;
        private GUIStyle _notifStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _resourceStyle;
        private bool _stylesInitialized;

        public void Initialize()
        {
            GameEvents.OnNotification += OnNotification;
            GameEvents.OnResourceChanged += (_, _, _) => { }; // triggers repaint
        }

        private void OnDestroy()
        {
            GameEvents.OnNotification -= OnNotification;
        }

        private void InitStyles()
        {
            if (_stylesInitialized) return;
            _stylesInitialized = true;

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.85f, 0.3f) }
            };

            _notifStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal = { textColor = Color.white }
            };

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };

            _resourceStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = Color.white }
            };
        }

        private void OnGUI()
        {
            if (GameManager.Instance?.CurrentState == null) return;
            InitStyles();

            var state = GameManager.Instance.CurrentState;

            // Dark background tint for UI areas
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, 100), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // ─── Top Bar: Resources ───
            DrawResourceBar(state);

            // ─── Player Info Bar ───
            DrawPlayerInfo(state);

            // ─── Bottom: Action Buttons ───
            DrawActionButtons(state);

            // ─── Panels ───
            if (_showBuildMenu) DrawBuildMenu(state);
            if (_showRudraPowers) DrawRudraPowers(state);
            if (_showVedaResearch) DrawVedaResearch(state);
            if (_showAvatarProgress) DrawAvatarProgress(state);
            if (_showBattleMenu) DrawBattleMenu(state);
            if (_showArmyMenu) DrawArmyMenu(state);
            if (_showDarbarMenu) DrawDarbarMenu(state);

            // ─── Notifications ───
            DrawNotification();

            // ─── ESC to exit build mode ───
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (BuildingSystem.Instance?.IsInBuildMode == true)
                    BuildingSystem.Instance.ExitBuildMode();
                CloseAllPanels();
            }
        }

        // ─────────────────────────────────────────────
        //  Resource Bar (Top)
        // ─────────────────────────────────────────────
        private void DrawResourceBar(GameState state)
        {
            float x = 10;
            float y = 5;
            float w = (Screen.width - 20) / 8f;

            foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
            {
                float current = state.Resources.Get(type);
                float max = state.MaxResources.Get(type);
                string icon = GameConstants.RESOURCE_ICONS[type];
                float rate = ResourceManager.Instance?.GetProductionRate(type) ?? 0;
                string rateStr = rate > 0 ? $" +{rate:F0}/t" : "";

                GUI.Label(new Rect(x, y, w, 20), $"{icon} {type}", _resourceStyle);
                GUI.Label(new Rect(x, y + 18, w, 20), $"{current:F0}/{max:F0}{rateStr}", _resourceStyle);

                // Mini progress bar
                GUI.color = new Color(0.3f, 0.3f, 0.3f);
                GUI.DrawTexture(new Rect(x, y + 38, w - 10, 4), Texture2D.whiteTexture);
                GUI.color = GetResourceColor(type);
                GUI.DrawTexture(new Rect(x, y + 38, (w - 10) * Mathf.Clamp01(current / max), 4), Texture2D.whiteTexture);
                GUI.color = Color.white;

                x += w;
            }
        }

        private Color GetResourceColor(ResourceType type) => type switch
        {
            ResourceType.Suvarna => new Color(1f, 0.84f, 0f),
            ResourceType.Anna => new Color(0.55f, 0.8f, 0.2f),
            ResourceType.Pashana => new Color(0.6f, 0.55f, 0.5f),
            ResourceType.Kashtha => new Color(0.6f, 0.4f, 0.2f),
            ResourceType.Loha => new Color(0.5f, 0.5f, 0.6f),
            ResourceType.Shakti => new Color(0.9f, 0.3f, 0.9f),
            ResourceType.Vidya => new Color(0.3f, 0.7f, 1f),
            ResourceType.Praja => new Color(0.9f, 0.9f, 0.5f),
            _ => Color.white
        };

        // ─────────────────────────────────────────────
        //  Player Info (Below resource bar)
        // ─────────────────────────────────────────────
        private void DrawPlayerInfo(GameState state)
        {
            float y = 50;
            var avatar = GameConstants.AVATARS[state.CurrentAvatarAge];

            // Level & Title
            GUI.Label(new Rect(10, y, 300, 22),
                $"👤 {state.PlayerName} | Lv.{state.PlayerLevel} {state.PlayerTitle}", _resourceStyle);

            // Level progress bar
            float lvlProgress = state.GetLevelProgress();
            GUI.color = new Color(0.2f, 0.2f, 0.2f);
            GUI.DrawTexture(new Rect(10, y + 20, 200, 6), Texture2D.whiteTexture);
            GUI.color = new Color(0.3f, 0.8f, 0.3f);
            GUI.DrawTexture(new Rect(10, y + 20, 200 * lvlProgress, 6), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Avatar Age
            GUI.Label(new Rect(220, y, 400, 22),
                $"🕉️ Age of {avatar.Name} — {avatar.Title}", _resourceStyle);

            // Kingdom name
            GUI.Label(new Rect(Screen.width - 250, y, 240, 22),
                $"🏛️ {state.KingdomName}", _resourceStyle);

            // Battles unlocked indicator
            if (state.BattlesUnlocked)
            {
                GUI.color = new Color(1f, 0.3f, 0.3f);
                GUI.Label(new Rect(Screen.width - 250, y + 18, 240, 20),
                    $"⚔️ Battles Unlocked | Won: {state.BattleRecord.BattlesWon}", _resourceStyle);
                GUI.color = Color.white;
            }
            else
            {
                GUI.Label(new Rect(Screen.width - 250, y + 18, 240, 20),
                    $"⚔️ Battles unlock at Lv.200 ({200 - state.PlayerLevel} levels away)", _resourceStyle);
            }

            // Active effects
            if (GameManager.Instance.IsDamaruActive)
            {
                GUI.color = new Color(1f, 1f, 0.3f);
                GUI.Label(new Rect(Screen.width / 2 - 100, y, 200, 20),
                    $"🥁 Damaru: {GameManager.Instance.DamaruTimeRemaining:F0}s", _resourceStyle);
                GUI.color = Color.white;
            }
            if (GameManager.Instance.IsMeditationActive)
            {
                GUI.color = new Color(0.6f, 0.8f, 1f);
                GUI.Label(new Rect(Screen.width / 2 - 100, y + 18, 200, 20),
                    $"🧘 Meditating: {GameManager.Instance.MeditationTimeRemaining:F0}s", _resourceStyle);
                GUI.color = Color.white;
            }
        }

        // ─────────────────────────────────────────────
        //  Action Buttons (Bottom)
        // ─────────────────────────────────────────────
        private void DrawActionButtons(GameState state)
        {
            float btnW = 120;
            float btnH = 40;
            float y = Screen.height - btnH - 10;
            float x = 10;

            // Dark background for bottom bar
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.DrawTexture(new Rect(0, y - 5, Screen.width, btnH + 15), Texture2D.whiteTexture);
            GUI.color = Color.white;

            if (GUI.Button(new Rect(x, y, btnW, btnH), "🏗️ Build", _buttonStyle))
                TogglePanel(ref _showBuildMenu);
            x += btnW + 5;

            if (GUI.Button(new Rect(x, y, btnW, btnH), "🔱 Rudra", _buttonStyle))
                TogglePanel(ref _showRudraPowers);
            x += btnW + 5;

            if (GUI.Button(new Rect(x, y, btnW, btnH), "📜 Vedas", _buttonStyle))
                TogglePanel(ref _showVedaResearch);
            x += btnW + 5;

            if (GUI.Button(new Rect(x, y, btnW, btnH), "🕉️ Avatar", _buttonStyle))
                TogglePanel(ref _showAvatarProgress);
            x += btnW + 5;

            Color savedColor = GUI.color;
            if (!state.BattlesUnlocked) GUI.color = new Color(0.5f, 0.5f, 0.5f);
            if (GUI.Button(new Rect(x, y, btnW, btnH), "⚔️ Battle", _buttonStyle))
            {
                if (state.BattlesUnlocked)
                    TogglePanel(ref _showBattleMenu);
                else
                    OnNotification($"⚔️ Battles unlock at Level 200! (Current: {state.PlayerLevel})");
            }
            GUI.color = savedColor;
            x += btnW + 5;

            if (GUI.Button(new Rect(x, y, btnW, btnH), "🛡️ Army", _buttonStyle))
                TogglePanel(ref _showArmyMenu);
            x += btnW + 5;

            if (GUI.Button(new Rect(x, y, btnW, btnH), "⚖️ Darbar", _buttonStyle))
                TogglePanel(ref _showDarbarMenu);
            x += btnW + 5;

            // Right side: Save & Menu
            if (GUI.Button(new Rect(Screen.width - 130, y, 120, btnH), "💾 Save", _buttonStyle))
            {
                GameManager.Instance.SaveGame(0);
                OnNotification("💾 Game saved!");
            }

            // Advance age button (if eligible)
            if (DashavatarManager.Instance != null && DashavatarManager.Instance.CanAdvanceAge())
            {
                GUI.color = new Color(1f, 0.9f, 0.3f);
                if (GUI.Button(new Rect(Screen.width - 270, y, 130, btnH), "🕉️ ADVANCE AGE!", _buttonStyle))
                {
                    DashavatarManager.Instance.AdvanceAge();
                }
                GUI.color = Color.white;
            }
        }

        // ─────────────────────────────────────────────
        //  Build Menu Panel
        // ─────────────────────────────────────────────
        private void DrawBuildMenu(GameState state)
        {
            float panelW = 400;
            float panelH = 400;
            float panelX = 10;
            float panelY = Screen.height - panelH - 60;

            GUI.Box(new Rect(panelX, panelY, panelW, panelH), "");

            // Dark background
            GUI.color = new Color(0.1f, 0.08f, 0.15f, 0.95f);
            GUI.DrawTexture(new Rect(panelX, panelY, panelW, panelH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(panelX + 10, panelY + 5, 300, 25), "🏗️ BUILD MENU", _headerStyle);

            // Category tabs
            float tabY = panelY + 30;
            float tabW = 90;
            string[] categories = { "Village", "City", "Kingdom", "Divine" };
            BuildingCategory[] cats = { BuildingCategory.Village, BuildingCategory.City, BuildingCategory.Kingdom, BuildingCategory.Divine };

            // Scrollable building list
            _buildMenuScroll = GUI.BeginScrollView(
                new Rect(panelX + 5, tabY + 5, panelW - 10, panelH - 40),
                _buildMenuScroll,
                new Rect(0, 0, panelW - 30, 1500));

            float yy = 0;
            var buildings = BuildingDatabase.GetBuildingsForAge(state.CurrentAvatarAge);
            foreach (var def in buildings)
            {
                bool canAfford = ResourceManager.Instance.HasEnough(def.Cost);

                // Building card
                GUI.color = canAfford ? new Color(0.15f, 0.15f, 0.25f) : new Color(0.2f, 0.1f, 0.1f);
                GUI.DrawTexture(new Rect(0, yy, panelW - 30, 70), Texture2D.whiteTexture);
                GUI.color = Color.white;

                // Category icon
                string catIcon = def.Category switch
                {
                    BuildingCategory.Village => "🏘️",
                    BuildingCategory.City => "🏙️",
                    BuildingCategory.Kingdom => "🏰",
                    BuildingCategory.Divine => "🕉️",
                    _ => "🏗️"
                };

                GUI.Label(new Rect(5, yy + 2, panelW - 100, 20),
                    $"{catIcon} {def.Name}", _resourceStyle);
                GUI.Label(new Rect(5, yy + 20, panelW - 100, 18),
                    def.Description.Length > 60 ? def.Description[..60] + "..." : def.Description,
                    new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = Color.gray } });

                // Cost summary
                string costStr = "";
                foreach (var c in def.Cost)
                    costStr += $"{GameConstants.RESOURCE_ICONS[c.Key]}{c.Value:F0} ";
                GUI.Label(new Rect(5, yy + 38, panelW - 100, 18),
                    $"Cost: {costStr}",
                    new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = new Color(1f, 0.8f, 0.4f) } });

                // Production info
                string prodStr = "";
                foreach (var p in def.ProductionPerTick)
                    prodStr += $"+{p.Value:F0}{GameConstants.RESOURCE_ICONS[p.Key]} ";
                if (!string.IsNullOrEmpty(prodStr))
                {
                    GUI.Label(new Rect(5, yy + 52, panelW - 100, 18),
                        $"Produces: {prodStr}",
                        new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = new Color(0.4f, 1f, 0.4f) } });
                }

                // Build button
                GUI.enabled = canAfford;
                if (GUI.Button(new Rect(panelW - 95, yy + 15, 60, 35), "Build"))
                {
                    BuildingSystem.Instance.EnterBuildMode(def.TypeId);
                    _showBuildMenu = false;
                }
                GUI.enabled = true;

                yy += 75;
            }

            GUI.EndScrollView();
        }

        // ─────────────────────────────────────────────
        //  Rudra Powers Panel
        // ─────────────────────────────────────────────
        private void DrawRudraPowers(GameState state)
        {
            float panelW = 350;
            float panelH = 350;
            float panelX = 140;
            float panelY = Screen.height - panelH - 60;

            GUI.color = new Color(0.12f, 0.05f, 0.15f, 0.95f);
            GUI.DrawTexture(new Rect(panelX, panelY, panelW, panelH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(panelX + 10, panelY + 5, 300, 25), "🔱 RUDRA POWERS", _headerStyle);

            float yy = panelY + 35;
            foreach (var kvp in GameConstants.RUDRA_POWERS)
            {
                var type = kvp.Key;
                var info = kvp.Value;
                float cd = RudraPowerSystem.Instance?.GetCooldownRemaining(type) ?? 0;
                bool isReady = cd <= 0;

                // Power card
                GUI.color = isReady ? new Color(0.2f, 0.1f, 0.25f) : new Color(0.1f, 0.1f, 0.1f);
                GUI.DrawTexture(new Rect(panelX + 5, yy, panelW - 10, 55), Texture2D.whiteTexture);
                GUI.color = Color.white;

                string icon = type switch
                {
                    RudraPowerType.Tandava => "🕺",
                    RudraPowerType.TrishulStrike => "🔱",
                    RudraPowerType.ThirdEye => "👁️",
                    RudraPowerType.DamaruBeat => "🥁",
                    RudraPowerType.Meditation => "🧘",
                    _ => "⚡"
                };

                GUI.Label(new Rect(panelX + 10, yy + 2, 250, 20), $"{icon} {info.Name}", _resourceStyle);
                GUI.Label(new Rect(panelX + 10, yy + 20, 250, 16), info.Description,
                    new GUIStyle(GUI.skin.label) { fontSize = 9, wordWrap = true, normal = { textColor = Color.gray } });
                GUI.Label(new Rect(panelX + 10, yy + 38, 150, 16),
                    $"Shakti: {info.ShaktiCost} | CD: {info.CooldownSeconds}s",
                    new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = new Color(0.9f, 0.3f, 0.9f) } });

                GUI.enabled = isReady;
                string btnText = isReady ? "Invoke" : $"{cd:F0}s";
                if (GUI.Button(new Rect(panelX + panelW - 70, yy + 12, 55, 30), btnText))
                {
                    bool needsTarget = RudraPowerSystem.Instance.RequiresTarget(type);
                    if (needsTarget)
                    {
                        // For now, use map center as target — in full version, player clicks map
                        var center = GridManager.Instance.GetMapCenter();
                        RudraPowerSystem.Instance.ActivatePower(type, center);
                    }
                    else
                    {
                        RudraPowerSystem.Instance.ActivatePower(type);
                    }
                }
                GUI.enabled = true;

                yy += 60;
            }
        }

        // ─────────────────────────────────────────────
        //  Veda Research Panel
        // ─────────────────────────────────────────────
        private void DrawVedaResearch(GameState state)
        {
            float panelW = 380;
            float panelH = 350;
            float panelX = 270;
            float panelY = Screen.height - panelH - 60;

            GUI.color = new Color(0.1f, 0.08f, 0.02f, 0.95f);
            GUI.DrawTexture(new Rect(panelX, panelY, panelW, panelH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(panelX + 10, panelY + 5, 300, 25), "📜 VEDA RESEARCH", _headerStyle);

            // Active research progress
            if (!string.IsNullOrEmpty(state.VedaProgress.ActiveResearchVeda))
            {
                float progress = state.VedaProgress.CurrentResearchProgress;
                GUI.Label(new Rect(panelX + 10, panelY + 30, 360, 20),
                    $"Researching: {state.VedaProgress.ActiveResearchVeda} ({progress * 100:F0}%)", _resourceStyle);

                GUI.color = new Color(0.2f, 0.2f, 0.2f);
                GUI.DrawTexture(new Rect(panelX + 10, panelY + 50, 360, 8), Texture2D.whiteTexture);
                GUI.color = new Color(0.3f, 0.8f, 1f);
                GUI.DrawTexture(new Rect(panelX + 10, panelY + 50, 360 * progress, 8), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            float yy = panelY + 65;
            foreach (var kvp in VedaResearchSystem.VEDA_INFO)
            {
                var veda = kvp.Key;
                var info = kvp.Value;
                int level = state.VedaProgress.GetLevel(veda);
                bool isMaxed = level >= VedaResearchSystem.MAX_VEDA_LEVEL;
                bool isResearching = state.VedaProgress.ActiveResearchVeda == veda.ToString();

                // Veda card
                GUI.color = new Color(info.Color.r * 0.3f, info.Color.g * 0.3f, info.Color.b * 0.3f, 0.9f);
                GUI.DrawTexture(new Rect(panelX + 5, yy, panelW - 10, 65), Texture2D.whiteTexture);
                GUI.color = Color.white;

                GUI.Label(new Rect(panelX + 10, yy + 2, 300, 20), $"{info.Name}", _resourceStyle);
                GUI.Label(new Rect(panelX + 10, yy + 20, 300, 16), info.Subtitle,
                    new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = info.Color } });

                GUI.Label(new Rect(panelX + 10, yy + 38, 200, 16),
                    $"Level: {level}/{VedaResearchSystem.MAX_VEDA_LEVEL}",
                    new GUIStyle(GUI.skin.label) { fontSize = 11, normal = { textColor = new Color(1f, 0.9f, 0.5f) } });

                // Level dots
                for (int i = 0; i < VedaResearchSystem.MAX_VEDA_LEVEL; i++)
                {
                    GUI.color = i < level ? info.Color : new Color(0.3f, 0.3f, 0.3f);
                    GUI.DrawTexture(new Rect(panelX + 100 + i * 14, yy + 40, 10, 10), Texture2D.whiteTexture);
                }
                GUI.color = Color.white;

                if (!isMaxed && !isResearching && string.IsNullOrEmpty(state.VedaProgress.ActiveResearchVeda))
                {
                    float cost = VedaResearchSystem.Instance.GetResearchCost(level);
                    if (GUI.Button(new Rect(panelX + panelW - 95, yy + 15, 80, 30),
                        $"📖 {cost:F0}V"))
                    {
                        VedaResearchSystem.Instance.StartResearch(veda);
                    }
                }
                else if (isMaxed)
                {
                    GUI.Label(new Rect(panelX + panelW - 90, yy + 20, 80, 25),
                        "✅ Mastered",
                        new GUIStyle(GUI.skin.label) { fontSize = 11, normal = { textColor = new Color(0.3f, 1f, 0.3f) } });
                }

                yy += 70;
            }
        }

        // ─────────────────────────────────────────────
        //  Avatar Progression Panel
        // ─────────────────────────────────────────────
        private void DrawAvatarProgress(GameState state)
        {
            float panelW = 500;
            float panelH = 250;
            float panelX = (Screen.width - panelW) / 2;
            float panelY = Screen.height / 2 - panelH / 2;

            GUI.color = new Color(0.05f, 0.05f, 0.15f, 0.95f);
            GUI.DrawTexture(new Rect(panelX, panelY, panelW, panelH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(panelX + 10, panelY + 5, 400, 25), "🕉️ DASHAVATAR — THE TEN DIVINE AGES", _headerStyle);

            // Timeline
            float timelineY = panelY + 40;
            float dotSize = 30;
            float spacing = (panelW - 40) / 10f;

            for (int i = 0; i < 10; i++)
            {
                var avatar = GameConstants.AVATARS[i];
                float x = panelX + 20 + i * spacing;
                bool isCurrent = i == state.CurrentAvatarAge;
                bool isUnlocked = i <= state.CurrentAvatarAge;

                // Connection line
                if (i > 0)
                {
                    GUI.color = isUnlocked ? avatar.PrimaryColor : new Color(0.3f, 0.3f, 0.3f);
                    GUI.DrawTexture(new Rect(x - spacing + dotSize / 2, timelineY + dotSize / 2 - 1, spacing - dotSize, 3), Texture2D.whiteTexture);
                }

                // Dot
                GUI.color = isUnlocked ? avatar.PrimaryColor : new Color(0.25f, 0.25f, 0.25f);
                if (isCurrent) GUI.color = Color.white;
                GUI.DrawTexture(new Rect(x, timelineY, dotSize, dotSize), Texture2D.whiteTexture);
                GUI.color = Color.white;

                // Name
                GUI.Label(new Rect(x - 10, timelineY + dotSize + 2, spacing, 16),
                    avatar.Name,
                    new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 9,
                        alignment = TextAnchor.UpperCenter,
                        normal = { textColor = isCurrent ? Color.white : (isUnlocked ? avatar.PrimaryColor : Color.gray) }
                    });
            }

            // Current age details
            var current = GameConstants.AVATARS[state.CurrentAvatarAge];
            float detailY = timelineY + dotSize + 30;

            GUI.Label(new Rect(panelX + 10, detailY, panelW - 20, 22),
                $"Current Age: {current.Name} — {current.Title}", _resourceStyle);
            GUI.Label(new Rect(panelX + 10, detailY + 22, panelW - 20, 40),
                current.Description,
                new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true, normal = { textColor = Color.gray } });

            // Next age requirements
            if (!DashavatarManager.Instance.IsMaxAge)
            {
                string reqs = DashavatarManager.Instance.GetNextAgeRequirements();
                float progress = DashavatarManager.Instance.GetAgeProgress();

                GUI.Label(new Rect(panelX + 10, detailY + 65, panelW - 20, 60),
                    reqs, new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true, normal = { textColor = new Color(1f, 0.8f, 0.4f) } });

                // Progress bar
                GUI.color = new Color(0.2f, 0.2f, 0.2f);
                GUI.DrawTexture(new Rect(panelX + 10, detailY + 115, panelW - 20, 10), Texture2D.whiteTexture);
                GUI.color = new Color(0.9f, 0.7f, 0.2f);
                GUI.DrawTexture(new Rect(panelX + 10, detailY + 115, (panelW - 20) * progress, 10), Texture2D.whiteTexture);
                GUI.color = Color.white;

                GUI.Label(new Rect(panelX + 10, detailY + 128, 200, 20),
                    $"Progress: {progress * 100:F0}%", _resourceStyle);
            }
        }

        // ─────────────────────────────────────────────
        //  Battle Menu Panel
        // ─────────────────────────────────────────────
        private void DrawBattleMenu(GameState state)
        {
            float panelW = 400;
            float panelH = 400;
            float panelX = Screen.width / 2 - panelW / 2;
            float panelY = Screen.height / 2 - panelH / 2;

            GUI.color = new Color(0.15f, 0.05f, 0.05f, 0.95f);
            GUI.DrawTexture(new Rect(panelX, panelY, panelW, panelH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(panelX + 10, panelY + 5, 350, 25), "⚔️ KURUKSHETRA — BATTLE", _headerStyle);

            // Army summary
            GUI.Label(new Rect(panelX + 10, panelY + 30, 380, 20),
                $"Army Strength: {state.Army.GetArmyStrength():F0} | Warriors: {state.Army.GetTotalWarriors()}", _resourceStyle);
            GUI.Label(new Rect(panelX + 10, panelY + 48, 380, 20),
                $"Formation: {state.Army.SelectedVyuha} | Astras: {state.Army.UnlockedAstras.Count}", _resourceStyle);

            // Formation selector
            GUI.Label(new Rect(panelX + 10, panelY + 70, 100, 20), "Vyuha:", _resourceStyle);
            float fyy = panelY + 68;
            float fx = panelX + 70;
            foreach (var vyuha in state.Army.UnlockedVyuhas)
            {
                bool isSelected = state.Army.SelectedVyuha == vyuha;
                GUI.color = isSelected ? new Color(1f, 0.8f, 0.3f) : Color.white;
                if (GUI.Button(new Rect(fx, fyy, 90, 22), vyuha))
                    state.Army.SelectedVyuha = vyuha;
                fx += 95;
                if (fx > panelX + panelW - 100) { fx = panelX + 70; fyy += 25; }
            }
            GUI.color = Color.white;

            // Available battles
            _battleMenuScroll = GUI.BeginScrollView(
                new Rect(panelX + 5, panelY + 130, panelW - 10, panelH - 140),
                _battleMenuScroll,
                new Rect(0, 0, panelW - 30, 800));

            float yy = 0;
            var battles = BattleSystem.Instance?.GetAvailableBattles();
            if (battles != null)
            {
                foreach (var option in battles)
                {
                    GUI.color = new Color(0.2f, 0.1f, 0.1f);
                    GUI.DrawTexture(new Rect(0, yy, panelW - 30, 50), Texture2D.whiteTexture);
                    GUI.color = Color.white;

                    GUI.Label(new Rect(5, yy + 2, 250, 20),
                        $"⚔️ {option.EnemyName}", _resourceStyle);
                    GUI.Label(new Rect(5, yy + 20, 250, 16),
                        $"Difficulty: {option.Difficulty} | Strength: {option.EstimatedEnemyStrength:F0}",
                        new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = Color.gray } });
                    GUI.Label(new Rect(5, yy + 34, 250, 16),
                        $"Reward: {option.GoldReward:F0}🪙  {option.XPReward}XP",
                        new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = new Color(1f, 0.8f, 0.3f) } });

                    if (GUI.Button(new Rect(panelW - 95, yy + 10, 60, 30), "Fight!"))
                    {
                        BattleSystem.Instance.StartBattle(BattleMode.Conquest, option.Difficulty);
                    }

                    yy += 55;
                }
            }

            GUI.EndScrollView();
        }

        // ─────────────────────────────────────────────
        //  Army Menu Panel
        // ─────────────────────────────────────────────
        private void DrawArmyMenu(GameState state)
        {
            float panelW = 350;
            float panelH = 350;
            float panelX = Screen.width / 2 + 50;
            float panelY = Screen.height / 2 - panelH / 2;

            GUI.color = new Color(0.1f, 0.1f, 0.05f, 0.95f);
            GUI.DrawTexture(new Rect(panelX, panelY, panelW, panelH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(panelX + 10, panelY + 5, 300, 25), "🛡️ ARMY", _headerStyle);

            float yy = panelY + 35;

            // Warrior types
            DrawWarriorRow(panelX, ref yy, "⚔️ Kshatriya (Infantry)", state.Army.Kshatriya, WarriorType.Kshatriya);
            DrawWarriorRow(panelX, ref yy, "🏹 Dhanurdhar (Archer)", state.Army.Dhanurdhar, WarriorType.Dhanurdhar);
            DrawWarriorRow(panelX, ref yy, "🐎 Ashvarohi (Cavalry)", state.Army.Ashvarohi, WarriorType.Ashvarohi);
            DrawWarriorRow(panelX, ref yy, "🏇 Rathi (Chariot)", state.Army.Rathi, WarriorType.Rathi);
            DrawWarriorRow(panelX, ref yy, "🐘 Gajasena (Elephant)", state.Army.Gajasena, WarriorType.Gajasena);

            // Total
            yy += 10;
            GUI.Label(new Rect(panelX + 10, yy, 300, 20),
                $"Total: {state.Army.GetTotalWarriors()} warriors | Strength: {state.Army.GetArmyStrength():F0}", _resourceStyle);

            // Unlocked Astras
            yy += 25;
            GUI.Label(new Rect(panelX + 10, yy, 300, 20), "Divine Weapons (Astras):", _resourceStyle);
            yy += 20;
            if (state.Army.UnlockedAstras.Count == 0)
            {
                GUI.Label(new Rect(panelX + 10, yy, 300, 20), "None — Research Atharvaveda to unlock!",
                    new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = Color.gray } });
            }
            else
            {
                string astraStr = string.Join(", ", state.Army.UnlockedAstras);
                GUI.Label(new Rect(panelX + 10, yy, panelW - 20, 50), astraStr,
                    new GUIStyle(GUI.skin.label) { fontSize = 10, wordWrap = true, normal = { textColor = new Color(1f, 0.6f, 0.2f) } });
            }
        }

        private void DrawWarriorRow(float panelX, ref float yy, string label, int count, WarriorType type)
        {
            GUI.Label(new Rect(panelX + 10, yy, 200, 20), $"{label}: {count}", _resourceStyle);

            if (GUI.Button(new Rect(panelX + 240, yy, 40, 20), "+1"))
                BattleSystem.Instance?.TrainWarriors(type, 1);
            if (GUI.Button(new Rect(panelX + 285, yy, 45, 20), "+10"))
                BattleSystem.Instance?.TrainWarriors(type, 10);

            yy += 25;
        }

        // ─────────────────────────────────────────────
        //  Darbar (Governance & Petitions) Panel
        // ─────────────────────────────────────────────
        private void DrawDarbarMenu(GameState state)
        {
            float panelW = 550;
            float panelH = 450;
            float panelX = Screen.width / 2 - panelW / 2;
            float panelY = Screen.height / 2 - panelH / 2;

            GUI.color = new Color(0.15f, 0.1f, 0.05f, 0.95f);
            GUI.DrawTexture(new Rect(panelX, panelY, panelW, panelH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(panelX + 10, panelY + 5, 400, 25), "⚖️ RAJA DARBAR — ROYAL COURT", _headerStyle);

            // Tabs
            string[] tabs = { "📜 Petitions", "👥 Praja (People)", "👑 Navaratna", "📜 Dharmaniti" };
            _darbarTab = GUI.Toolbar(new Rect(panelX + 10, panelY + 35, panelW - 20, 30), _darbarTab, tabs);

            float contentY = panelY + 70;
            float contentH = panelH - 80;

            if (_darbarTab == 0) // Petitions
            {
                var petitions = state.Governance.ActivePetitions;
                if (petitions.Count == 0)
                {
                    GUI.Label(new Rect(panelX + 10, contentY + 20, panelW - 20, 30), 
                        "The court is peaceful. No citizen problems today.", _resourceStyle);
                }
                else
                {
                    _darbarMenuScroll = GUI.BeginScrollView(
                        new Rect(panelX + 5, contentY, panelW - 10, contentH),
                        _darbarMenuScroll,
                        new Rect(0, 0, panelW - 30, petitions.Count * 90));

                    float yy = 0;
                    for (int i = 0; i < petitions.Count; i++)
                    {
                        var p = petitions[i];
                        var def = PetitionSystem.Instance?.GetPetitionTemplate(p.Title);
                        if (def == null) continue;

                        GUI.color = new Color(0.2f, 0.15f, 0.1f);
                        GUI.DrawTexture(new Rect(0, yy, panelW - 30, 85), Texture2D.whiteTexture);
                        GUI.color = Color.white;

                        GUI.Label(new Rect(5, yy + 2, 350, 20), $"⚠ {p.Title} ({p.Type})", _resourceStyle);
                        GUI.Label(new Rect(5, yy + 20, 400, 30), p.Description,
                            new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true, normal = { textColor = Color.gray } });

                        string costStr = "";
                        foreach (var c in def.ResolutionCost) costStr += $"{GameConstants.RESOURCE_ICONS[c.Key]}{c.Value:F0} ";
                        GUI.Label(new Rect(5, yy + 55, 300, 20), $"Cost: {costStr}",
                            new GUIStyle(GUI.skin.label) { fontSize = 11, normal = { textColor = new Color(1f, 0.6f, 0.3f) } });

                        GUI.Label(new Rect(panelW - 150, yy + 2, 100, 20), $"Time: {p.TimeRemainingSeconds:F0}s",
                            new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = Color.red } });

                        if (GUI.Button(new Rect(panelW - 105, yy + 40, 70, 30), "Resolve"))
                        {
                            PetitionSystem.Instance?.ResolvePetition(p.Id);
                        }

                        yy += 90;
                    }
                    GUI.EndScrollView();
                }
            }
            else if (_darbarTab == 1) // Praja
            {
                GUI.Label(new Rect(panelX + 10, contentY, panelW - 20, 20), 
                    "Assign your population to different vocational paths.", _resourceStyle);
                
                float k = state.Governance.KrishakAllocation * 100f;
                float s = state.Governance.ShilpiAllocation * 100f;
                float a = state.Governance.SainikAllocation * 100f;
                float v = state.Governance.VidvanAllocation * 100f;

                float yy = contentY + 30;
                GUI.Label(new Rect(panelX + 10, yy, 150, 20), $"🌾 Krishak (Farmers): {k:F0}%", _resourceStyle);
                k = GUI.HorizontalSlider(new Rect(panelX + 170, yy + 5, 200, 20), k, 0, 100);
                yy += 30;

                GUI.Label(new Rect(panelX + 10, yy, 150, 20), $"🛠️ Shilpi (Artisans): {s:F0}%", _resourceStyle);
                s = GUI.HorizontalSlider(new Rect(panelX + 170, yy + 5, 200, 20), s, 0, 100);
                yy += 30;

                GUI.Label(new Rect(panelX + 10, yy, 150, 20), $"🏹 Sainik (Guards): {a:F0}%", _resourceStyle);
                a = GUI.HorizontalSlider(new Rect(panelX + 170, yy + 5, 200, 20), a, 0, 100);
                yy += 30;

                GUI.Label(new Rect(panelX + 10, yy, 150, 20), $"📜 Vidvan (Scholars): {v:F0}%", _resourceStyle);
                v = GUI.HorizontalSlider(new Rect(panelX + 170, yy + 5, 200, 20), v, 0, 100);

                if (GUI.Button(new Rect(panelX + 390, contentY + 60, 120, 40), "Apply Roles"))
                {
                    GovernanceSystem.Instance?.SetPrajaAllocation(k, s, a, v);
                    GameEvents.ShowNotification("👥 Population Roles Updated!");
                }

                // Show multipliers
                yy += 60;
                if (GovernanceSystem.Instance != null) {
                    GUI.Label(new Rect(panelX + 10, yy, panelW - 20, 40), 
                        $"Food Prod: {GovernanceSystem.Instance.GetKrishakMultiplier():F1}x | Material Prod: {GovernanceSystem.Instance.GetShilpiMultiplier():F1}x\n" +
                        $"Army Speed: {GovernanceSystem.Instance.GetSainikMultiplier():F1}x | Vidya Prod: {GovernanceSystem.Instance.GetVidvanMultiplier():F1}x", 
                        new GUIStyle(GUI.skin.label) { fontSize = 12, normal = { textColor = new Color(0.8f, 1f, 0.8f) } });
                }
            }
            else if (_darbarTab == 2) // Ministers
            {
                _darbarMenuScroll = GUI.BeginScrollView(
                        new Rect(panelX + 5, contentY, panelW - 10, contentH),
                        _darbarMenuScroll,
                        new Rect(0, 0, panelW - 30, 9 * 50));

                float yy = 0;
                foreach (MinisterRole role in System.Enum.GetValues(typeof(MinisterRole)))
                {
                    bool isAppointed = GovernanceSystem.Instance?.IsMinisterAppointed(role) ?? false;
                    
                    GUI.color = isAppointed ? new Color(0.2f, 0.3f, 0.2f) : new Color(0.2f, 0.15f, 0.1f);
                    GUI.DrawTexture(new Rect(0, yy, panelW - 30, 45), Texture2D.whiteTexture);
                    GUI.color = Color.white;

                    GUI.Label(new Rect(5, yy + 10, 150, 25), role.ToString(), _resourceStyle);
                    
                    if (isAppointed)
                    {
                        string name = "";
                        if (role == MinisterRole.Purohita) name = state.Governance.Purohita;
                        else if (role == MinisterRole.Senapati) name = state.Governance.Senapati;
                        else if (role == MinisterRole.Amatya) name = state.Governance.Amatya;
                        // etc ... (Simplified display)
                        if (string.IsNullOrEmpty(name)) name = "Appointed";
                        GUI.Label(new Rect(160, yy + 10, 200, 25), $"✅ {name}", _resourceStyle);

                        if (GUI.Button(new Rect(panelW - 100, yy + 8, 60, 30), "Dismiss"))
                            GovernanceSystem.Instance?.DismissMinister(role);
                    }
                    else
                    {
                        GUI.Label(new Rect(160, yy + 10, 200, 25), "❌ Vacant", 
                            new GUIStyle(GUI.skin.label) { fontSize = 12, normal = { textColor = Color.red } });
                        if (GUI.Button(new Rect(panelW - 100, yy + 8, 60, 30), "Appoint"))
                            GovernanceSystem.Instance?.AppointMinister(role, "Elder");
                    }
                    yy += 50;
                }
                GUI.EndScrollView();
            }
            else if (_darbarTab == 3) // Laws
            {
                _darbarMenuScroll = GUI.BeginScrollView(
                        new Rect(panelX + 5, contentY, panelW - 10, contentH),
                        _darbarMenuScroll,
                        new Rect(0, 0, panelW - 30, GovernanceSystem.ALL_LAWS.Count * 65));

                float yy = 0;
                foreach (var law in GovernanceSystem.ALL_LAWS)
                {
                    bool isActive = GovernanceSystem.Instance?.IsLawActive(law.Id) ?? false;
                    bool isUnlocked = GovernanceSystem.Instance?.IsLawUnlocked(law.Id) ?? false;

                    GUI.color = isActive ? new Color(0.2f, 0.2f, 0.3f) : new Color(0.1f, 0.1f, 0.1f);
                    GUI.DrawTexture(new Rect(0, yy, panelW - 30, 60), Texture2D.whiteTexture);
                    GUI.color = Color.white;

                    GUI.Label(new Rect(5, yy + 2, 350, 20), $"⚖️ {law.Name} ({law.Source})", _resourceStyle);
                    GUI.Label(new Rect(5, yy + 22, 400, 35), law.Description,
                        new GUIStyle(GUI.skin.label) { fontSize = 10, wordWrap = true, normal = { textColor = Color.gray } });

                    if (isActive)
                    {
                        if (GUI.Button(new Rect(panelW - 100, yy + 15, 60, 30), "Revoke"))
                            GovernanceSystem.Instance?.RevokeLaw(law.Id);
                    }
                    else if (isUnlocked)
                    {
                        if (GUI.Button(new Rect(panelW - 100, yy + 15, 60, 30), "Enact"))
                            GovernanceSystem.Instance?.EnactLaw(law.Id);
                    }
                    else
                    {
                        GUI.Label(new Rect(panelW - 150, yy + 15, 120, 30), $"Requires: {law.Prerequisite}",
                            new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = Color.red } });
                    }
                    yy += 65;
                }
                GUI.EndScrollView();
            }
        }

        // ─────────────────────────────────────────────
        //  Notifications
        // ─────────────────────────────────────────────
        private void OnNotification(string message)
        {
            _notification = message;
            _notifTimer = 4f;
        }

        private void DrawNotification()
        {
            if (_notifTimer > 0)
            {
                _notifTimer -= Time.deltaTime;
                float alpha = Mathf.Clamp01(_notifTimer);
                GUI.color = new Color(0, 0, 0, 0.8f * alpha);
                float notifW = Mathf.Min(Screen.width * 0.6f, 500);
                float notifH = 60;
                float nx = (Screen.width - notifW) / 2;
                float ny = 110;
                GUI.DrawTexture(new Rect(nx, ny, notifW, notifH), Texture2D.whiteTexture);
                GUI.color = new Color(1, 1, 1, alpha);
                GUI.Label(new Rect(nx + 10, ny + 5, notifW - 20, notifH - 10), _notification, _notifStyle);
                GUI.color = Color.white;
            }
        }

        // ─────────────────────────────────────────────
        //  UI Helpers
        // ─────────────────────────────────────────────
        private void TogglePanel(ref bool flag)
        {
            bool newVal = !flag;
            CloseAllPanels();
            flag = newVal;
        }

        private void CloseAllPanels()
        {
            _showBuildMenu = false;
            _showRudraPowers = false;
            _showVedaResearch = false;
            _showAvatarProgress = false;
            _showBattleMenu = false;
            _showArmyMenu = false;
            _showDarbarMenu = false;
        }
    }
}
