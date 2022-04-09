using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Avangardum.ComplectEnergoTestTask
{
    public interface IGameModel
    {
        void ChangeDirection();

        void StartGame();

        event EventHandler<CollectibleAddedArgs> BadCollectibleAdded;

        event EventHandler<CollectibleRemovedArgs> BadCollectibleRemoved;

        event EventHandler GameOver;

        event EventHandler GameStarted;

        event EventHandler<CollectibleAddedArgs> GoodCollectibleAdded;

        event EventHandler<CollectibleRemovedArgs> GoodCollectibleRemoved;

        event EventHandler<int> LivesChanged;

        event EventHandler<Vector2> PlayerPositionChanged;

        event EventHandler<int> ScoreChanged;
    }
}
