using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class KeyAction : MonoBehaviour
{
    [SerializeField] private InputAction actionToTrigger;
    [Space]
    [SerializeField] private UnityEvent onPress;

    private void OnEnable()
    {
        actionToTrigger.Enable();
        actionToTrigger.performed += Trigger;
    }

    private void OnDisable()
    {
        actionToTrigger.performed -= Trigger;
        actionToTrigger.Disable();
    }

    private void Trigger(InputAction.CallbackContext c) => onPress.Invoke();


    //other methods
    public void ToggleGameObject(GameObject obj) => obj.SetActive(!obj.activeSelf);
}
