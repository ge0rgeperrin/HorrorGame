using System;
using Subterranea;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class InteractableItem : MonoBehaviour
{
    public new Rigidbody rigidbody;

    private void Start()
    {
        if (rigidbody != null)
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        gameObject.layer = PlayerController.Instance.itemMask;
    }

    private void Pickup()
    {
        
    }

    private void Drop()
    {
        
    }
}