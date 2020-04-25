using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Engine;
public class MoonAltar : Activatable
{
    public override Entity entity => null;

    protected override void Start()
    {
        base.Start();
    }


    protected override void OnChannelStart(Entity activator) { }
    protected override void OnChannelCancel(Entity activator) { }
    protected override void OnChannelSuccess(Entity activator)
    {
        if (!activator.photonView.IsMine) return;
        GameEventMessage.SendEvent("Shop");

    }
}
