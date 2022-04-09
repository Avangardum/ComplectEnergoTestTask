using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Avangardum.ComplectEnergoTestTask
{
    public interface IGameView
    {
        void AddBadCollectible(int id, Vector2 position);

        void AddGoodCollectible(int id, Vector2 position);

        void GoToGame();

        void GoToMenu();

        void RemoveGoodCollectible(int id);

        void RemoveBadCollectible(int id);

        void SetLives(int value);

        void SetPlayerPosition(Vector2 value);

        void SetScore(int value);

        event EventHandler ChangeDirectionClicked;

        event EventHandler GameStartClicked;
    }
}
