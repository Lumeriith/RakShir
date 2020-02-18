using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SFXInstance : MonoBehaviourPun
{
    public Vector2 pitchRandomMultiplier = new Vector2(0.9f, 1.1f);
    public bool destroyWhenNotPlaying = true;

    private AudioSource source;
    private Transform followTarget;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    private void Start()
    {
        source.pitch *= Random.Range(pitchRandomMultiplier.x, pitchRandomMultiplier.y);
    }

    private void Update()
    {
        if (followTarget != null) transform.position = followTarget.position;
    }
    private void FixedUpdate()
    {
        if(photonView.IsMine && destroyWhenNotPlaying && !source.isPlaying)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
    public void Follow(MonoBehaviourPun target)
    {
        photonView.RPC("RpcFollow", RpcTarget.All, target.photonView.ViewID);
    }

    [PunRPC]
    private void RpcFollow(int viewID)
    {
        PhotonView view = PhotonNetwork.GetPhotonView(viewID);
        if (view = null) return;
        followTarget = view.transform;
    }

    public void Destroy()
    {
        photonView.RPC("RpcDestroy", photonView.Owner);
    }

    public void DestroyAfterFinished()
    {
        photonView.RPC("RpcDestroyAfterFinished", photonView.Owner);
    }

    public void DestroyFadingOut(float time)
    {
        photonView.RPC("RpcDestroyFadingOut", RpcTarget.All, time);
    }

    public void Play()
    {
        photonView.RPC("RpcPlay", RpcTarget.All);
    }

    public void Stop()
    {
        photonView.RPC("RpcStop", RpcTarget.All);
    }

    [PunRPC]
    private void RpcPlay()
    {
        source.Stop();
        source.Play();
    }
    
    [PunRPC]
    private void RpcStop()
    {
        source.Stop();
    }

    [PunRPC]
    private void RpcDestroy()
    {
        PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    private void RpcDestroyAfterFinished()
    {
        StartCoroutine(CoroutineDestroyAfterFinished());
    }

    [PunRPC]
    private void RpcDestroyFadingOut(float time)
    {
        StartCoroutine(CoroutineDestroyFadingOut(time));
    }

    private IEnumerator CoroutineDestroyFadingOut(float time)
    {
        float volume = source.volume;
        for(float t = 0; t < time; t += Time.deltaTime)
        {
            source.volume = Mathf.Lerp(volume, 0f, t / time);
            yield return null;
        }
        source.Stop();
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private IEnumerator CoroutineDestroyAfterFinished()
    {
        source.loop = false;
        while (source.isPlaying)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
    }
}
