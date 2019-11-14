using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class Obelisk : MonoBehaviour
    {
        [Header("Source")]
        public Material obelisk_branch;
        public Material obelisk_branchLight;
        public Material obelisk_mainLight;
        public Material obelisk_mask;
        public GameObject laser;
        public ParticleSystem explosionEff;
        public Light light;
        [Header("Animation Speed Setting")]
        public AnimationCurve animationSpeedCurve;
        public float speedFac = 3f;
        [Header("Dissolve Setting")]
        public bool updateGI;
        public List<Renderer> branches = new List<Renderer>();
        public float lightDissolveTime;

        private Animator anim;
        private AnimationClip activationAnimation;
        private float playTime;
        private void Awake()
        {
            anim = GetComponent<Animator>();
            activationAnimation = anim.GetCurrentAnimatorClipInfo(0)[0].clip;
            playTime = activationAnimation.length;
            lightDissolveTime += playTime;
            anim.enabled = false;
            ResetObelisk(false);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine("OnActivateObelisk", false);
            }
            else if (Input.GetMouseButtonDown(1))
            {
                StartCoroutine("OnActivateObelisk", true);
            }
        }

        public void ResetObelisk(bool invert)
        {
            if (!invert)
            {
                obelisk_mainLight.DisableKeyword("_EMISSION");

                obelisk_branch.SetFloat("_DissolveCutoff", 1);
                obelisk_branchLight.SetFloat("_DissolveCutoff", 1);
                obelisk_mask.SetFloat("_DissolveCutoff", 0);
                laser.SetActive(false);
                light.gameObject.SetActive(false);
            }
            else
            {
                obelisk_mainLight.EnableKeyword("_EMISSION");

                obelisk_branch.SetFloat("_DissolveCutoff", 0);
                obelisk_branchLight.SetFloat("_DissolveCutoff", 0);
                obelisk_mask.SetFloat("_DissolveCutoff", 1);
                laser.SetActive(true);
                light.gameObject.SetActive(true);
            }
        }

        public IEnumerator OnActivateObelisk(bool invert)
        {
            float deltaPlayTime = 0;
            float deltaLDT = 0;
            bool doneAnim = false;

            while (true)
            {
                if (animationSpeedCurve == null) yield break;
                anim.enabled = true;

                if (!invert)
                {
                    if (deltaPlayTime >= playTime && !doneAnim)
                    {
                        ResetObelisk(!invert);
                        explosionEff.Play();
                        doneAnim = true;
                    }
                    else
                    {
                        anim.SetFloat("ActivationSpeed", speedFac * animationSpeedCurve.Evaluate(Mathf.Clamp01(deltaPlayTime / playTime)));
                        obelisk_branch.SetFloat("_DissolveCutoff", Mathf.Clamp01(1 - (deltaPlayTime / playTime)));
                    }

                    if (deltaLDT > lightDissolveTime)
                    {
                        anim.enabled = false;
                        ResetObelisk(!invert);
                        yield break;
                    }
                    else
                    {
                        obelisk_branchLight.SetFloat("_DissolveCutoff", Mathf.Clamp01(1 - (deltaLDT / lightDissolveTime)));
                        obelisk_mask.SetFloat("_DissolveCutoff", Mathf.Clamp01(deltaLDT / lightDissolveTime));
                    }
                }
                else
                {
                    laser.SetActive(false);
                    if (deltaPlayTime >= playTime && !doneAnim)
                    {
                        ResetObelisk(!invert);
                        doneAnim = true;
                    }
                    else
                    {
                        anim.SetFloat("ActivationSpeed", speedFac * animationSpeedCurve.Evaluate(1 - Mathf.Clamp01(deltaPlayTime / playTime)));
                        obelisk_branch.SetFloat("_DissolveCutoff", Mathf.Clamp01(deltaPlayTime / playTime));
                    }

                    if (deltaLDT > lightDissolveTime)
                    {
                        anim.enabled = false;
                        ResetObelisk(!invert);
                        yield break;
                    }
                    else
                    {
                        obelisk_branchLight.SetFloat("_DissolveCutoff", Mathf.Clamp01(deltaLDT / lightDissolveTime));
                        obelisk_mask.SetFloat("_DissolveCutoff", Mathf.Clamp01(1 - (deltaLDT / lightDissolveTime)));
                    }
                }

                if (updateGI && branches.Count > 0)
                {
                    foreach (Renderer ren in branches)
                    {
                        Material[] mats = ren.sharedMaterials;
                        foreach (Material mat in mats)
                        {
                            try
                            {
                                mat.SetVector("_Dissolve_ObjectWorldPos", transform.position);
                            }
                            catch
                            {
                                continue;
                            }
                        }
                        RendererExtensions.UpdateGIMaterials(ren);
                    }
                }

                deltaPlayTime += Time.deltaTime;
                deltaLDT += Time.deltaTime;

                yield return null;
            }
        }
    }
}
