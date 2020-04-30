using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Sirenix.OdinInspector;
using System;

public abstract class Item : Activatable, IInfoTextable
{
    private readonly Type[] ComponentsToDisable = { typeof(Rigidbody), typeof(Collider), typeof(Renderer), typeof(Light) };

    [BoxGroup("Item Metadata"), HorizontalGroup("Item Metadata/horizontal", 50), VerticalGroup("Item Metadata/horizontal/icon")]
    [PreviewField(50, ObjectFieldAlignment.Left)]
    [HideLabel]
    public Sprite itemIcon;
    [HorizontalGroup("Item Metadata/horizontal"), VerticalGroup("Item Metadata/horizontal/text")]
    [HideLabel]
    public string itemName = "Item Name";
    [HorizontalGroup("Item Metadata/horizontal"), VerticalGroup("Item Metadata/horizontal/text")]
    public ItemTier itemTier;
    [HorizontalGroup("Item Metadata/horizontal"), VerticalGroup("Item Metadata/horizontal/text")]
    public float value;
    
    [HideLabel]
    [MultiLineProperty(6)]
    [BoxGroup("Item Metadata")]
    public string itemDescription = "This is an awesome ability!";

    public Entity owner { get; set; }

    private Vector3 startPosition;

    public override Entity entity => owner;

    private List<Component> _disabledComponents;

    protected override void Start()
    {
        base.Start();
        startPosition = transform.position;
        Rigidbody rb = GetComponent<Rigidbody>();
        if(rb != null)
        {
            rb.constraints = RigidbodyConstraints.None;
            rb.drag = 1f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }
    private void FixedUpdate()
    {

        if(transform.position.y < startPosition.y - 15f)
        {
            GetComponent<Rigidbody>().velocity = Vector3.up * 2f; // QoL change. Fallen items will come back.
            transform.position = startPosition + Vector3.up;
        }

    }

    public void Own(Entity owner)
    {
        photonView.RPC(nameof(RpcOwn), RpcTarget.All, owner.photonView.ViewID);
    }

    public void Disown()
    {
        if (owner == null) return;
        photonView.RPC(nameof(RpcDisown), RpcTarget.All);
    }

    public void DestroySelf()
    {
        photonView.RPC(nameof(RpcDestroySelf), photonView.Owner);
    }

    [PunRPC]
    public void RpcDestroySelf()
    {
        PhotonNetwork.Destroy(gameObject);
    }

    protected override void OnChannelCancel(Entity activator)
    {

    }

    protected override void OnChannelStart(Entity activator)
    {

    }

    protected override void OnChannelSuccess(Entity activator)
    {
        if (activator.photonView.IsMine)
        {
            activator.GetComponent<PlayerInventory>().Pickup(this);
        }
    }


    [PunRPC]
    protected void RpcOwn(int owner_id)
    {
        Entity livingThing = PhotonNetwork.GetPhotonView(owner_id).GetComponent<Entity>();

        if (owner != null)
        {
            RpcDisown();
        }

        owner = livingThing;
        transform.SetParent(owner.transform);
        transform.position = owner.transform.position;
        DisableComponents();
    }


    [PunRPC]
    protected void RpcDisown()
    {
        if (owner == null) return;
        transform.position = owner.transform.position + owner.GetCenterOffset();
        startPosition = transform.position;
        owner = null;
        transform.SetParent(null);
        EnableComponents();
        SFXManager.CreateSFXInstance("si_local_ItemDrop", transform.position, true);
    }

    private void DisableComponents()
    {
        if (_disabledComponents == null) _disabledComponents = new List<Component>();
        for (int i = 0; i < ComponentsToDisable.Length; i++)
        {
            dynamic[] components = GetComponents(ComponentsToDisable[i]);
            for (int j = 0; j < components.Length; j++)
            {
                if (IsComponentEnabled(components[j]))
                {
                    _disabledComponents.Add(components[j]);
                    DisableComponent(components[j]);
                }
            }

            components = GetComponentsInChildren(ComponentsToDisable[i]);
            for (int j = 0; j < components.Length; j++)
            {
                if (IsComponentEnabled(components[j]))
                {
                    _disabledComponents.Add(components[j]);
                    DisableComponent(components[j]);
                }
            }
        }
    }

    private bool IsComponentEnabled(Component component)
    {
        if (component is Rigidbody rb) return rb.detectCollisions && !rb.isKinematic;

        dynamic dComponent = component;
        return dComponent.enabled;
    }

    private void DisableComponent(Component component)
    {
        if (component == null) return;
        if (component is Rigidbody rb)
        {
            rb.detectCollisions = false;
            rb.isKinematic = true;
            return;
        }

        dynamic dComponent = component;
        dComponent.enabled = false;
    }

    private void EnableComponent(Component component)
    {
        if (component == null) return;
        if (component is Rigidbody rb)
        {
            rb.detectCollisions = true;
            rb.isKinematic = false;
            return;
        }

        dynamic dComponent = component;
        dComponent.enabled = true;
    }

    private void EnableComponents()
    {
        if (_disabledComponents == null) return;
        for (int i = 0; i < _disabledComponents.Count; i++)
        {
            EnableComponent(_disabledComponents[i]);
        }
        _disabledComponents = null;
    }

    public virtual void OnInfoTextClick()
    {
        UnitControlManager.instance.selectedUnit.control.CommandActivate(this, Input.GetKey(UnitControlManager.instance.reservationModifier));
        Instantiate(UnitControlManager.instance.commandMarkerInterest, transform.position, Quaternion.identity, transform);
    }

    public virtual bool shouldShowInfoText => owner == null;
    public virtual InfoTextIcon infoTextIcon => InfoTextIcon.Consumable;
    public virtual Vector3 infoTextWorldPosition => transform.position;
    public virtual string infoTextName => itemName;
}
