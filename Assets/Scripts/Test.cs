using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

    private Material mat;

    private int colorMatId;

	// Use this for initialization
	void Start () {
        mat = this.gameObject.GetComponent<MeshRenderer>().material;
        colorMatId = Shader.PropertyToID("_ColorMats");

        applyColorMatrixFilter();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void applyColorMatrixFilter()
    {
        ColorMatrix cmat = new ColorMatrix();
        cmat.applyColorDeficiency(ColorDeficiencyType.Achromatomaly.ToString());
        cmat.adjustBrightness(90f);
        mat.SetFloatArray(colorMatId, cmat.matrix);

        /*
        cmat.toGreyscale(0.5f, 0.5f, 0.5f);
        cmat.adjustBrightness(90f);
        */
    }
}
