using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UniversalHealthbar : MonoBehaviour, IInfobar
{
    private LivingThing target;

    public Vector3 worldOffset;
    public Vector3 UIOffset;

    public bool useGrid;

    private Image image_health;
    private Image image_health_fill;
    private Image image_health_delta;

    private Image image_shield;

    private Image image_health_HoT;
    private Image image_health_DoT;

    private CanvasGroup canvasGroup;

    private Image image_line;

    private Transform transform_grid;

    private List<Image> lines = new List<Image>();

    public Color enemyHealthbarColor;
    public Color ownHealthbarColor;
    public Color allyHealthbarColor;

    public void SetTarget(LivingThing target)
    {
        this.target = target;
    }

    private void Awake()
    {
        image_health = GetComponent<Image>();
        image_health_fill = transform.Find("Main/Fill").GetComponent<Image>();
        image_health_delta = transform.Find("Main/Delta").GetComponent<Image>();
        image_shield = transform.Find("Main/Fill/Shield").GetComponent<Image>();
        image_health_HoT = transform.Find("Main/HoT").GetComponent<Image>();
        image_health_DoT = transform.Find("Main/Fill/DoT").GetComponent<Image>();
        image_line = transform.Find("Grid/Line").GetComponent<Image>();
        image_line.enabled = false;
        transform_grid = transform.Find("Grid");
        canvasGroup = GetComponent<CanvasGroup>();

    }

    void LateUpdate()
    {
        if (target == null) return;

        UpdateGrid();
        UpdateFill();
    }
    private void UpdateGrid()
    {
        float maxAmount = target.maximumHealth + target.statusEffect.totalShieldAmount;
        float width = image_health.rectTransform.rect.width;
        int lineCount = (int)(maxAmount / 100f);

        transform_grid.gameObject.SetActive(useGrid);
        if (!useGrid) return;

        while(lines.Count > lineCount)
        {
            Destroy(lines[lines.Count - 1].gameObject);
            lines.RemoveAt(lines.Count - 1);
        }
        while(lines.Count < lineCount)
        {
            lines.Add(Instantiate(image_line.gameObject, image_line.transform.position, image_line.transform.rotation, image_line.transform.parent).GetComponent<Image>());
        }

        for(int i = 0; i < lines.Count; i++)
        {
            lines[i].enabled = true;
            lines[i].rectTransform.anchoredPosition = new Vector2(100f * (i + 1) / maxAmount * width, 0); 

        }
    }

    private void UpdateFill()
    {
        float maxAmount = target.maximumHealth + target.statusEffect.totalShieldAmount;
        float futureHealthDelta = target.statusEffect.totalHealOverTimeAmount - target.statusEffect.totalDamageOverTimeAmount;
        float healthWidth = image_health.rectTransform.rect.width;

        image_health_fill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (target.currentHealth + target.statusEffect.totalShieldAmount) / maxAmount * healthWidth);
        image_shield.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, target.statusEffect.totalShieldAmount / maxAmount * healthWidth);
        if (futureHealthDelta > 0)
        {
            image_health_HoT.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, futureHealthDelta / maxAmount * healthWidth);
            image_health_DoT.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
        }
        else
        {
            image_health_HoT.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
            image_health_DoT.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, -futureHealthDelta / maxAmount * healthWidth);
        }
        if(GameManager.instance.localPlayer == null)
        {
            image_health_fill.color = enemyHealthbarColor;
        }
        else
        {
            Relation relation = target.GetRelationTo(GameManager.instance.localPlayer);
            switch (relation)
            {
                case Relation.Ally:
                    image_health_fill.color = allyHealthbarColor;
                    break;
                case Relation.Enemy:
                    image_health_fill.color = enemyHealthbarColor;
                    break;
                case Relation.Own:
                    image_health_fill.color = ownHealthbarColor;
                    break;

            }
        }
        Color hotColor = image_health_fill.color;
        hotColor.a /= 6f;
        image_health_HoT.color = hotColor;

    }


}
