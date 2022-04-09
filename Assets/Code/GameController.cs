using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Avangardum.ComplectEnergoTestTask
{
    public class GameController
    {
        private IGameModel _gameModel;
        private IGameView _gameView;

        public GameController(IGameModel gameModel, IGameView gameView)
        {
            _gameModel = gameModel;
            _gameView = gameView;

            _gameModel.GameOver += OnGameOver;
            _gameModel.GameStarted += OnGameStarted;
            _gameModel.LivesChanged += OnLivesChanged;
            _gameModel.ScoreChanged += OnScoreChanged;
            _gameModel.BadCollectibleAdded += OnBadCollectibleAdded;
            _gameModel.BadCollectibleRemoved += OnBadCollectibleRemoved;
            _gameModel.GoodCollectibleAdded += OnGoodCollectibleAdded;
            _gameModel.GoodCollectibleRemoved += OnGoodCollectibleRemoved;
            _gameModel.PlayerPositionChanged += OnPlayerPositionChanged;

            _gameView.ChangeDirectionClicked += OnChangeDirectionClicked;
            _gameView.GameStartClicked += OnGameStartClicked;
        }

        #region GameModelEventHandlers

        private void OnPlayerPositionChanged(object sender, Vector2 value)
        {
            _gameView.SetPlayerPosition(value);
        }

        private void OnGoodCollectibleRemoved(object sender, CollectibleRemovedArgs e)
        {
            _gameView.RemoveGoodCollectible(e.Id);
        }

        private void OnGoodCollectibleAdded(object sender, CollectibleAddedArgs e)
        {
            _gameView.AddGoodCollectible(e.Id, e.Position);
        }

        private void OnBadCollectibleRemoved(object sender, CollectibleRemovedArgs e)
        {
            _gameView.RemoveBadCollectible(e.Id);
        }

        private void OnBadCollectibleAdded(object sender, CollectibleAddedArgs e)
        {
            _gameView.AddBadCollectible(e.Id, e.Position);
        }

        private void OnScoreChanged(object sender, int value)
        {
            _gameView.SetScore(value);
        }

        private void OnLivesChanged(object sender, int value)
        {
            _gameView.SetLives(value);
        }

        private void OnGameStarted(object sender, EventArgs e)
        {
            _gameView.GoToGame();
        }

        private void OnGameOver(object sender, EventArgs e)
        {
            _gameView.GoToMenu();
        }

        #endregion


        #region GameViewEventHandlers

        private void OnGameStartClicked(object sender, EventArgs e)
        {
            _gameModel.StartGame();
        }

        private void OnChangeDirectionClicked(object sender, EventArgs e)
        {
            _gameModel.ChangeDirection();
        }

        #endregion
        
        
    }
}
