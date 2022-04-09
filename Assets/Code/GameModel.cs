using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

namespace Avangardum.ComplectEnergoTestTask
{
    public class GameModel : IGameModel
    {
        private struct Collectible
        {
            public int Id;
            public Vector2 Position;
        }
        
        private const float StartingVerticalSpeed = 4;
        private const float StartingHorizontalSpeed = 4;
        private const float StartingMaxHorizontalSpeed = 4;
        private const float StartingMaxHorizontalAcceleration = 4;
        private const float MinPlayerXPosition = -5;
        private const float MaxPlayerXPosition = 5;
        private const int StartingLives = 3;
        private const float ChunkSize = 50;
        private const float VerticalDistanceToGenerateChunk = 20;
        private const float VerticalDistanceBetweenCollectibles = 5;
        private const float GoodCollectibleChance = 0.5f;
        private const float CollectibleGatheringDistance = 0.9f;
        private const float CollectibleCleanupDistance = 20;
        
        /// <summary>
        /// Key - amount of score for a multiplier to be enabled, value - speed multiplier
        /// </summary>
        private readonly Dictionary<int, float> _speedMultipliers = new Dictionary<int, float>
        {
            [0] = 1,
            [10] = 1.5f,
            [25] = 2,
            [50] = 3,
            [100] = 4
        };

        private bool _isGameInProgress;
        private Vector2 _playerPosition;
        private float _verticalSpeed;
        private float _horizontalSpeed;
        private bool _isDesiredDirectionRight;
        private int _score;
        private int _lives;
        private List<Collectible> _goodCollectibles = new List<Collectible>();
        private List<Collectible> _badCollectibles = new List<Collectible>();
        private float _lastCollectibleY;
        private float _lastChunkEndY;
        private int _lastCollectibleId;
        private float _maxHorizontalSpeed;
        private float _maxHorizontalAcceleration;

        public GameModel(UnityEventObserver unityEventObserver)
        {
            unityEventObserver.FixedUpdateTriggered += FixedUpdate;
        }

        private Vector2 PlayerPosition
        {
            get => _playerPosition;
            set
            {
                _playerPosition = value;
                PlayerPositionChanged?.Invoke(this, value);
            }
        }

        private int Score
        {
            get => _score;
            set
            {
                _score = value;
                ScoreChanged?.Invoke(this, value);
                RecalculateSpeedMultiplier();
            }
        }

