using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;



public enum EquipmentType { Helmet, Armor, Boots, Weapon, Ring }
public enum BodyPart { Head, LeftHand, RightHand, LeftFoot, RightFoot }

[System.Serializable]
public struct Attachment
{
    public Transform transform;
    public BodyPart attachTo;
    public Vector3 offsetPosition;
    public Vector3 offsetRotation;
}


public abstract class Equippable : Activatable
{
    [Header("Equippable Settings")]
    public EquipmentType type;
    public AbilityTrigger[] skillSetReplacements = new AbilityTrigger[6];
    public List<Attachment> attachments = new List<Attachment>();

    public bool debug_UpdateAttachmentsEveryFrame = false;

    [HideInInspector]
    public LivingThing owner = null;
    private void Awake()
    {
        foreach (Attachment attachment in attachments)
        {
            attachment.transform.position = transform.position;
            attachment.transform.SetParent(transform);
            attachment.transform.gameObject.SetActive(false);
        }
    }

    public void Equip()
    {
        photonView.RPC("RpcEquip", RpcTarget.All, owner.photonView.ViewID);
    }

    public void Unequip()
    {
        photonView.RPC("RpcUnequip", RpcTarget.All, owner.photonView.ViewID);
    }

    public abstract void OnEquip(LivingThing owner);
    public abstract void OnUnequip(LivingThing owner);

    [PunRPC]
    protected void RpcEquip(int owner_id)
    {
        LivingThing owner = PhotonNetwork.GetPhotonView(owner_id).GetComponent<LivingThing>();
        for (int i = 0; i < skillSetReplacements.Length; i++)
        {
            
            if (skillSetReplacements[i] != null)
            {
                float remainingCooldownTime = owner.control.skillSet[i] == null ? 0 : owner.control.skillSet[i].remainingCooldownTime;
                owner.control.skillSet[i] = skillSetReplacements[i];
                owner.control.skillSet[i].StartCooldown(remainingCooldownTime, true);
            }
        }
        UpdateAttachments(owner);
        OnEquip(owner);
    }

    [PunRPC]
    protected void RpcUnequip(int owner_id)
    {
        LivingThing owner = PhotonNetwork.GetPhotonView(owner_id).GetComponent<LivingThing>();
        for (int i = 0; i < skillSetReplacements.Length; i++)
        {
            if (skillSetReplacements[i] != null)
            {
                owner.control.skillSet[i] = null;
            }
        }
        DetachAttachments();
        OnUnequip(owner);
    }

    private void DetachAttachments()
    {
        foreach (Attachment attachment in attachments)
        {
            attachment.transform.position = transform.position;
            attachment.transform.SetParent(transform);
            attachment.transform.gameObject.SetActive(false);
        }
    }

    private void UpdateAttachments(LivingThing owner)
    {
        foreach (Attachment attachment in attachments)
        {
            switch (attachment.attachTo)
            {
                case BodyPart.Head:
                    attachment.transform.SetParent(owner.head);
                    break;
                case BodyPart.LeftHand:
                    attachment.transform.SetParent(owner.leftHand);
                    break;
                case BodyPart.RightHand:
                    attachment.transform.SetParent(owner.rightHand);
                    break;
                case BodyPart.LeftFoot:
                    attachment.transform.SetParent(owner.leftFoot);
                    break;
                case BodyPart.RightFoot:
                    attachment.transform.SetParent(owner.rightFoot);
                    break;
            }
            attachment.transform.position = attachment.transform.parent.position;
            attachment.transform.rotation = attachment.transform.parent.rotation;
            attachment.transform.Translate(attachment.offsetPosition);
            attachment.transform.Rotate(attachment.offsetRotation, Space.Self);
            attachment.transform.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (debug_UpdateAttachmentsEveryFrame)
        {
            LivingThing owner = GetComponentInParent<LivingThing>();
            if (owner == null) return;
            UpdateAttachments(owner);
        }
    }







    protected override void OnChannelCancel(LivingThing activator)
    {

    }

    protected override void OnChannelStart(LivingThing activator)
    {

    }

    protected override void OnChannelSuccess(LivingThing activator)
    {
        PlayerItemBelt belt = activator.GetComponent<PlayerItemBelt>();
        if (belt == null) return;
        if (owner != null) return;
        owner = activator;
        if (belt.AddEquippable(this))
        {
            transform.SetParent(activator.transform);
            transform.position = activator.transform.position;
            gameObject.SetActive(false);
        }
    }
}
