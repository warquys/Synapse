﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using InventorySystem.Items.Pickups;
using MapGeneration.Distributors;
using Mirror;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Objects;

//TODO: Fix Item Floating
public class SynapseLocker : StructureSyncSynapseObject, IJoinUpdate
{
    private readonly MirrorService _mirror;
    private readonly PlayerService _player;

    public static Dictionary<LockerType, Locker> Prefabs { get; } = new ();


    public Locker Locker { get; }
    public ReadOnlyCollection<SynapseLockerChamber> Chambers { get; private set; }
    public override GameObject GameObject => Locker.gameObject;
    public override NetworkIdentity NetworkIdentity => Locker.netIdentity;
    protected override NetworkBehaviour NetworkObject => Locker;
    public override ObjectType Type => ObjectType.Locker;
    public bool NeedsJoinUpdate => hideForAll;

    public override void OnDestroy()
    {
        Map._synapseLockers.Remove(this);
        Chambers = null;
        base.OnDestroy();
        
        if (Parent is SynapseSchematic schematic) schematic._lockers.Remove(this);
    }

    public void SpawnItem(ItemType type, int chamber, int amount = 1)
    {
        if(chamber >= 0 && Chambers.Count > chamber)
            Chambers[chamber].SpawnItem(type,amount);
        UnfreezeAll();
    }

    public string Name => GameObject.name;
    
    public LockerType SynapseLockerType { get; private set; }


    internal bool hideForAll = false;
    internal HashSet<SynapsePlayer> hideForPlayer = new ();

    public override void Refresh()
    {
        var message = _mirror.GetSpawnMessage(NetworkIdentity);
        var realScale = Scale;
        foreach (var player in _player.Players)
        {
            if (!hideForPlayer.Contains(player))
                player.Connection.Send(message);
        }
    }

    public SynapseLocker(LockerType lockerType, Vector3 position, Quaternion rotation, Vector3 scale,
        bool removeDefaultItems = false) : this()
    {
        Locker = CreateLocker(lockerType, position, rotation, scale, removeDefaultItems);
        SetUp(lockerType);
    }
    internal SynapseLocker(Locker locker) : this()
    {
        Locker = locker;
        SetUp(GetLockerType());
    }

    internal SynapseLocker(SchematicConfiguration.LockerConfiguration configuration,
        SynapseSchematic schematic) :
        this(configuration.LockerType, configuration.Position, configuration.Rotation, configuration.Scale,
            configuration.DeleteDefaultItems)
    {
        Parent = schematic;
        schematic._lockers.Add(this);

        OriginalScale = configuration.Scale;
        CustomAttributes = configuration.CustomAttributes;
        Update = configuration.Update;
        UpdateFrequency = configuration.UpdateFrequency;
        
        for (int i = 0; i < configuration.Chambers.Count; i++)
        {
            foreach (var item in configuration.Chambers[i].Items)
                SpawnItem(item, i);
        }
    }

    private SynapseLocker()
    {
        _mirror = Synapse.Get<MirrorService>();
        _player = Synapse.Get<PlayerService>();
    }

