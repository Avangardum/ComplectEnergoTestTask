using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Avangardum.ComplectEnergoTestTask
{
    public class GameView : MonoBehaviour, IGameView
    {
        private const float BackgroundAndWallSectionHeight = 50;

        [SerializeField] private GameObject _menu;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _changeDirectionButton;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _livesText;
        [SerializeField] private GameObject _player;
        [SerializeField] private GameObject _camera;
        [SerializeField] private List<GameObject> _backgroundSections;
        [SerializeField] private GameObject _goodCollectiblePrefab;
        [SerializeField] private GameObject _badCollectiblePrefab;
        
        private Vector3 _cameraOffset;
        private float _lastPlacedBackgroundSectionYPos = 50;
        private Dictionary<int, GameObject> _goodCollectibles = new Dictionary<int, GameObject>();
        private Dictionary<int, GameObject> _badCollectibles = new Dictionary<int, GameObject>();
        private List<Vector2> _backgroundSectionsInitialPositions = new List<Vector2>();
        
        private void Awake()
        {
            _startButton.onClick.AddListener(() => GameStartClicked?.Invoke(this, EventArgs.Empty));
            _changeDirectionButton.onClick.AddListener(() => ChangeDirectionClicked?.Invoke(this, EventArgs.Empty));
            _cameraOffset = _camera.transform.position - _player.transform.position;
            _backgroundSectionsInitialPositions = _backgroundSections.Select(x => (Vector2) x.transform.position).ToList();
        }

        private void Update()
        {
            UpdateCameraPosition();
            UpdateBackgroundAndWalls();
        }

        private void UpdateCameraPosition()
        {
            var position = _player.transform.position + _cameraOffset;
            position.x = 0;
            _camera.transform.position = position;
        }

        private void UpdateBackgroundAndWalls()
        {
            if (_lastPlacedBackgroundSectionYPos < _camera.transform.position.y)
            {
                // Move the most lower section up
                var sectionToMove = _backgroundSections.OrderBy(x => x.transform.position.y).First();
                _lastPlacedBackgroundSectionYPos += BackgroundAndWallSectionHeight;
                var position = sectionToMove.transform.position;
                position.y = _lastPlacedBackgroundSectionYPos;
                sectionToMove.transform.position = position;
            }
        }

        public void AddBadCollectible(int id, Vector2 position)
        {
            var collectible = Instantiate(_badCollectiblePrefab, position, Quaternion.identity);
            _badCollectibles.Add(id, collectible);
        }

        public void AddGoodCollectible(int id, Vector2 position)
        {
            var collectible = Instantiate(_goodCollectiblePrefab, position, Quaternion.identity);
            _goodCollectibles.Add(id, collectible);
        }

        public void GoToGame()
        {
            _menu.SetActive(false);
            
            // Reset background sections positions
            Assert.AreEqual(_backgroundSections.Count, _backgroundSectionsInitialPositions.Count);
            for (int i = 0; i < _backgroundSections.Count; i++)
            {
                _backgroundSections[i].transform.position = _backgroundSectionsInitialPositions[i];
            }
            _lastPlacedBackgroundSectionYPos = _backgroundSections.Select(x => x.transform.position.y).Max();
        }

        public void GoToMenu()
        {
            _menu.SetActive(true);
        }

        public void RemoveGoodCollectible(int id)
        {
            Destroy(_goodCollectibles[id]);
            _goodCollectibles.Remove(id);
        }

        public void RemoveBadCollectible(int id)
        {
            Destroy(_badCollectibles[id]);
            _badCollectibles.Remove(id);
        }

        public void SetLives(int value)
        {
            _livesText.text = $"Lives: {value}";
        }

        public void SetPlayerPosition(Vector2 value)
        {
            _player.transform.position = value;
        }

        public void SetScore(int value)
        {
            _scoreText.text = $"Score: {value}";
        }

        public event EventHandler ChangeDirectionClicked;
        public event EventHandler GameStartClicked;
    }
}
