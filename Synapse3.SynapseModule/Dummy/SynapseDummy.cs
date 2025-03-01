﻿using CentralAuth;
using InventorySystem.Items;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Player;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Synapse3.SynapseModule.Dummy;

public class SynapseDummy : DefaultSynapseObject, IRefreshable
{
    private readonly DummyService _dummy;
    private readonly PlayerService _player;
    
    public MovementDirection Direction { get; set; }

    public FakeConnection FakeConnection { get; }
    
    public bool RaVisible
    {
        get => Player.RaVisible;
        set => Player.RaVisible = value;
    }

    public bool SpectatorVisible
    {
        get => Player.SpectatorVisible;
        set => Player.SpectatorVisible = value;
    }

    public float WalkSpeed
    {
        get => Player.WalkSpeed;
        set
        {
            var firstPerson = Player.FirstPersonMovement;
            if (firstPerson != null)
                firstPerson.WalkSpeed = value;
        }
    }

    public float SneakSpeed
    {
        get => Player.SneakSpeed;
        set
        {
            var firstPerson = Player.FirstPersonMovement;
            if (firstPerson != null)
                firstPerson.SneakSpeed = value;
        }
    }

    public float RunSpeed
    {
        get => Player.RunSpeed;
        set
        {
            var firstPerson = Player.FirstPersonMovement;
            if (firstPerson != null)
                firstPerson.SprintSpeed = value;
        }
    }

    public float CrouchingSpeed
    {
        get => Player.CrouchingSpeed;
        set
        {
            var firstPerson = Player.FirstPersonMovement;
            if (firstPerson != null)
                firstPerson.CrouchSpeed = value;
        }
    }

    public bool DestroyWhenDied
    {
        get => Player.DestroyWhenDied;
        set => Player.DestroyWhenDied = value;
    }

    public override GameObject GameObject { get; }
    public override ObjectType Type => ObjectType.Dummy;

    public override Vector3 Position
    {
        get => Player.Position;
        set => Player.Position = value;
    }

    public override Quaternion Rotation
    {
        get => Player.Rotation;
        set => Player.SetRotation(value);
    }

    public Vector2 RotationVector2
    {
        get => Player.RotationVector2;
        set => Player.SetRotation(value);
    }
    public float RotationVertical
    {
        get => Player.RotationVertical;
        set => Player.SetRotationVertical(value);
    }

    public float RotationHorizontal
    {
        get => Player.RotationHorizontal;
        set => Player.SetRotationHorizontal(value);
    }

    public override Vector3 Scale
    {
        get => Player.Scale;
        set => Player.Scale = value;
    }

    public string Name
    {
        get => Player.NickName;
        set => Player.DisplayName = value;
    }

    public ItemType HeldItem
    {
        get => Player.VanillaInventory.NetworkCurItem.TypeId;
        set => Player.VanillaInventory.NetworkCurItem = new ItemIdentifier(value, 0);
    }

    public PlayerMovementState Movement
    {
        get => Player.FirstPersonMovement?.CurrentMovementState ?? PlayerMovementState.Crouching;
        set
        {
            if (Player.FirstPersonMovement != null)
                Player.FirstPersonMovement.CurrentMovementState = value;
        }
    }

    public DummyPlayer Player { get; }

    public bool PlayerUpdate { get; set; }

    public SynapseDummy(Vector3 position, Quaternion rotation, RoleTypeId role, string name, string badge = "",
        string badgeColor = "") : this(position, role, name, badge, badgeColor)
    {
        PlayerUpdate = false;
        Rotation = rotation;
    }

    public SynapseDummy(Vector3 position, Vector2 rotation, RoleTypeId role, string name, string badge = "",
        string badgeColor = "") : this(position, role, name, badge, badgeColor)
    {
        PlayerUpdate = true;
        RotationVector2 = rotation;
    }

