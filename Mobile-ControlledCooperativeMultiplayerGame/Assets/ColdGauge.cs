using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColdGauge : MonoBehaviour
{
    GameObject goldText;

    // Start is called before the first frame update
    void Start()
    {
        //goldText = this.gameObject.transform.GetChild(0).gameObject; 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DrawColdCounter(int cold) {
        goldText = this.gameObject.transform.GetChild(0).gameObject;
        this.gameObject.SetActive(true);
        this.gameObject.transform.GetChild(0);

        TextMesh text = this.goldText.GetComponent<TextMesh>();
        text.text = cold.ToString();
        StartCoroutine(DeactiveCoroutine());
    }

    public IEnumerator DeactiveCoroutine() {
        yield return new WaitForSeconds(2f);
        this.gameObject.SetActive(false);
    }

}



