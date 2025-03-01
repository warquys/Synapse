﻿using Hints;
using InventorySystem;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;
using Mirror;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Item.SubAPI;

public class Throwable : ISubSynapseItem
{
    private readonly SynapseItem _item;
    
    public Throwable(SynapseItem item)
    {
        _item = item;
    }
    
    /// <summary>
    /// The Projectile Object of the Item
    /// </summary>
    public ThrownProjectile Projectile { get; internal set; }
    
    /// <summary>
    /// The Time that is left until the Grenade is exploded
    /// </summary>
    public double FuseTime
    {
        get => Projectile == null ? 0 : ((TimeGrenade)Projectile).TargetTime - NetworkTime.time;
        set
        {
            if (Projectile == null) return;
            var comp = (TimeGrenade)Projectile;
            comp.TargetTime = value + NetworkTime.time;
            comp.syncVarDirtyBits = ~(0uL);
        }
    }

    /// <summary>
    /// Activates the Grenade when it's already on the Map
    /// </summary>
    public void Fuse(SynapsePlayer owner = null)
    {
        if(_item.State != ItemState.Map) return;
        if(!InventoryItemLoader.AvailableItems.TryGetValue(_item.ItemType, out var itemBase)) return;
        if(itemBase is not ThrowableItem throwableItem) return;

        Projectile = Object.Instantiate(throwableItem.Projectile, _item.Pickup.Position, _item.Pickup.Rotation);
        Projectile.transform.localScale = _item.Scale;
        if (Projectile.TryGetComponent<Rigidbody>(out var rigidbodyGrenade) &&
            _item.Pickup.TryGetComponent<Rigidbody>(out var rigidbodyItem))
        {
            rigidbodyGrenade.velocity = rigidbodyItem.velocity;
            rigidbodyGrenade.angularVelocity = rigidbodyItem.angularVelocity;
        }

        _item.Pickup.Info.Locked = true;
        Projectile.NetworkInfo = _item.Pickup.Info;
        if (owner != null)
            Projectile.PreviousOwner = owner;
        NetworkServer.Spawn(Projectile.gameObject);
        Projectile.InfoReceivedHook(default, _item.Pickup.Info);

        var comp = Projectile.gameObject.AddComponent<SynapseObjectScript>();
        comp.Object = _item;
        
        Projectile.ServerActivate();

        _item.DestroyItem();
        _item.DestroyPickup();
        _item.State = ItemState.Thrown;
    }

    public void Throw(Vector3 startVelocity = default, bool fullForce = false)
    {
        if(_item.State != ItemState.Inventory || _item.Item is not ThrowableItem throwableItem) return;
        
        var settings = fullForce ? throwableItem.FullThrowSettings : throwableItem.WeakThrowSettings;

        Throw(settings.StartVelocity, settings.UpwardsFactor, settings.StartTorque, startVelocity);
    }

    public void Throw(float forceAmount, float upwardFactor, Vector3 torque, Vector3 startVel)
    {
        if(_item.State != ItemState.Inventory || _item.Item is not ThrowableItem throwableItem) return;
        
        if(throwableItem._alreadyFired) return;

        throwableItem._destroyTime = Time.timeSinceLevelLoad + throwableItem._postThrownAnimationTime;
        throwableItem._alreadyFired = true;
        Projectile = Object.Instantiate(throwableItem.Projectile, throwableItem.Owner.PlayerCameraReference.position,
            throwableItem.Owner.PlayerCameraReference.rotation);
        var transform = Projectile.transform;
        transform.localScale = _item.Scale;
        var info = new PickupSyncInfo
        {
            ItemId = _item.ItemType,
            Locked = !throwableItem._repickupable,
            Serial = _item.Serial,
            WeightKg = _item.Weight,
        };
        Projectile.NetworkInfo = info;//Can break here
        Projectile.PreviousOwner = _item.ItemOwner;
        NetworkServer.Spawn(Projectile.gameObject);
        
        var comp = Projectile.gameObject.AddComponent<SynapseObjectScript>();
        comp.Object = _item;

        if (Projectile.TryGetComponent<Rigidbody>(out var rb))
        {
            throwableItem.PropelBody(rb, torque, startVel, forceAmount, upwardFactor);
        }
        Projectile.ServerActivate();

        _item.DestroyItem();
        _item.DestroyPickup();
        _item.SetState(ItemState.Thrown);
    }

    /// <summary>
    /// Destroys the Projectile Object of the SynapseItem
    /// </summary>
    internal void DestroyProjectile()
    {
        if (Projectile != null)
            NetworkServer.Destroy(Projectile.gameObject);
        Projectile = null;
    }

    public float Durability
    {
        get => (float)FuseTime;
        set => FuseTime = value;
    }

    public void ChangeState(ItemState newState) { }
}