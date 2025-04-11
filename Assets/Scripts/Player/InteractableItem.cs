using System;
using PallonAnticheat;
using Subterranea;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class InteractableItem : MonoBehaviour
{
    public Sprite slotDisplay;
    public new Rigidbody rigidbody;

    public string GetItemName()
    {
        return this.gameObject.name;
    }
    
    private void Start()
    {
        if (rigidbody != null)
        {
            rigidbody = GetComponent<Rigidbody>();
        }
    }

    public void Pickup()
    {
        MonkeLogger.Log($"Picked up item {gameObject.name}");
        PlayerController.AddItemToInventory(this);
    }

    public void Drop()
    {
        MonkeLogger.Log($"Dropped up item {gameObject.name}");
        PlayerController.DropItemFromInventory(this);
    }
}