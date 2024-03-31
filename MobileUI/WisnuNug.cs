using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Mobile;
using StardewValley.Tools;
using StardewValley;
using StardewValley.Menus;

namespace MobileUI;
internal class WisnuNug : IClickableMenu
    {
        public static bool DEBUG_CONSOLE_ENABLED = true;

        public new const int width = 300;

        public new const int height = 284;

        public ClickableTextureComponent buttonGameMenu;

        public ClickableTextureComponent buttonF8;

        public Game1 game1;

        private int paddingX = 12;

        private int paddingY = 12;

        private int spacing = 12;

        private bool drawingJustTheMenuButton;

        private bool _buttonGameMenuDown;

        private bool _buttonJournalDown;

        public Vector2 position;

        private Rectangle sourceRect;

        public MoneyDial moneyDial = new MoneyDial(8);

        public int timeShakeTimer;

        public int moneyShakeTimer;

        public int questPulseTimer;

        public int whenToPulseTimer;

        public ClickableTextureComponent questButton;

        public ClickableTextureComponent zoomOutButton;

        public ClickableTextureComponent zoomInButton;

        private StringBuilder _hoverText = new StringBuilder();

        private StringBuilder _timeText = new StringBuilder();

        private StringBuilder _dateText = new StringBuilder();

        private StringBuilder _hours = new StringBuilder();

        private StringBuilder _padZeros = new StringBuilder();

        private StringBuilder _temp = new StringBuilder();

        private int _lastDayOfMonth = -1;

        private string _lastDayOfMonthString;

        private string _amString;

        private string _pmString;

        private LocalizedContentManager.LanguageCode _languageCode = (LocalizedContentManager.LanguageCode)(-1);

        public bool questsDirty;

        public int questPingTimer;

        private Vector2 _datePosition = new Vector2(20f, 15f);

        private Vector2 _timePosition = new Vector2(196f, 15f);

        private int scaledViewportWidth => (int)((float)Game1.uiViewport.Width / Game1.DateTimeScale);

        private bool ShowingTutorial
        {
            get
            {
                if (TutorialManager.Instance.currentTutorial != null && (TutorialManager.Instance.currentTutorial.tType == tutorialType.TAP_JOURNAL || TutorialManager.Instance.currentTutorial.tType == tutorialType.TAP_GAME_MENU))
                {
                    return false;
                }

                return TutorialManager.Instance.ShowingDialogueBox;
            }
        }

        public bool buttonGameMenuVisible
        {
            get
            {
                if (Game1.displayHUD || (Game1.activeClickableMenu == null && Game1.CurrentEvent != null && Game1.pauseTime <= 0f && Game1.CurrentEvent.playerControlSequence && Game1.CurrentEvent.playerControlSequenceID != null && (Game1.CurrentEvent.playerControlSequenceID == "fair" || Game1.CurrentEvent.playerControlSequenceID == "iceFestival" || Game1.CurrentEvent.playerControlSequenceID == "eggFestival" || Game1.CurrentEvent.playerControlSequenceID == "halloween" || Game1.CurrentEvent.playerControlSequenceID == "eggHunt" || Game1.CurrentEvent.playerControlSequenceID == "flowerFestival" || Game1.CurrentEvent.playerControlSequenceID == "luau" || Game1.CurrentEvent.playerControlSequenceID == "jellies" || Game1.CurrentEvent.playerControlSequenceID == "christmas" || (Game1.CurrentEvent.eventCommands != null && Game1.CurrentEvent.eventCommands.Length > Game1.CurrentEvent.CurrentCommand && Game1.CurrentEvent.eventCommands[Game1.CurrentEvent.CurrentCommand].Equals("playerControl christmas2")))))
                {
                    return true;
                }

                return false;
            }
        }

        public WisnuNug(DayTimeMoneyBox timeBox)
        : base(Game1.uiViewport.Width - 300 + 32, 8, 300, 284)
    {
            position = new Vector2(base.xPositionOnScreen, base.yPositionOnScreen);
            sourceRect = new Rectangle(333, 431, 71, 43);
            buttonGameMenu = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 210, base.yPositionOnScreen + 300, 44, 46), Game1.mouseCursors, new Rectangle(366, 373, 16, 16), 4f);
            questButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 220, base.yPositionOnScreen + 240, 44, 46), Game1.mouseCursors, new Rectangle(383, 493, 11, 14), 4f);
            zoomOutButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 92, base.yPositionOnScreen + 244, 28, 32), Game1.mouseCursors, new Rectangle(177, 345, 7, 8), 4f);
            zoomInButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 124, base.yPositionOnScreen + 244, 28, 32), Game1.mouseCursors, new Rectangle(184, 345, 7, 8), 4f);
    }

    public override bool isWithinBounds(int x, int y)
        {
        x = (int)((float)x / Game1.DateTimeScale);
        y = (int)((float)y / Game1.DateTimeScale);
        if (Game1.options.zoomButtons && ((zoomInButton != null && zoomInButton.containsPoint(x, y)) || (zoomOutButton != null && zoomOutButton.containsPoint(x, y))))
            {
                return true;
            }

            if (Game1.player.visibleQuestCount > 0 && questButton.containsPoint(x, y))
            {
                return true;
            }

            return false;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
        if (Game1.player.visibleQuestCount > 0 && questButton.containsPoint(x, y) && Game1.player.CanMove && !Game1.dialogueUp && !Game1.eventUp && Game1.farmEvent == null)
                  {
                         Game1.activeClickableMenu = new QuestLog();
                  }
        if (!Game1.virtualJoypad.joystickHeld && (Game1.currentLocation == null || !Game1.currentLocation.tapToMove.TapHoldActive) && !(Game1.activeClickableMenu is MuseumMenu) && !(Game1.currentLocation is MermaidHouse))
        {
            if (!drawingJustTheMenuButton)
            {
                updatePosition();
            }

            x = (int)((float)x / Game1.DateTimeScale);
            y = (int)((float)y / Game1.DateTimeScale);
            if (buttonGameMenu.containsPoint(x, y) && ((buttonGameMenuVisible && !Game1.player.isEating && Game1.player.CanMove && (!Game1.player.usingTool || !(Game1.player.CurrentTool is FishingRod))) || drawingJustTheMenuButton) && !PinchZoom.Instance.Pinching && !ShowingTutorial)
            {
                OnTapGameMenuButton();
            }

            if (Game1.player.visibleQuestCount > 0 && questButton.containsPoint(x, y) && !Game1.player.isEating && Game1.player.CanMove && (!Game1.player.usingTool || !(Game1.player.CurrentTool is FishingRod)) && !Game1.eventUp && !PinchZoom.Instance.Pinching && !ShowingTutorial)
            {
                OnTapJournalButton();
            }
        }
        if (Game1.options.showToggleJoypadButton)
        {
            if (this.zoomInButton.containsPoint(x, y) && Game1.options.desiredBaseZoomLevel < 2f)
            {
                OnTapZoomInButton();
            }
            else if (this.zoomOutButton.containsPoint(x, y) && Game1.options.desiredBaseZoomLevel > 0.75f)
            {
                OnTapZoomOutButton();
            }
        }
    }

    public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            updatePosition();

    }

    public void questIconPulse()
        {
            questPulseTimer = 2000;
        }

        public override void performHoverAction(int x, int y)
        {
            updatePosition();
        if (Game1.player.visibleQuestCount > 0 && questButton.containsPoint(x, y))
        {
            _hoverText.Clear();
            if (Game1.options.gamepadControls)
            {
                _hoverText.Append(Game1.content.LoadString("Strings\\UI:QuestButton_Hover_Console"));
            }
            else
            {
                _hoverText.Append(Game1.content.LoadString("Strings\\UI:QuestButton_Hover", Game1.options.journalButton[0].ToString()));
            }
        }
        if (Game1.player.CanMove && buttonGameMenu.containsPoint(x, y))
        {
            _hoverText.Clear();
            if (Game1.options.gamepadControls)
            {
                _hoverText.Append(Game1.content.LoadString("Strings\\UI:GameMenu_Inventory"));
            }
            else
            {
                _hoverText.Append(Game1.content.LoadString("Strings\\UIGameMenu_Inventory"));
            }
        }
    }

        public void drawJustTheGameMenuButton(SpriteBatch b)
        {
            drawingJustTheMenuButton = false;
          if (buttonGameMenuVisible && Game1.ShowJustTheMinimalButtons)
            {
            Utility.makeSafe(ref position, 300, 284);
            base.xPositionOnScreen = (int)position.X;
            base.yPositionOnScreen = (int)position.Y;
            SetPositionTopRight();
            buttonGameMenu.bounds = new Rectangle(base.xPositionOnScreen + 210, base.yPositionOnScreen + 300, 44, 46);
            buttonGameMenu.draw(b, Color.White, 1E-07f);
            Game1.virtualJoypad.drawJustToggleShowJoypadButton(b);
            drawingJustTheMenuButton = true;
            }
        }

    public void drawMoneyBox(SpriteBatch b, int overrideX = -1, int overrideY = -1)
    {
        updatePosition();
        b.Draw(Game1.mouseCursors, ((overrideY != -1) ? new Vector2((overrideX == -1) ? position.X : ((float)overrideX), overrideY - 172) : position) + new Vector2(28 + ((moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0), 172 + ((moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)), new Rectangle(340, 472, 65, 17), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
        moneyDial.draw(b, ((overrideY != -1) ? new Vector2((overrideX == -1) ? position.X : ((float)overrideX), overrideY - 172) : position) + new Vector2(68 + ((moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0), 196 + ((moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)), Game1.player.Money);
        if (moneyShakeTimer > 0)
        {
            moneyShakeTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
        }
    }


    public override void update(GameTime time)
        {
            base.update(time);
            if (_languageCode != LocalizedContentManager.CurrentLanguageCode)
            {
                _languageCode = LocalizedContentManager.CurrentLanguageCode;
                _amString = Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370");
                _pmString = Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371");
            }

            if (questPingTimer > 0)
            {
                questPingTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
            }

            if (questPingTimer < 0)
            {
                questPingTimer = 0;
            }

            if (questsDirty)
            {
                if (Game1.player.hasPendingCompletedQuests)
                {
                    PingQuestLog();
                }

                questsDirty = false;
            }
        }

        public virtual void PingQuestLog()
        {
            questPingTimer = 6000;
        }

        public virtual void DismissQuestPing()
        {
            questPingTimer = 0;
        }

        public override void draw(SpriteBatch b)
        {
            drawingJustTheMenuButton = false;
            updatePosition();
            SpriteFont font = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? Game1.smallFont : Game1.dialogueFont);
            updatePosition();
        if (timeShakeTimer > 0)
            {
                timeShakeTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
            }

            if (questPulseTimer > 0)
            {
                questPulseTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
            }

            if (whenToPulseTimer >= 0)
            {
                whenToPulseTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
                if (whenToPulseTimer <= 0)
                {
                    whenToPulseTimer = 3000;
                    if (Game1.player.hasNewQuestActivity())
                    {
                        questPulseTimer = 1000;
                    }
                }
            }

            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(Game1.DateTimeScale));
            b.Draw(Game1.mouseCursors, position, sourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
        if (Game1.dayOfMonth != _lastDayOfMonth)
        {
            _lastDayOfMonth = Game1.dayOfMonth;
            _lastDayOfMonthString = Game1.shortDayDisplayNameFromDayOfSeason(_lastDayOfMonth);
        }
        _dateText.Clear();
        if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja)
        {
            _dateText.AppendEx(Game1.dayOfMonth);
            _dateText.Append("日 (");
            _dateText.Append(_lastDayOfMonthString);
            _dateText.Append(")");
        }
        else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh)
        {
            _dateText.Append(_lastDayOfMonthString);
            _dateText.Append(" ");
            _dateText.AppendEx(Game1.dayOfMonth);
            _dateText.Append("日");
        }
        else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod)
        {
            _dateText.Append(LocalizedContentManager.CurrentModLanguage.ClockDateFormat.Replace("[DAY_OF_WEEK]", _lastDayOfMonthString).Replace("[DAY_OF_MONTH]", Game1.dayOfMonth.ToString()));
        }
        else
        {
            _dateText.Append(_lastDayOfMonthString);
            _dateText.Append(". ");
            _dateText.AppendEx(Game1.dayOfMonth);
        }
        Vector2 daySize = font.MeasureString(_dateText);
        Vector2 dayPosition = new Vector2((float)sourceRect.X * 0.55f - daySize.X / 2f, (float)sourceRect.Y * (LocalizedContentManager.CurrentLanguageLatin ? 0.1f : 0.1f) - daySize.Y / 2f);
        Utility.drawTextWithShadow(b, _dateText, font, position + dayPosition, Game1.textColor);
		b.Draw(Game1.mouseCursors, position + new Vector2(212f, 68f), new Rectangle(406, 441 + Utility.getSeasonNumber(Game1.currentSeason) * 8, 12, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
		b.Draw(Game1.mouseCursors, position + new Vector2(116f, 68f), new Rectangle(317 + 12 * Game1.weatherIcon, 421, 12, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
        _padZeros.Clear();
        if (Game1.timeOfDay % 100 == 0)
        {
            _padZeros.Append("0");
        }
        _hours.Clear();
        switch (LocalizedContentManager.CurrentLanguageCode)
        {
            case LocalizedContentManager.LanguageCode.zh:
                if (Game1.timeOfDay / 100 % 24 == 0)
                {
                    _hours.Append("00");
                }
                else if (Game1.timeOfDay / 100 % 12 == 0)
                {
                    _hours.Append("12");
                }
                else
                {
                    _hours.AppendEx(Game1.timeOfDay / 100 % 12);
                }
                break;
            case LocalizedContentManager.LanguageCode.ru:
            case LocalizedContentManager.LanguageCode.pt:
            case LocalizedContentManager.LanguageCode.es:
            case LocalizedContentManager.LanguageCode.de:
            case LocalizedContentManager.LanguageCode.th:
            case LocalizedContentManager.LanguageCode.fr:
            case LocalizedContentManager.LanguageCode.tr:
            case LocalizedContentManager.LanguageCode.hu:
                _temp.Clear();
                _temp.AppendEx(Game1.timeOfDay / 100 % 24);
                if (Game1.timeOfDay / 100 % 24 <= 9)
                {
                    _hours.Append("0");
                }
                _hours.AppendEx(_temp);
                break;
            default:
                if (Game1.timeOfDay / 100 % 12 == 0)
                {
                    if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja)
                    {
                        _hours.Append("0");
                    }
                    else
                    {
                        _hours.Append("12");
                    }
                }
                else
                {
                    _hours.AppendEx(Game1.timeOfDay / 100 % 12);
                }
                break;
        }
        _timeText.Clear();
        _timeText.AppendEx(_hours);
        _timeText.Append(":");
        _timeText.AppendEx(Game1.timeOfDay % 100);
        _timeText.AppendEx(_padZeros);
        if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.it)
        {
            _timeText.Append(" ");
            if (Game1.timeOfDay < 1200 || Game1.timeOfDay >= 2400)
            {
                _timeText.Append(_amString);
            }
            else
            {
                _timeText.Append(_pmString);
            }
        }
        else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko)
        {
            if (Game1.timeOfDay < 1200 || Game1.timeOfDay >= 2400)
            {
                _timeText.Append(_amString);
            }
            else
            {
                _timeText.Append(_pmString);
            }
        }
        else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja)
        {
            _temp.Clear();
            _temp.AppendEx(_timeText);
            _timeText.Clear();
            if (Game1.timeOfDay < 1200 || Game1.timeOfDay >= 2400)
            {
                _timeText.Append(_amString);
                _timeText.Append(" ");
                _timeText.AppendEx(_temp);
            }
            else
            {
                _timeText.Append(_pmString);
                _timeText.Append(" ");
                _timeText.AppendEx(_temp);
            }
        }
        else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh)
        {
            _temp.Clear();
            _temp.AppendEx(_timeText);
            _timeText.Clear();
            if (Game1.timeOfDay < 600 || Game1.timeOfDay >= 2400)
            {
                _timeText.Append("凌晨 ");
                _timeText.AppendEx(_temp);
            }
            else if (Game1.timeOfDay < 1200)
            {
                _timeText.Append(_amString);
                _timeText.Append(" ");
                _timeText.AppendEx(_temp);
            }
            else if (Game1.timeOfDay < 1300)
            {
                _timeText.Append("中午  ");
                _timeText.AppendEx(_temp);
            }
            else if (Game1.timeOfDay < 1900)
            {
                _timeText.Append(_pmString);
                _timeText.Append(" ");
                _timeText.AppendEx(_temp);
            }
            else
            {
                _timeText.Append("晚上  ");
                _timeText.AppendEx(_temp);
            }
        }
        else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod)
        {
            _timeText.Clear();
            _timeText.Append(LocalizedContentManager.FormatTimeString(Game1.timeOfDay, LocalizedContentManager.CurrentModLanguage.ClockTimeFormat));
        }
        Vector2 txtSize = font.MeasureString(_timeText);
        Vector2 timePosition = new Vector2((float)sourceRect.X * 0.55f - txtSize.X / 2f + (float)((timeShakeTimer > 0) ? Game1.random.Next(-2, 3) : 0), (float)sourceRect.Y * (LocalizedContentManager.CurrentLanguageLatin ? 0.31f : 0.31f) - txtSize.Y / 2f + (float)((timeShakeTimer > 0) ? Game1.random.Next(-2, 3) : 0));
        bool nofade = Game1.shouldTimePass() || Game1.fadeToBlack || Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 2000.0 > 1000.0;
        Utility.drawTextWithShadow(b, _timeText, font, position + timePosition, (Game1.timeOfDay >= 2400) ? Color.Red : (Game1.textColor * (nofade ? 1f : 0.5f)));
        b.End();
        b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(Game1.DateTimeScale));
        int adjustedTime = (int)((float)(Game1.timeOfDay - Game1.timeOfDay % 100) + (float)(Game1.timeOfDay % 100 / 10) * 16.66f);
        if (Game1.player.visibleQuestCount > 0)
        {
            questButton.draw(b, Color.White, 1E-07f);
            if (questPulseTimer > 0)
            {
                float scaleMult = 1f / (Math.Max(300f, Math.Abs(questPulseTimer % 1000 - 500)) / 500f);
                b.Draw(Game1.mouseCursors, new Vector2(questButton.bounds.X + 24, questButton.bounds.Y + 32) + ((scaleMult > 1f) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(2f, 4f), 4f * scaleMult, SpriteEffects.None, 0.99f);
            }
            if (questPingTimer > 0)
            {
                b.Draw(Game1.mouseCursors, new Vector2(Game1.dayTimeMoneyBox.questButton.bounds.Left - 16, Game1.dayTimeMoneyBox.questButton.bounds.Bottom + 8), new Rectangle(128 + ((questPingTimer / 200 % 2 != 0) ? 16 : 0), 208, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
            }
        }
        buttonGameMenu.draw(b, Color.White, 1f);
        drawMoneyBox(b);
        if (_hoverText.Length > 0 && isWithinBounds(Game1.getOldMouseX(), Game1.getOldMouseY()))
        {
            IClickableMenu.drawHoverText(b, _hoverText, Game1.dialogueFont);
        }
        b.Draw(Game1.mouseCursors, position + new Vector2(88f, 88f), new Rectangle(324, 477, 7, 19), Color.White, (float)(Math.PI + Math.Min(Math.PI, (double)(((float)adjustedTime + (float)Game1.gameTimeInterval / 7000f * 16.6f - 600f) / 2000f) * Math.PI)), new Vector2(3f, 17f), 4f, SpriteEffects.None, 0.9f);
        if (Game1.displayHUD)
        {
            Game1.virtualJoypad.drawAndUpdateToggleShowJoypadButton(b);
        }

        if (!ShowingTutorial)
            {
                buttonGameMenu.draw(b, Color.White, 1E-07f);
            }

            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        if (Game1.options.showToggleJoypadButton)
        {
            this.zoomInButton.draw(b, Color.White * ((Game1.options.desiredBaseZoomLevel >= 2f) ? 0.5f : 1f), 1f);
            this.zoomOutButton.draw(b, Color.White * ((Game1.options.desiredBaseZoomLevel <= 0.75f) ? 0.5f : 1f), 1f);
        }

    }




    private void updatePosition()
    {
        bool flag = false;
        Point center = Game1.player.GetBoundingBox().Center;
        Vector2 vector = Game1.GlobalToLocal(globalPosition: new Vector2(center.X, center.Y), viewport: Game1.viewport);
        if (Game1.options.pinToolbarToggle)
        {
            flag = false;
        }
        else
        {
            flag = vector.Y > (float)(Game1.uiViewport.Height / 2 + 64);
        }

        base.xPositionOnScreen = (int)position.X;
        base.yPositionOnScreen = (int)position.Y;
        paddingX = Math.Max(12, Game1.xEdge);
        SetPositionTopRight();
        questButton.bounds = new Rectangle(base.xPositionOnScreen + 220, base.yPositionOnScreen + 240, 44, 46);
        zoomOutButton.bounds = new Rectangle(base.xPositionOnScreen + 92, base.yPositionOnScreen + 244, 28, 32);
        zoomInButton.bounds = new Rectangle(base.xPositionOnScreen + 124, base.yPositionOnScreen + 244, 28, 32);
        if (drawingJustTheMenuButton)
        {
            buttonGameMenu.bounds = new Rectangle(base.xPositionOnScreen + 210, base.yPositionOnScreen + 300, 44, 46);
        }
        else
        {
            buttonGameMenu.bounds.X = scaledViewportWidth - 64 - paddingX;
            if (Game1.player.visibleQuestCount > 0)
            {
                buttonGameMenu.bounds.Y = questButton.bounds.Y + questButton.bounds.Height + spacing;
            }
            else
            {
                buttonGameMenu.bounds.Y = questButton.bounds.Y;
            }
        }
        xPositionOnScreen = buttonGameMenu.bounds.X;
        yPositionOnScreen = 0;
        if (_buttonGameMenuDown)
        {
            buttonGameMenu.bounds.X -= 4;
            buttonGameMenu.bounds.Y += 4;
        }

        if (_buttonJournalDown)
        {
            questButton.bounds.X -= 4;
            questButton.bounds.Y += 4;
        }
    }

    public override void leftClickHeld(int x, int y)
        {
        x = (int)((float)x / Game1.DateTimeScale);
        y = (int)((float)y / Game1.DateTimeScale);
        if (_buttonGameMenuDown && !buttonGameMenu.containsPoint(x, y))
            {
                _buttonGameMenuDown = false;
                Toolbar.toolbarPressed = false;
            }

            if (_buttonJournalDown && !questButton.containsPoint(x, y))
            {
                _buttonJournalDown = false;
                Toolbar.toolbarPressed = false;
            }
        }

    private void SetPositionTopRight()
    {
        position = new Vector2((int)((float)scaledViewportWidth - (float)sourceRect.Width * 4f - (float)paddingX), paddingY);
    }

    public bool testToOpenDebugConsole(int x, int y)
    {
        return false;
    }

    public override void releaseLeftClick(int x, int y)
        {
            _buttonGameMenuDown = false;
            _buttonJournalDown = false;
        }

    private void OnTapGameMenuButton()
        {
            _buttonGameMenuDown = true;
            Game1.currentLocation.tapToMove.Reset();
            Toolbar.toolbarPressed = true;
            Game1.activeClickableMenu = new GameMenu(0);
        }

    private void OnTapJournalButton()
        {
            _buttonJournalDown = true;
            Game1.currentLocation.tapToMove.Reset();
            Toolbar.toolbarPressed = true;
            Game1.activeClickableMenu = new QuestLog();
        }
    private void OnTapZoomInButton()
    {

    }
    private void OnTapZoomOutButton()
    {

    }
}