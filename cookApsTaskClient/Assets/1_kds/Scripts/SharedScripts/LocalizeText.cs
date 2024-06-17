using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocalizeText : MonoBehaviour
{
    public List<ELanguageTable> eLanguageList;
    private TMP_Text _text;
    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
        if(eLanguageList.Count> 0 )
        {
            _text.text = string.Empty;
            for (int i = 0; i < eLanguageList.Count; i++)
            {
                _text.text += (eLanguageList[i]).LocalIzeText();
                if (i != eLanguageList.Count - 1)
                {
                    _text.text += "\n";
                }

            }
        }
       
    }
}
