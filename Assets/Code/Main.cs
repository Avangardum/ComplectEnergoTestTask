using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Avangardum.ComplectEnergoTestTask
{
    public class Main : MonoBehaviour
    {
        [SerializeField] private GameView _gameView;
        [SerializeField] private UnityEventObserver _unityEventObserver;
        
        private void Awake()
        {
            var gameModel = new GameModel(_unityEventObserver);
            var gameController = new GameController(gameModel, _gameView);
        }
    }
}
