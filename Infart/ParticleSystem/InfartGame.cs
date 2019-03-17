using FbonizziMonoGame.Extensions;
using Infart.Assets;
using Infart.Astronaut;
using Infart.Background;
using Infart.Drawing;
using Infart.HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infart.ParticleSystem
{
    public class InfartGame
    {
        private Camera _playerCamera;
        private BackgroundManager _background;
        private GroundManager _ground;
        private GemmaManager _gemme;
        private SoundManager _soundManager;
        private StatusBar _statusBar;
        private readonly AssetsLoader _assetsLoader;
        private bool _isPaused = false;
        private InfartExplosion _deadExplosion;
        private bool _newHighScore = false;
        private bool _forceToFinish;
        private Rectangle _highScorePosition;
        private readonly Color _highScoreColor = Color.Blue;
        private readonly SpriteFont _font;

        public double BucoProbability { get; private set; }
        public double PowerUpProbability { get; private set; }
        public double PeperoncinoDuration { get; private set; }
        public double GemmaProbability { get; private set; }
        public double BroccoloDuration { get; private set; }
        public Vector2 LarghezzaBuchi { get => _larghezzaBuchi; private set => _larghezzaBuchi = value; }

        private const double DefaultBucoProbability = 0.005;
        private const double DefaultPoweupProbability = 0.03;
        private const double DefaultPeperoncinoDuration = 6000.0;
        private const double DefaultMerdoneDuration = 3500.0;
        private const double DefaultGemmaProbability = 0.4;
        private const float DefaultBucoMinW = 190.0f;
        private const float DefaultBucoMaxW = 250.0f;
        private int _oldScoreMetri = 0;

        public int ScoreMetri { get; set; } = 0;

        private readonly StringBuilder _scoreString;
        private readonly Vector2 _scoreStringPosition = new Vector2(680, 438);

        private Rectangle _barRectangle = new Rectangle(400, 430, 800, 50);
        private readonly Color _barColor = Color.LightCyan * 0.5f;

        private readonly string _metriString;

        private int _highScoreX;

        public bool FallSoundActive { get; set; }
        public bool MerdaModeActive { get; set; }
        public bool JalapenosModeActive { get; set; }

        private int _gameCameraHLimit;
        private Vector2 _larghezzaBuchi;

        public InfartGame(
            AssetsLoader assetsLoader,
            SoundManager soundManager,
            int highScore,
            string metriString,
            string pauseString)
        {
            ResolutionWidth = 800;
            ResolutionHeight = 480;
            PlayableGameWindowHeight = 1500;

            this._assetsLoader = assetsLoader;

            this._soundManager = soundManager;

            _statusBar = new StatusBar(new Vector2(460, 435), assetsLoader, soundManager);

            this.GetScore = highScore;
            SetRecordRectangle();
            SetHighScore(highScore);

            _font = assetsLoader.Font;
            //   px_texture_ = AssetsLoader.px_texture_;

            _playerCamera = new Camera(Vector2.Zero, new Vector2(800, 480), 0.8f);

            _deadExplosion = new InfartExplosion(assetsLoader);
            //     record_explosion_ = new RecordExplosion_episodio1(AssetsLoader);
#warning TODO: mettere un popup per il record

            _isPaused = false;
            _forceToFinish = false;

            InitializeBackgroundManager();
            InitializeGroundManager();
            InitializeGemmaManager();
            InitializePlayer();

            _scoreString = new StringBuilder();

            _highScoreColor = new Color(22, 232, 86) * 0.5f;
            //     px_texture_ = AssetsLoader.px_texture_;
            _metriString = metriString;

            NewGame();
        }

        private void NewGame()
        {
            _deadExplosion.Reset();

            _isPaused = false;
            _forceToFinish = false;
            _newHighScore = false;

            _gameCameraHLimit = -PlayableGameWindowHeight - _playerCamera.ViewPortHeight;

            FallSoundActive = false;
            MerdaModeActive = false;
            JalapenosModeActive = false;

            _oldScoreMetri = 0;
            ScoreMetri = 0;
            GetScoregge = 0;

            BucoProbability = DefaultBucoProbability;
            PowerUpProbability = DefaultPoweupProbability;
            PeperoncinoDuration = DefaultPeperoncinoDuration;
            BroccoloDuration = DefaultMerdoneDuration;
            GemmaProbability = DefaultGemmaProbability;
            LarghezzaBuchi = new Vector2(DefaultBucoMinW, DefaultBucoMaxW);

            _playerCamera.Reset(Vector2.Zero);
            PlayerReference.Reset(new Vector2(240, 300));

            _background.Reset(_playerCamera);
            _ground.Reset(_playerCamera);
            _gemme.Reset(_playerCamera);
            _statusBar.Reset();
        }

        private void InitializeBackgroundManager()
        {
            _background = new BackgroundManager(_playerCamera, (_assetsLoader as AssetsLoader), this);
        }

        private void InitializeGroundManager()
        {
            _ground = new GroundManager(_playerCamera, (_assetsLoader as AssetsLoader), this);
        }

        private void InitializeGemmaManager()
        {
            _gemme = new GemmaManager(_playerCamera, (_assetsLoader as AssetsLoader));
        }

        private void InitializePlayer()
        {
            PlayerReference = new Player(new Vector2(240, 300), (_assetsLoader as AssetsLoader), this);
        }

        public int GetScoregge { get; private set; }

        public int GetCurrentScore
        {
            get { return ScoreMetri; }
        }

        public void HandleInput()
        {
            if (!PlayerReference.Dead)
            {
                PlayerReference.HandleInput();
            }

            if (FallSoundActive && !_deadExplosion.Started)
            {
                _forceToFinish = true;

                if ((_soundManager as SoundManager).HasFallFinished())
                {
                    _deadExplosion.Explode(PlayerReference.Position, false, (_soundManager as SoundManager));
                    _statusBar.SetInfart();
                }
            }

            if (_deadExplosion.Started)
            {
                _forceToFinish = true;
            }
        }

        public int GetHamburger
        {
            get { return _statusBar.HamburgerMangiatiInTotale; }
        }

        public List<GameObject> GroundObjects()
        {
            return _ground.WalkableObjects();
        }

        private void SetRecordRectangle()
        {
            _highScorePosition = new Rectangle(0, -1020, 20, 1500);
        }

        public int GetScore { get; private set; }

        public bool IsPaused
        {
            get { return _isPaused; }
            set
            {
                if (value)
                    Pause();
                else ResumeFromPause();
            }
        }

        public void Pause()
        {
            _isPaused = true;
            _soundManager.PauseAll();
            GC.Collect();
        }

        public void ResumeFromPause()
        {
            _isPaused = false;
            _soundManager.ResumeAll();
        }

        public int ResolutionWidth { get; }

        public int ResolutionHeight { get; }

        public int PlayableGameWindowHeight { get; }

        public void StopScoreggia()
        {
            _soundManager.StopFart();
        }

        public void DecreaseParallaxSpeed()
        {
            _background.DecreaseParallaxSpeed();
        }

        public void IncreaseParallaxSpeed()
        {
            _background.IncreaseParallaxSpeed();
        }

        public void PlayerJumped()
        {
            _statusBar.PlayerJumped();
        }

        public void AddScoreggia()
        {
            ++GetScoregge;
            _soundManager.PlayFart();
        }

        public void SetHighScore(int value)
        {
            GetScore = value;
            _highScorePosition.X = value * 100;
            _highScoreX = value * 100;
        }

        public void AddGemma(Vector2 position)
        {
            if (position.X > PlayerReference.Position.X + ResolutionWidth / 2)
                _gemme.AddGemma(position);
        }

        public int HamburgerMangiati()
        {
            return _statusBar.HamburgerMangiatiInTotale;
        }

        public void AddPowerUp(Vector2 position)
        {
            if (position.X > PlayerReference.Position.X)
                (_gemme as GemmaManager).AddPowerUp(position);
        }

        public void PlayerCollidedWithNormalGemma()
        {
            _statusBar.HamburgerEaten();
            (_soundManager as SoundManager).PlayBite();
        }

        public void Update(TimeSpan elapsed)
        {
            var gametime = elapsed.TotalMilliseconds;
            if (!_isPaused)
            {
                RepositionCamera();

                CheckDead();
                _background.Update(gametime);
                _ground.Update(gametime);
                PlayerReference.Update(gametime);
                PlayerReference.CollidingObjectsReference = _ground.WalkableObjects();
                _gemme.Update(gametime);
                CheckPlayerGemmaCollision();
                //         record_explosion_.Update(gametime);

                if (_statusBar != null)
                    _statusBar.Update(gametime);

                if (_deadExplosion.Started)
                {
                    _deadExplosion.Update(gametime);
                }
            }

            if (ScoreMetri % 50 == 0)
            {
                if (_oldScoreMetri != ScoreMetri)
                {
                    (PlayerReference as Player).IncreaseMoveSpeed();
                    _background.IncreaseParallaxSpeed();
                    if (LarghezzaBuchi.Y < 600)
                    {
                        _larghezzaBuchi.X += 80.0f;
                        _larghezzaBuchi.Y += 80.0f;
                    }
                }
            }
            _oldScoreMetri = ScoreMetri;
        }

        public void CheckPlayerGemmaCollision()
        {
            if (_gemme.CheckCollisionWithPlayer(PlayerReference))
            {
                PlayerCollidedWithNormalGemma();
            }

            if ((_gemme as GemmaManager).CheckJalapenoCollisionWithPlayer(PlayerReference))
            {
                _statusBar.ComputeJalapenos();
                (PlayerReference as Player).ActivateJalapenos();
                (_soundManager as SoundManager).PlayJalapeno();
                JalapenosModeActive = true;
            }
            else if ((_gemme as GemmaManager).CheckMerdaCollisionWithPlayer(PlayerReference))
            {
                _statusBar.ComputeMerda();
                (PlayerReference as Player).ActivateBroccolo();
                (_soundManager as SoundManager).PlayShit();
                MerdaModeActive = true;
            }
        }

        public void Resume()
        {
            throw new NotImplementedException();
        }

        private void MakePlayerDead()
        {
            PlayerReference.Dead = true;

            if (GetScore < ScoreMetri)
            {
                SetHighScore(ScoreMetri);
                //       RecordExplosion.Explode(PlayerReference.Position, 90);
            }
        }

        private void CheckDead()
        {
            if (!FallSoundActive)
            {
                if (PlayerReference.Position.Y > _playerCamera.ViewPortHeight)
                {
                    MakePlayerDead();
                    (_soundManager as SoundManager).PlayFall();
                    FallSoundActive = true;
                }
            }

            if (_statusBar.IsInfarting() && !PlayerReference.Dead)
            {
                _deadExplosion.Explode(PlayerReference.Position, true, (_soundManager as SoundManager));
                MakePlayerDead();
            }
        }

        public bool IsTimeToGameOver()
        {
            if (_forceToFinish)
            {
                _soundManager.StopSounds();
                return true;
            }

            if (_deadExplosion.Started)
            {
                return _deadExplosion.Finished;
            }
            else if (FallSoundActive)
            {
                return false;
            }
            else
            {
                return PlayerReference.Dead;
            }
        }

        private void LerpCameraPosition(float newCameraX, float newCameraY)
        {
            _playerCamera.Position = new Vector2(
              MathHelper.Lerp(_playerCamera.Position.X, newCameraX, 0.08f),
              MathHelper.Lerp(_playerCamera.Position.Y, newCameraY, 0.08f));
        }

        public Player PlayerReference { get; private set; }

        private void RepositionCamera()
        {
            float playerCameraY;
            float playerCameraX;

            playerCameraX = PlayerReference.Position.X - 150;
            playerCameraY = PlayerReference.Position.Y - 200;

            if (playerCameraY < _gameCameraHLimit)
                playerCameraY = _gameCameraHLimit;
            else if (playerCameraY > 0)
                playerCameraY = 0;

            LerpCameraPosition(playerCameraX, playerCameraY);
        }

        private void DrawUi(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            //spriteBatch.Draw(
            //    px_texture_,
            //    bar_rectangle_,
            //    bar_color_);

            _scoreString.Clear();
            StringBuilderExtensions.AppendNumber(_scoreString, ScoreMetri).Append(_metriString);
            spriteBatch.DrawString(_font, _scoreString, _scoreStringPosition, Color.DarkBlue);
            _statusBar.Draw(spriteBatch);

            spriteBatch.End();
        }

        public void Draw(SpriteBatch spritebatch)
        {
            Matrix cameraTransformation = _playerCamera.GetTransformation();

            spritebatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, cameraTransformation);

            _background.Draw(spritebatch);
            _ground.Draw(spritebatch);
            _gemme.Draw(spritebatch);

            spritebatch.End();

            if (!_deadExplosion.Started)
            {
                spritebatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, cameraTransformation);
                PlayerReference.DrawParticles(spritebatch);
                spritebatch.End();
            }

            spritebatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, cameraTransformation);

            if (_deadExplosion.Started)
                _deadExplosion.Draw(spritebatch);
            else
            {
                PlayerReference.Draw(spritebatch);
            }

            _background.DrawSpecial(spritebatch);

            if (GetScore != 0
               && PlayerReference.Position.X >= _highScorePosition.X - ResolutionWidth
               && !_newHighScore)
            {
                if (PlayerReference.Position.X < _highScorePosition.X)
                {
                    //spritebatch.Draw(
                    //    px_texture_,
                    //    high_score_position_,
                    //    high_score_color_);
                }
                else
                {
                    //  RecordExplosion.Explode(PlayerReference.Position, 0);
                    _newHighScore = true;
                }
            }

            //    record_explosion_.Draw(spritebatch);

            spritebatch.End();

            DrawUi(spritebatch);
        }

        public void DrawForMenu(SpriteBatch spritebatch)
        {
            Matrix cameraTransformation = _playerCamera.GetTransformation();

            spritebatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, cameraTransformation);
            _background.Draw(spritebatch);
            _ground.Draw(spritebatch);
            spritebatch.End();

            spritebatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, cameraTransformation);
            if (!_deadExplosion.Started)
                PlayerReference.DrawParticles(spritebatch);
            spritebatch.End();

            spritebatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, cameraTransformation);
            if (!_deadExplosion.Started)
                PlayerReference.Draw(spritebatch);
            _background.DrawSpecial(spritebatch);
            spritebatch.End();
        }
    }
}