    private SynapseDummy(Vector3 position, RoleTypeId role, string name, string badge, string badgeColor)
    {
        _dummy = Synapse.Get<DummyService>();
        _player = Synapse.Get<PlayerService>();

        GameObject = Object.Instantiate(NetworkManager.singleton.playerPrefab, _dummy._dummyParent);
        Player = GameObject.GetComponent<DummyPlayer>();

        var comp = GameObject.AddComponent<SynapseObjectScript>();
        comp.Object = this;
        
        FakeConnection = new FakeConnection(Player.Hub._playerId);
        NetworkServer.AddPlayerForConnection(FakeConnection, GameObject);

        Player.SynapseDummy = this;
        Player.RoleType = role;
        Player.Health = Player.MaxHealth;
        Player.NicknameSync.Network_myNickSync = name;
        Player.RankName = badge;
        Player.RankColor = badgeColor;
        Player.Position = position;
        Player.Hub.authManager.InstanceMode = ClientInstanceMode.Unverified;

        if (Player.ExistsInSpace)
        {
            Player.FirstPersonMovement.IsGrounded = true;
        }

        _ = Timing.RunCoroutine(UpdateMovement());

        MoveInElevator = true;
    }

    internal SynapseDummy(SchematicConfiguration.DummyConfiguration configuration, SynapseSchematic schematic) :
        this(configuration.Position, configuration.Rotation,
            configuration.Role, configuration.Name,
            configuration.Badge, configuration.BadgeColor)
    {
        Parent = schematic;
        schematic._dummies.Add(this);
        OriginalScale = configuration.Scale;
        CustomAttributes = configuration.CustomAttributes;
    }

    public void Refresh()
    {
        Position = base.Position;
        Rotation = base.Rotation;
        Scale = base.Scale;
    }

    public bool Update { get; set; } = false;

    public float UpdateFrequency { get; set; }

    public void RotateToPosition(Vector3 pos)
    {
        Rotation = Quaternion.LookRotation((pos - GameObject.transform.position).normalized);
    }

    public void DeSpawn()
        => NetworkServer.UnSpawn(GameObject);

    public void Spawn()
        => NetworkServer.Spawn(GameObject);

    //Thanks to GameHunt.I used some of his code for the Dummy API https://github.com/gamehunt/CustomNPCs
    private IEnumerator<float> UpdateMovement()
    {
        Vector3 lastPos = Vector3.zero;

        while (true)
        {
            yield return Timing.WaitForSeconds(0.1f);

            try
            {
                if (GameObject == null) yield break;
                if (Direction == MovementDirection.None || !Player.ExistsInSpace) continue;

                var speed = 0f;

                switch (Movement)
                {
                    case PlayerMovementState.Sneaking:
                        speed = SneakSpeed;
                        break;

                    case PlayerMovementState.Sprinting:
                        speed = RunSpeed;
                        break;

                    case PlayerMovementState.Walking:
                        speed = WalkSpeed;
                        break;

                    case PlayerMovementState.Crouching:
                        speed = CrouchingSpeed;
                        break;
                }


                switch (Direction)
                {

                    case MovementDirection.Forward:
                        var pos = Position + Player.CameraReference.forward / 10 * speed;
                        Player.Position = pos;
                        break;

                    case MovementDirection.BackWards:
                        pos = Position - Player.CameraReference.forward / 10 * speed;
                        Player.Position = pos;
                        break;

                    case MovementDirection.Right:
                        pos = Position + Quaternion.AngleAxis(90, Vector3.up) * Player.CameraReference.forward / 10 * speed;
                        Player.Position = pos;
                        break;

                    case MovementDirection.Left:
                        pos = Position - Quaternion.AngleAxis(90, Vector3.up) * Player.CameraReference.forward / 10 * speed;
                        Player.Position = pos;
                        break;
                }

                if (lastPos.normalized == Player.Position.normalized)
                {
                    Direction = MovementDirection.None;
                }
                lastPos = Player.Position;
            }
            catch (Exception e)
            {
                SynapseLogger<Synapse>.Error($"Sy3 Dummy: Update Failed:\n{e}");
            }
        }
    }

    public override void Destroy()
    {
        FakeConnection.Disconnect();
        NetworkServer.Destroy(GameObject);
    }
}