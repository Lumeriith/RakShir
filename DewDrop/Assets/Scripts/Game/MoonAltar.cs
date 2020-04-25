using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Engine;
public class MoonAltar : Activatable
{
    public override LivingThing entity => null;

    protected override void Start()
    {
        base.Start();
    }


    protected override void OnChannelStart(LivingThing activator) { }
    protected override void OnChannelCancel(LivingThing activator) { }
    protected override void OnChannelSuccess(LivingThing activator)
    {
        if (!activator.photonView.IsMine) return;
        GameEventMessage.SendEvent("Shop");

    }
}
