using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SFXInstance : MonoBehaviourPun
{
    public Vector2 pitchRandomMultiplier = new Vector2(0.9f, 1.1f);
    public bool destroyWhenNotPlaying = true;

    [SerializeField]
    private AudioClip[] _clips = new AudioClip[0];

    private AudioSource _source;
    private Transform _followTarget;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        _source.pitch *= Random.Range(pitchRandomMultiplier.x, pitchRandomMultiplier.y);
        if (_clips.Length > 0) _source.clip = _clips[Random.Range(0, _clips.Length)];
        else _source.clip = null;
        _source.Play();
    }

    private void Update()
    {
        if (_followTarget != null) transform.position = _followTarget.position;
    }
    private void FixedUpdate()
    {
        if(photonView.IsMine && destroyWhenNotPlaying && !_source.isPlaying)
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
        if (view == null) return;
        _followTarget = view.transform;
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
        _source.Stop();
        _source.Play();
    }
    
    [PunRPC]
    private void RpcStop()
    {
        _source.Stop();
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
        float volume = _source.volume;
        for(float t = 0; t < time; t += Time.deltaTime)
        {
            _source.volume = Mathf.Lerp(volume, 0f, t / time);
            yield return null;
        }
        _source.Stop();
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private IEnumerator CoroutineDestroyAfterFinished()
    {
        _source.loop = false;
        while (_source.isPlaying)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
    }
}
