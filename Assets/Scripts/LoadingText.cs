using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LoadingText : MonoBehaviour
{
    [SerializeField] private TilesManager tilesManager;
    private Text m_text;

    private StringBuilder sb = new StringBuilder();

    private void Awake()
    {
        m_text = GetComponent<Text>();
    }

    void Update()
    {
        if (tilesManager.IsLoading) {
            sb.Clear();
            sb.Append("Loading: ");
            sb.Append(tilesManager.CurrentLoadingState);
            if (tilesManager.CurrentLoadingState == TilesManager.LoadingState.ComputeKeyframes) {
                sb.Append(' ');
                sb.Append(tilesManager.KeyframesProgress);
                sb.Append('/');
                sb.Append(tilesManager.KeyframesCount);
            }
            m_text.text = sb.ToString();
        } else {
            Destroy(gameObject);
        }
    }
}