    private void SetUp(LockerType type)
    {
        Map._synapseLockers.Add(this);
        var comp = GameObject.AddComponent<SynapseObjectScript>();
        comp.Object = this;

        var list = new List<SynapseLockerChamber>();

        for (ushort i = 0; i < Locker.Chambers.Count(); i++)
            list.Add(new SynapseLockerChamber(Locker.Chambers[i], this, i));

        Chambers = list.AsReadOnly();
        SynapseLockerType = type;
    }
    private Locker CreateLocker(LockerType lockerType, Vector3 position, Quaternion rotation, Vector3 scale,
        bool removeDefaultItems = false)
    {
        var locker = CreateNetworkObject(Prefabs[lockerType], position, rotation, scale);

        foreach (var lockerChamber in locker.Chambers)
            lockerChamber._spawnpoint.SetParent(locker.transform);
        
        foreach (var pickupBase in locker.GetComponentsInChildren<ItemPickupBase>())
        {
            if (removeDefaultItems)
            {
                NetworkServer.Destroy(pickupBase.gameObject);
            }
            else if (pickupBase.TryGetComponent<Rigidbody>(out var rigidbody))
            {
                rigidbody.isKinematic = false;
                rigidbody.useGravity = true;
            }
        }

        return locker;
    }
    private LockerType GetLockerType()
    {
        if (Name.Contains("AdrenalineMedkit")) return LockerType.MedkitWallCabinet;
        if (Name.Contains("RegularMedkit")) return LockerType.AdrenalineWallCabinet;
        if (Name.Contains("MiscLocker")) return LockerType.StandardLocker;
        if (Name.Contains("RifleRack")) return LockerType.RifleRackLocker;
        if (Name.Contains("LargeGunLocker")) return LockerType.LargeGunLocker;
        if (Name.Contains("Scp018")) return LockerType.Scp018PedestalVariant;
        if (Name.Contains("Scp1576")) return LockerType.Scp1576PedestalVariant;
        if (Name.Contains("Scp1853")) return LockerType.Scp1853PedestalVariant;
        if (Name.Contains("Scp207")) return LockerType.Scp207PedestalVariant;
        if (Name.Contains("Scp2176")) return LockerType.Scp2176PedestalVariant;
        if (Name.Contains("Scp244")) return LockerType.Scp244PedestalVariant;
        if (Name.Contains("Scp268")) return LockerType.Scp268PedestalVariant;
        if (Name.Contains("Scp500")) return LockerType.Scp500PedestalVariant;
        if (Name.Contains("Pedestal")) return LockerType.ScpPedestal;
        return default;
    }
    private void UnfreezeAll()
    {
        foreach (Rigidbody rigidbody in SpawnablesDistributorBase.BodiesToUnfreeze)
            if (rigidbody != null)
            {
                rigidbody.isKinematic = false;
                rigidbody.useGravity = true;
            }
    }

    public void UpdatePlayer(SynapsePlayer player)
    {
        hideForPlayer.Add(player);
        Refresh();
    }

    public enum LockerType
    {
        StandardLocker,
        LargeGunLocker,
        RifleRackLocker,
        ScpPedestal,
        MedkitWallCabinet,
        AdrenalineWallCabinet,
        Scp018PedestalVariant,
        Scp207PedestalVariant,
        Scp244PedestalVariant,
        Scp268PedestalVariant,
        Scp500PedestalVariant,
        Scp1853PedestalVariant,
        Scp2176PedestalVariant,
        Scp1576PedestalVariant,
    }
    
    public class SynapseLockerChamber
    {
        public SynapseLockerChamber(LockerChamber chamber, SynapseLocker locker, ushort id)
        {
            LockerChamber = chamber;
            Locker = locker;
            ColliderID = id;
            ByteID = (ushort)(1 << id);
        }
        
        
        public SynapseLocker Locker { get; }
        public LockerChamber LockerChamber { get; }
        public ushort ByteID { get; }
        public ushort ColliderID { get; }

        public GameObject GameObject => LockerChamber.gameObject;

        public string Name => GameObject.name;

        public bool CanInteract => LockerChamber.CanInteract;

        public Vector3 Position => GameObject.transform.position;

        public bool Open
        {
            get => (Locker.Locker.OpenedChambers & ByteID) == ByteID;
            set
            {
                LockerChamber.IsOpen = value;
                Locker.Locker.RefreshOpenedSyncvar();
                if (value)
                    Locker.Locker.OpenedChambers = (ushort)(Locker.Locker.OpenedChambers | ByteID);
                else
                    Locker.Locker.OpenedChambers = (ushort)(Locker.Locker.OpenedChambers & ~ByteID);
            }
        }

        public void SpawnItem(ItemType type, int amount = 1)
            => LockerChamber.SpawnItem(type, amount);
    }
}