        private int Lives
        {
            get => _lives;
            set
            {
                _lives = value;
                LivesChanged?.Invoke(this, value);
                if (_lives <= 0)
                {
                    _isGameInProgress = false;
                    GameOver?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void FixedUpdate(object sender, UpdateArgs args)
        {
            if (!_isGameInProgress)
            {
                return;
            }
            
            // Change horizontal speed
            var desiredSpeed = _isDesiredDirectionRight ? _maxHorizontalSpeed : -_maxHorizontalSpeed;
            if (_horizontalSpeed < desiredSpeed)
            {
                _horizontalSpeed += _maxHorizontalAcceleration * args.DeltaTime;
                if (_horizontalSpeed > desiredSpeed)
                    _horizontalSpeed = desiredSpeed;
            }
            else if (_horizontalSpeed > desiredSpeed)
            {
                _horizontalSpeed -= _maxHorizontalAcceleration * args.DeltaTime;
                if (_horizontalSpeed < desiredSpeed)
                    _horizontalSpeed = desiredSpeed;
            }
            
            // Change player position
            PlayerPosition += new Vector2(_horizontalSpeed, _verticalSpeed) * args.DeltaTime;
            
            // Clamp x position
            var position = PlayerPosition;
            var oldX = position.x;
            position.x = Mathf.Clamp(position.x, MinPlayerXPosition, MaxPlayerXPosition);
            PlayerPosition = position;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            bool collidedWithWall = PlayerPosition.x != oldX;
            if (collidedWithWall)
            {
                _horizontalSpeed = 0;
            }
            
            // Generate a new chunk if needed
            if (_lastChunkEndY - _playerPosition.y <= VerticalDistanceToGenerateChunk)
                GenerateChunk();

            CleanupCollectibles();
            CheckCollisionsWithCollectibles();
        }

        private void CheckCollisionsWithCollectibles()
        {
            foreach (var collectible in _goodCollectibles
                .Where(c => Vector2.Distance(c.Position, _playerPosition) <= CollectibleGatheringDistance)
                .ToList())
            {
                RemoveGoodCollectible(collectible.Id);
                Score++;
            }
            
            foreach (var collectible in _badCollectibles
                .Where(c => Vector2.Distance(c.Position, _playerPosition) <= CollectibleGatheringDistance)
                .ToList())
            {
                RemoveBadCollectible(collectible.Id);
                Lives--;
            }
        }

        private void CleanupCollectibles()
        {
            // Delete collectibles left far below the player
            foreach (var collectible in _goodCollectibles
                .Where(collectible => _playerPosition.y - collectible.Position.y >= CollectibleCleanupDistance).ToList())
            {
                RemoveGoodCollectible(collectible.Id);
            }
            foreach (var collectible in _badCollectibles
                .Where(collectible => _playerPosition.y - collectible.Position.y >= CollectibleCleanupDistance).ToList())
            {
                RemoveBadCollectible(collectible.Id);
            }
        }

        private void RemoveGoodCollectible(int id)
        {
            int itemsRemoved = _goodCollectibles.RemoveAll(x => x.Id == id);
            Assert.AreEqual(itemsRemoved, 1);
            GoodCollectibleRemoved?.Invoke(this, new CollectibleRemovedArgs {Id = id});
        }
        
        private void RemoveBadCollectible(int id)
        {
            int itemsRemoved = _badCollectibles.RemoveAll(x => x.Id == id);
            Assert.AreEqual(itemsRemoved, 1);
            BadCollectibleRemoved?.Invoke(this, new CollectibleRemovedArgs {Id = id});
        }

        private void GenerateChunk()
        {
            _lastChunkEndY += ChunkSize;
            while (_lastCollectibleY < _lastChunkEndY)
            {
                // Generate a new collectible
                _lastCollectibleY += VerticalDistanceBetweenCollectibles;
                var collectibleX = Random.Range(MinPlayerXPosition, MaxPlayerXPosition);
                var collectiblePosition = new Vector2(collectibleX, _lastCollectibleY);
                var collectibleId = ++_lastCollectibleId;
                var isCollectibleGood = Random.Range(0f, 1f) < GoodCollectibleChance;
                var collectible = new Collectible {Position = collectiblePosition, Id = collectibleId};
                if (isCollectibleGood)
                {
                    _goodCollectibles.Add(collectible);
                    GoodCollectibleAdded?.Invoke(this,
                        new CollectibleAddedArgs {Position = collectiblePosition, Id = collectibleId});
                }
                else
                {
                    _badCollectibles.Add(collectible);
                    BadCollectibleAdded?.Invoke(this,
                        new CollectibleAddedArgs {Position = collectiblePosition, Id = collectibleId});
                }
            }
        }

        public void ChangeDirection()
        {
            _isDesiredDirectionRight = !_isDesiredDirectionRight;
        }

        public void StartGame()
        {
            Score = 0;
            Lives = StartingLives;
            
            PlayerPosition = Vector2.zero;
            _verticalSpeed = StartingVerticalSpeed;
            _horizontalSpeed = StartingHorizontalSpeed;
            _isDesiredDirectionRight = true;
            RecalculateSpeedMultiplier();

            foreach (var collectible in _goodCollectibles.ToList())
            {
                RemoveGoodCollectible(collectible.Id);
            }
            foreach (var collectible in _badCollectibles.ToList())
            {
                RemoveBadCollectible(collectible.Id);
            }
            _lastCollectibleId = 0;
            _lastChunkEndY = 0;
            _lastCollectibleY = 0;
            GenerateChunk();
            
            _isGameInProgress = true;
            GameStarted?.Invoke(this, EventArgs.Empty);
        }

        private void RecalculateSpeedMultiplier()
        {
            var multiplier = _speedMultipliers[_speedMultipliers.Keys.Where(k => k <= Score).Max()];
            _verticalSpeed = StartingVerticalSpeed * multiplier;
            _maxHorizontalSpeed = StartingMaxHorizontalSpeed * multiplier;
            _maxHorizontalAcceleration = StartingMaxHorizontalAcceleration * multiplier;
        }

        public event EventHandler<CollectibleAddedArgs> BadCollectibleAdded;
        public event EventHandler<CollectibleRemovedArgs> BadCollectibleRemoved;
        public event EventHandler GameOver;
        public event EventHandler GameStarted;
        public event EventHandler<CollectibleAddedArgs> GoodCollectibleAdded;
        public event EventHandler<CollectibleRemovedArgs> GoodCollectibleRemoved;
        public event EventHandler<int> LivesChanged;
        public event EventHandler<Vector2> PlayerPositionChanged;
        public event EventHandler<int> ScoreChanged;
    }
}
