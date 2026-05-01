using UnityEngine;
using UnityEngine.SceneManagement;

namespace TenjikuDevaYuddha.Core
{
    /// <summary>
    /// Main menu UI with New Game, Continue, and Settings options.
    /// Uses IMGUI for rapid prototyping — will be replaced with UI Toolkit later.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        private bool _showNewGame = false;
        private bool _showLoadGame = false;
        private bool _showSettings = false;

        private string _playerName = "Arjuna";
        private string _kingdomName = "Hastinapura";
        private string _selectedCityId = "Hastinapura";
        private Vector2 _cityScrollPos;

        // Styles
        private GUIStyle _titleStyle;
        private GUIStyle _subtitleStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _textFieldStyle;
        private bool _stylesInit;

        private void Start()
        {
            // Ensure singleton managers exist
            if (GameManager.Instance == null)
            {
                var gmObj = new GameObject("GameManager");
                gmObj.AddComponent<GameManager>();
                gmObj.AddComponent<SaveLoadManager>();
            }
            LegendaryCityDatabase.Initialize();
        }

        private void InitStyles()
        {
            if (_stylesInit) return;
            _stylesInit = true;

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 42,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.85f, 0.25f) }
            };

            _subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.8f, 0.7f, 0.5f) }
            };

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                fixedHeight = 50
            };

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = Color.white }
            };

            _textFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 16,
                fixedHeight = 30
            };
        }

        private void OnGUI()
        {
            InitStyles();

            // Full screen background
            GUI.color = new Color(0.05f, 0.03f, 0.1f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Decorative border
            DrawDecorativeBorder();

            float centerX = Screen.width / 2f;
            float y = Screen.height * 0.12f;

            // ─── Title ───
            GUI.Label(new Rect(0, y, Screen.width, 50), "天竺 देव युद्ध", _titleStyle);
            y += 55;
            GUI.Label(new Rect(0, y, Screen.width, 30),
                "TENJIKU DEVA YUDDHA", new GUIStyle(_subtitleStyle) { fontSize = 24 });
            y += 30;
            GUI.Label(new Rect(0, y, Screen.width, 25),
                "The Divine War of India", _subtitleStyle);
            y += 25;

            // OM symbol
            GUI.Label(new Rect(0, y, Screen.width, 40),
                "🕉️", new GUIStyle(_titleStyle) { fontSize = 36 });
            y += 50;

            // Subtitle description
            GUI.Label(new Rect(0, y, Screen.width, 22),
                "Build legendary kingdoms through the 10 Avatars of Lord Vishnu", _subtitleStyle);
            y += 22;
            GUI.Label(new Rect(0, y, Screen.width, 22),
                "Invoke Lord Rudra's divine powers • Master the Four Vedas", _subtitleStyle);
            y += 22;
            GUI.Label(new Rect(0, y, Screen.width, 22),
                "Wage epic Mahabharata-style battles on the fields of Kurukshetra", _subtitleStyle);
            y += 45;

            // ─── Menu Buttons ───
            float btnW = 280;
            float btnX = centerX - btnW / 2;

            if (!_showNewGame && !_showLoadGame && !_showSettings)
            {
                if (GUI.Button(new Rect(btnX, y, btnW, 50), "🏗️  New Kingdom", _buttonStyle))
                    _showNewGame = true;
                y += 60;

                bool hasSave = SaveLoadManager.Instance?.SaveExists(0) == true ||
                               SaveLoadManager.Instance?.AutoSaveExists() == true;
                GUI.enabled = hasSave;
                if (GUI.Button(new Rect(btnX, y, btnW, 50), "📂  Continue Journey", _buttonStyle))
                    _showLoadGame = true;
                GUI.enabled = true;
                y += 60;

                if (GUI.Button(new Rect(btnX, y, btnW, 50), "⚙️  Settings", _buttonStyle))
                    _showSettings = true;
                y += 60;

                if (GUI.Button(new Rect(btnX, y, btnW, 50), "🚪  Exit", _buttonStyle))
                {
                    #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                    #else
                    Application.Quit();
                    #endif
                }
            }

            // ─── New Game Panel ───
            if (_showNewGame)
                DrawNewGamePanel(centerX);

            // ─── Load Game Panel ───
            if (_showLoadGame)
                DrawLoadGamePanel(centerX);

            // ─── Settings Panel ───
            if (_showSettings)
                DrawSettingsPanel(centerX);

            // ─── Version ───
            GUI.Label(new Rect(10, Screen.height - 25, 300, 20),
                "v0.1.0 — Early Development",
                new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = Color.gray } });
        }

        private void DrawNewGamePanel(float centerX)
        {
            float panelW = 700;
            float panelH = 550;
            float px = centerX - panelW / 2;
            float py = Screen.height / 2 - panelH / 2;

            GUI.color = new Color(0.08f, 0.06f, 0.15f, 0.95f);
            GUI.DrawTexture(new Rect(px, py, panelW, panelH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(px + 10, py + 10, 680, 30), "🏗️ Found Your Kingdom", _titleStyle);

            float yy = py + 65;

            // ─── Player Name & Kingdom Name ───
            GUI.Label(new Rect(px + 20, yy, 120, 25), "Your Name:", _labelStyle);
            _playerName = GUI.TextField(new Rect(px + 140, yy, 200, 28), _playerName, _textFieldStyle);

            GUI.Label(new Rect(px + 360, yy, 120, 25), "Kingdom:", _labelStyle);
            _kingdomName = GUI.TextField(new Rect(px + 460, yy, 210, 28), _kingdomName, _textFieldStyle);
            yy += 40;

            // ─── Legendary City Selection ───
            GUI.Label(new Rect(px + 20, yy, 660, 22),
                "🏛️ Choose a Legendary City Archetype (from Mahabharata & Ramayana):", _labelStyle);
            yy += 25;

            // City cards in scrollable area
            var cities = LegendaryCityDatabase.GetAllCities();
            _cityScrollPos = GUI.BeginScrollView(
                new Rect(px + 10, yy, panelW - 20, 320),
                _cityScrollPos,
                new Rect(0, 0, panelW - 40, cities.Count * 82));

            float cy = 0;
            foreach (var city in cities)
            {
                bool isSelected = _selectedCityId == city.CityId;

                // Card background
                GUI.color = isSelected
                    ? new Color(city.PrimaryColor.r * 0.4f, city.PrimaryColor.g * 0.4f, city.PrimaryColor.b * 0.4f, 0.95f)
                    : new Color(0.12f, 0.10f, 0.18f, 0.9f);
                GUI.DrawTexture(new Rect(0, cy, panelW - 40, 75), Texture2D.whiteTexture);

                // Selection border
                if (isSelected)
                {
                    GUI.color = city.PrimaryColor;
                    GUI.DrawTexture(new Rect(0, cy, 4, 75), Texture2D.whiteTexture);
                    GUI.DrawTexture(new Rect(0, cy, panelW - 40, 2), Texture2D.whiteTexture);
                    GUI.DrawTexture(new Rect(0, cy + 73, panelW - 40, 2), Texture2D.whiteTexture);
                }
                GUI.color = Color.white;

                // City name
                GUI.Label(new Rect(12, cy + 3, 350, 20), city.Name,
                    new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold,
                        normal = { textColor = isSelected ? city.PrimaryColor : Color.white } });

                // Era tag
                GUI.Label(new Rect(panelW - 180, cy + 3, 130, 16), city.Era,
                    new GUIStyle(GUI.skin.label) { fontSize = 10, alignment = TextAnchor.MiddleRight,
                        normal = { textColor = city.SecondaryColor } });

                // Description (truncated)
                string desc = city.Description.Length > 100 ? city.Description[..100] + "..." : city.Description;
                GUI.Label(new Rect(12, cy + 20, panelW - 60, 30), desc,
                    new GUIStyle(GUI.skin.label) { fontSize = 10, wordWrap = true,
                        normal = { textColor = new Color(0.7f, 0.7f, 0.7f) } });

                // Bonuses
                string bonusStr = $"✨ {GameConstants.RESOURCE_ICONS[city.BonusResource]} {city.BonusResource} x{city.BonusMultiplier:F1}  |  " +
                    $"{GameConstants.RESOURCE_ICONS[city.SecondaryBonus]} {city.SecondaryBonus} x{city.SecondaryMultiplier:F1}";
                GUI.Label(new Rect(12, cy + 50, 300, 16), bonusStr,
                    new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = new Color(1f, 0.85f, 0.3f) } });

                // Traits
                string traits = string.Join(" • ", city.UniqueTraits);
                GUI.Label(new Rect(320, cy + 50, panelW - 370, 16), traits,
                    new GUIStyle(GUI.skin.label) { fontSize = 9, alignment = TextAnchor.MiddleRight,
                        normal = { textColor = new Color(0.5f, 0.8f, 0.5f) } });

                // Special building
                GUI.Label(new Rect(12, cy + 63, panelW - 60, 14),
                    $"🏛️ Unique: {city.SpecialBuildingName}",
                    new GUIStyle(GUI.skin.label) { fontSize = 9, normal = { textColor = new Color(0.9f, 0.6f, 1f) } });

                // Click to select
                if (GUI.Button(new Rect(0, cy, panelW - 40, 75), "", GUIStyle.none))
                {
                    _selectedCityId = city.CityId;
                    _kingdomName = city.CityId;
                }

                cy += 80;
            }

            GUI.EndScrollView();
            yy += 325;

            // ─── Selected city detail line ───
            var selected = LegendaryCityDatabase.GetCity(_selectedCityId);
            if (selected != null)
            {
                GUI.color = selected.PrimaryColor;
                GUI.Label(new Rect(px + 20, yy, panelW - 40, 18),
                    $"Selected: {selected.Name} — \"{selected.Founded}\"",
                    new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Italic,
                        normal = { textColor = selected.PrimaryColor } });
                GUI.color = Color.white;
            }
            yy += 25;

            // ─── Action Buttons ───
            if (GUI.Button(new Rect(px + 20, yy, 220, 45), "🕉️ Begin Journey", _buttonStyle))
            {
                if (!string.IsNullOrEmpty(_playerName) && !string.IsNullOrEmpty(_kingdomName))
                {
                    GameManager.Instance.StartNewGame(_playerName, _kingdomName);
                }
            }

            if (GUI.Button(new Rect(px + panelW - 190, yy, 170, 45), "← Back", _buttonStyle))
                _showNewGame = false;
        }

        private void DrawLoadGamePanel(float centerX)
        {
            float panelW = 400;
            float panelH = 350;
            float px = centerX - panelW / 2;
            float py = Screen.height / 2 - panelH / 2;

            GUI.color = new Color(0.08f, 0.06f, 0.15f, 0.95f);
            GUI.DrawTexture(new Rect(px, py, panelW, panelH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(px + 10, py + 10, 380, 30), "📂 Load Game", _titleStyle);

            float yy = py + 70;

            // Auto save
            if (SaveLoadManager.Instance?.AutoSaveExists() == true)
            {
                if (GUI.Button(new Rect(px + 20, yy, 360, 40), "⏩ Continue from Auto-Save", _buttonStyle))
                {
                    GameManager.Instance.LoadAutoSave();
                }
                yy += 50;
            }

            // Save slots
            for (int i = 0; i < 5; i++)
            {
                var info = SaveLoadManager.Instance?.GetSaveSlotInfo(i);
                if (info?.Exists == true)
                {
                    string label = $"Slot {i + 1}: {info?.PlayerName} of {info?.KingdomName} (Lv.{info?.PlayerLevel})";
                    if (GUI.Button(new Rect(px + 20, yy, 360, 35), label, _buttonStyle))
                    {
                        GameManager.Instance.LoadGame(i);
                    }
                }
                else
                {
                    GUI.enabled = false;
                    GUI.Button(new Rect(px + 20, yy, 360, 35), $"Slot {i + 1}: Empty", _buttonStyle);
                    GUI.enabled = true;
                }
                yy += 40;
            }

            yy += 10;
            if (GUI.Button(new Rect(px + 120, yy, 160, 40), "← Back", _buttonStyle))
                _showLoadGame = false;
        }

        private void DrawSettingsPanel(float centerX)
        {
            float panelW = 350;
            float panelH = 200;
            float px = centerX - panelW / 2;
            float py = Screen.height / 2 - panelH / 2;

            GUI.color = new Color(0.08f, 0.06f, 0.15f, 0.95f);
            GUI.DrawTexture(new Rect(px, py, panelW, panelH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(px + 10, py + 10, 330, 30), "⚙️ Settings", _titleStyle);

            float yy = py + 70;

            GUI.Label(new Rect(px + 20, yy, 100, 20), "Music:", _labelStyle);
            float music = GUI.HorizontalSlider(new Rect(px + 130, yy + 5, 180, 20), 0.7f, 0f, 1f);
            yy += 35;

            GUI.Label(new Rect(px + 20, yy, 100, 20), "SFX:", _labelStyle);
            float sfx = GUI.HorizontalSlider(new Rect(px + 130, yy + 5, 180, 20), 1f, 0f, 1f);
            yy += 35;

            GUI.Label(new Rect(px + 20, yy, 100, 20), "Quality:", _labelStyle);
            string[] qualities = { "Low", "Medium", "High" };
            for (int i = 0; i < 3; i++)
            {
                if (GUI.Button(new Rect(px + 130 + i * 65, yy, 60, 25), qualities[i]))
                    QualitySettings.SetQualityLevel(i);
            }
            yy += 40;

            if (GUI.Button(new Rect(px + 95, yy, 160, 40), "← Back", _buttonStyle))
                _showSettings = false;
        }

        private void DrawDecorativeBorder()
        {
            // Top and bottom gold lines
            GUI.color = new Color(0.85f, 0.65f, 0.15f, 0.6f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, 3), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0, Screen.height - 3, Screen.width, 3), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0, 0, 3, Screen.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(Screen.width - 3, 0, 3, Screen.height), Texture2D.whiteTexture);

            // Inner border
            GUI.color = new Color(0.85f, 0.65f, 0.15f, 0.2f);
            GUI.DrawTexture(new Rect(10, 10, Screen.width - 20, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(10, Screen.height - 12, Screen.width - 20, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(10, 10, 2, Screen.height - 20), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(Screen.width - 12, 10, 2, Screen.height - 20), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
