using System;
using System.Collections;
using MTPS.Inventory;
using MTPS.Inventory.ItemTypes;
using UnityEngine;

namespace MTPS.Shooter
{
    public class DoorOpen : MonoBehaviour
    {
        [SerializeField] private BaseItemData requiredKey;
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float openSpeed = 2f;

        private bool _isOpen;
        private Quaternion _closedRotation;
        private Quaternion _openRotation;

        private void Awake()
        {
            _closedRotation = transform.rotation;
            _openRotation = _closedRotation * Quaternion.Euler(0f, openAngle, 0f);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_isOpen) return;

            var inventory = collision.transform.root.GetComponent<BaseInventory>();

            Debug.Log("collide");
            if (inventory.MinusItem(requiredKey))
                StartCoroutine(OpenDoor());
        }

        private IEnumerator OpenDoor()
        {
            _isOpen = true;
            Debug.Log("collide2");
            Quaternion current = transform.rotation;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * openSpeed;
                Debug.Log(transform.rotation.eulerAngles);

                transform.rotation = Quaternion.Slerp(current, _openRotation, t);
                yield return null;
            }

            transform.rotation = _openRotation;
        }
    }
}
