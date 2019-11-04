using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using NaughtyAttributes;
using Photon.Realtime;

public enum ItemTier { Common, Rare, Epic, Legendary }
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


public abstract class Equipment : Item
{
    [Header("Equipment Settings")]
    public EquipmentType type;
    public AbilityTrigger[] skillSetReplacements = new AbilityTrigger[7];
    public List<Attachment> attachments = new List<Attachment>();

    private bool isEquipped = false;


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
        photonView.RPC("RpcEquip", RpcTarget.All);
    }

    public void Unequip()
    {
        photonView.RPC("RpcUnequip", RpcTarget.All);
    }

    public abstract void OnEquip(LivingThing owner);
    public abstract void OnUnequip(LivingThing owner);

    [PunRPC]
    protected void RpcEquip()
    {
        isEquipped = true;
        for (int i = 0; i < skillSetReplacements.Length; i++)
        {
            
            if (skillSetReplacements[i] != null)
            {
                owner.control.skillSet[i] = skillSetReplacements[i];
                owner.control.skillSet[i].skillIndex = i;
                owner.control.skillSet[i].owner = owner;
                skillSetReplacements[i].transform.parent = owner.transform;
                if (owner.photonView.IsMine) owner.control.skillSet[i].OnEquip();
            }
        }
        UpdateAttachments(owner);
        OnEquip(owner);
    }

    [PunRPC]
    protected void RpcUnequip()
    {
        isEquipped = false;
        for (int i = 0; i < skillSetReplacements.Length; i++)
        {
            if (skillSetReplacements[i] != null)
            {
                if (owner.photonView.IsMine) owner.control.skillSet[i].OnUnequip();
                owner.control.skillSet[i].skillIndex = -1;
                owner.control.skillSet[i].owner = null;
                owner.control.skillSet[i] = null;
                skillSetReplacements[i].transform.parent = transform;

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




}
