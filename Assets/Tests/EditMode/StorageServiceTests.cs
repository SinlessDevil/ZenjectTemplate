using System.Collections.Generic;
using Code.Services.PersistenceProgress;
using Code.Services.PersistenceProgress.Player;
using Code.Services.SaveLoad;
using Code.Services.Storage;
using NSubstitute;
using NUnit.Framework;

namespace Tests.EditMode
{
    public class StorageServiceTests
    {
        private StorageService _storageService;
        private IPersistenceProgressService _progress;
        private ISaveLoadFacade _saveLoad;
        private PlayerData _playerData;
        private Currency _coins;

        [SetUp]
        public void SetUp()
        {
            _progress = Substitute.For<IPersistenceProgressService>();
            _saveLoad = Substitute.For<ISaveLoadFacade>();

            
            var newCurrency = new Currency(CurrencyType.Gold);
            newCurrency.Value = 100;
            _coins = newCurrency;
            
            _playerData = new PlayerData
            {
                PlayerResources = new PlayerResources
                {
                    Currencies = new List<Currency> { _coins }
                }
            };

            _progress.PlayerData.Returns(_playerData);
            _storageService = new StorageService(_progress, _saveLoad);
        }

        [Test]
        public void AddCurrency_Should_IncreaseValue_And_InvokeEvent()
        {
            Currency received = null;
            _storageService.ChangedCurrencyEvent += c => received = c;

            _storageService.AddCurrency(CurrencyType.Gold, 50);

            Assert.AreEqual(150, _coins.Value);
            Assert.AreEqual(_coins, received);
            _saveLoad.Received().SaveProgress(SaveMethod.PlayerPrefs);
        }

        [Test]
        public void SubstractCurrency_Should_DecreaseValue_And_NotBelowZero()
        {
            _storageService.SubstractCurrency(CurrencyType.Gold, 150);

            Assert.AreEqual(0, _coins.Value);
            _saveLoad.Received().SaveProgress(SaveMethod.PlayerPrefs);
        }

        [Test]
        public void AddCurrency_WithCurrencyObject_Should_AddCorrectly()
        {
            var newCurrency = new Currency(CurrencyType.Gold);
            newCurrency.Value = 20;
            
            _storageService.AddCurrency(newCurrency);

            Assert.AreEqual(120, _coins.Value);
            _saveLoad.Received().SaveProgress(SaveMethod.PlayerPrefs);
        }

        [Test]
        public void SubstractCurrency_WithCurrencyObject_Should_SubtractCorrectly()
        {
            var newCurrency = new Currency(CurrencyType.Gold);
            newCurrency.Value = 40;

            _storageService.SubstractCurrency(newCurrency);

            Assert.AreEqual(60, _coins.Value);
            _saveLoad.Received().SaveProgress(SaveMethod.PlayerPrefs);
        }
    }
}