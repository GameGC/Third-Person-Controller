using MTPS.Inventory;
using MTPS.Inventory.ItemTypes;
using UnityEngine;
using UnityEngine.UI;

namespace MTPS.Shooter.Scripts.UI
{
    public class GameHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HealthComponent healthComponent;
        [SerializeField] private BaseInventory inventory;

        [Header("Health")]
        [SerializeField] private Image healthFill;

        [Header("Inventory Items")]
        [SerializeField] private BaseItemData coinItem;
        [SerializeField] private Text coinCountText;
        [SerializeField] private BaseItemData keyItem;
        [SerializeField] private GameObject keyIndicator;

        private int _lastCoinCount = -1;
        private int _lastKeyCount = -1;

        private void OnEnable()
        {
            if (inventory != null)
            {
                inventory.onItemIncreased.AddListener(OnItemChanged);
                inventory.onItemReduced.AddListener(OnItemChanged);
                inventory.onItemRemoved.AddListener(OnItemRemoved);
            }
        }

        private void OnDisable()
        {
            if (inventory != null)
            {
                inventory.onItemIncreased.RemoveListener(OnItemChanged);
                inventory.onItemReduced.RemoveListener(OnItemChanged);
                inventory.onItemRemoved.RemoveListener(OnItemRemoved);
            }
        }

        private void Update()
        {
            UpdateHealth();
            UpdateCoinCount();
            UpdateKeyIndicator();
        }

        private void UpdateHealth()
        {
            if (healthComponent == null || healthFill == null) return;
            healthFill.fillAmount = Mathf.Clamp01(healthComponent.Health / 100f);
        }

        private int GetItemCount(BaseItemData itemData)
        {
            if (inventory == null || itemData == null) return 0;
            return inventory.AllItems.TryGetValue(itemData, out int count) ? count : 0;
        }

        private void UpdateCoinCount()
        {
            if (coinItem == null || coinCountText == null) return;
            int count = GetItemCount(coinItem);
            if (count != _lastCoinCount)
            {
                _lastCoinCount = count;
                coinCountText.text = count.ToString();
            }
        }

        private void UpdateKeyIndicator()
        {
            if (keyItem == null || keyIndicator == null) return;
            int count = GetItemCount(keyItem);
            bool hasKey = count > 0;
            if (count != _lastKeyCount)
            {
                _lastKeyCount = count;
                keyIndicator.SetActive(hasKey);
            }
        }

        private void OnItemChanged(BaseItemData item, int count)
        {
            UpdateCoinCount();
            UpdateKeyIndicator();
        }

        private void OnItemRemoved(BaseItemData item)
        {
            UpdateCoinCount();
            UpdateKeyIndicator();
        }
    }
}