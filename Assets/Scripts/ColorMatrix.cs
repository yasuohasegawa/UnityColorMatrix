using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColorDeficiencyType
{
    Protanopia,
    Protanomaly,
    Deuteranopia,
    Deuteranomaly,
    Tritanopia,
    Tritanomaly,
    Achromatopsia,
    Achromatomaly
}

public class ColorMatrix {
    // RGB to Luminance conversion constants as found on
    // Charles A. Poynton's colorspace-faq:
    // http://www.faqs.org/faqs/graphics/colorspace-faq/

    private static float LUMA_R = 0.212671f;
    private static float LUMA_G = 0.71516f;
    private static float LUMA_B = 0.072169f;


    // There seem different standards for converting RGB
    // values to Luminance. This is the one by Paul Haeberli:

    private static float LUMA_R2 = 0.3086f;
    private static float LUMA_G2 = 0.6094f;
    private static float LUMA_B2 = 0.0820f;

    private static float RAD = Mathf.PI / 180f;

    public float[] matrix;

    private ColorMatrix preHue;
    private ColorMatrix postHue;
    private bool hueInitialized;

    public ColorMatrix()
    {
        matrix = new float[]{
            1f,0f,0f,0f,0f,
            0f,1f,0f,0f,0f,
            0f,0f,1f,0f,0f,
            0f,0f,0f,1f,0f};
    }

    public void concat(float[] mat)
    {
        float[] temp = new float[20];
        int i = 0;
        int x, y;
        for (y = 0; y < 4; y++)
        {
            for (x = 0; x < 5; x++)
            {
                temp[(i + x)] = mat[i] * matrix[x] +
                                          mat[(i + 1)] * matrix[(x + 5)] +
                                          mat[(i + 2)] * matrix[(x + 10)] +
                                          mat[(i + 3)] * matrix[(x + 15)] +
                                          (x == 4 ? mat[(i + 4)] : 0);
            }
            i += 5;
        }

        matrix = temp;

    }

    public void reset() {
        matrix[0] = 1f;
        matrix[1] = 0f;
        matrix[2] = 0f;
        matrix[3] = 0f;
        matrix[4] = 0f;
        matrix[5] = 0f;
        matrix[6] = 1f;
        matrix[7] = 0f;
        matrix[8] = 0f;
        matrix[9] = 0f;
        matrix[10] = 0f;
        matrix[11] = 0f;
        matrix[12] = 1f;
        matrix[13] = 0f;
        matrix[14] = 0f;
        matrix[15] = 0f;
        matrix[16] = 0f;
        matrix[17] = 0f;
        matrix[18] = 1f;
        matrix[19] = 0f;
    }

    public void invert()
    {
        concat(new float[]{ -1 ,  0,  0, 0, 1f,
                      0 , -1,  0, 0, 1f,
                      0 ,  0, -1, 0, 1f,
                      0,   0,  0, 1,   0});
    }

    public void adjustContrast(float r, float g = float.NaN, float b = float.NaN)
    {
        if (float.IsNaN(g)) g = r;
        if (float.IsNaN(b)) b = r;
        r += 1;
        g += 1;
        b += 1;

        concat(new float[]{r, 0, 0, 0, (128f * (1 - r))/255f,
                    0, g, 0, 0, (128f * (1 - g))/255f,
                    0, 0, b, 0, (128f * (1 - b))/255f,
                    0, 0, 0, 1, 0});
    }


    public void adjustBrightness(float r, float g = float.NaN, float b = float.NaN)
    {
        if (float.IsNaN(g)) g = r;
        if (float.IsNaN(b)) b = r;
        concat(new float[]{1, 0, 0, 0, r/255f,
                    0, 1, 0, 0, g/255f,
                    0, 0, 1, 0, b/255f,
                    0, 0, 0, 1, 0});
    }

    public void adjustSaturation(float s ){

        float sInv;
        float irlum;
        float iglum;
        float iblum;

        sInv = (1 - s);
        irlum = (sInv* LUMA_R);
        iglum = (sInv* LUMA_G);
        iblum = (sInv* LUMA_B);

        concat(new float[]{(irlum + s), iglum, iblum, 0, 0,
                irlum, (iglum + s), iblum, 0, 0,
                irlum, iglum, (iblum + s), 0, 0,
                0, 0, 0, 1, 0});

    }

    public void adjustSaturation(float r, float g = float.NaN, float b = float.NaN)
    {
        if (float.IsNaN(g)) g = r;
        if (float.IsNaN(b)) b = r;

        float srInv;
        float sgInv;
        float sbInv;
        float irlum;
        float iglum;
        float iblum;

        srInv = (1 - r);
        sgInv = (1 - g);
        sbInv = (1 - b);
        irlum = (srInv * LUMA_R);
        iglum = (sgInv * LUMA_G);
        iblum = (sbInv * LUMA_B);

        concat(new float[]{(irlum + r), iglum, iblum, 0, 0,
                irlum, (iglum + g), iblum, 0, 0,
                irlum, iglum, (iblum + b), 0, 0,
                0, 0, 0, 1, 0});

    }

    public void toGreyscale(float r, float g, float b)
 	{
        concat(new float[]{r, g, b, 0, 0,
                r, g, b, 0, 0,
                r, g, b, 0, 0,
                0, 0, 0, 1, 0});
    }

    public void luminance2Alpha()
    {
            concat(new float[]{0, 0, 0, 0, 1f,
                    0, 0, 0, 0, 1f,
                    0, 0, 0, 0, 1f,
                    LUMA_R, LUMA_G, LUMA_B, 0, 0});
    }

    public void colorize(int rgb, float amount = 1f)
    {
        int r;
        int g;
        int b;
        float inv_amount;
            
            r = (((rgb >> 16) & 0xFF) / 0xFF);
            g = (((rgb >> 8) & 0xFF) / 0xFF);
            b = ((rgb & 0xFF) / 0xFF);
            inv_amount = (1 - amount);
            
            concat(new float[]{(inv_amount + ((amount * (float)r) * LUMA_R)), ((amount * (float)r) * LUMA_G), ((amount * (float)r) * LUMA_B), 0, 0,
                    ((amount * (float)g) * LUMA_R), (inv_amount + ((amount * (float)g) * LUMA_G)), ((amount * (float)g) * LUMA_B), 0, 0,
                    ((amount * (float)b) * LUMA_R), ((amount * (float)b) * LUMA_G), (inv_amount + ((amount * (float)b) * LUMA_B)), 0, 0,
                    0, 0, 0, 1f, 0});
    }

    public void average(float r = 0.333333f, float g = 0.333333f, float b = 0.333333f)
    {
        concat(new float[]{r, g, b, 0, 0,
                r, g, b, 0, 0,
                r, g, b, 0, 0,
                0, 0, 0, 1, 0});
    }

    public void threshold(float threshold, float factor = 256f)
    {
        concat(new float[]{(LUMA_R * factor), (LUMA_G * factor), (LUMA_B * factor), 0, (-(factor) * threshold),
                (LUMA_R * factor), (LUMA_G * factor), (LUMA_B * factor), 0, (-(factor) * threshold),
                (LUMA_R * factor), (LUMA_G * factor), (LUMA_B * factor), 0, (-(factor) * threshold),
                0, 0, 0, 1, 0});
    }
        
    public void desaturate()
    {
        concat(new float[]{LUMA_R, LUMA_G, LUMA_B, 0, 0,
                LUMA_R, LUMA_G, LUMA_B, 0, 0,
                LUMA_R, LUMA_G, LUMA_B, 0, 0,
                0, 0, 0, 1, 0});
    }

    public void setAlpha(float alpha)
    {
        concat(new float[]{1, 0, 0, 0, 0,
                0, 1, 0, 0, 0,
                0, 0, 1, 0, 0,
                0, 0, 0, alpha, 0});
    }

    private void initHue()
	{
		float greenRotation = 39.182655f;

		if (!hueInitialized)
		{
			hueInitialized = true;
			preHue = new ColorMatrix();
            preHue.rotateRed( 45 );
			preHue.rotateGreen(-greenRotation );

			float[] lum = new float[] { LUMA_R2, LUMA_G2, LUMA_B2, 1.0f };

			preHue.transformVector(lum);

			float red = lum[0] / lum[2];
            float green = lum[1] / lum[2];

			preHue.shearBlue(red, green);

			postHue = new ColorMatrix();
            postHue.shearBlue( -red, -green);
			postHue.rotateGreen(greenRotation);
			postHue.rotateRed(- 45.0f );
		}
  	}

    public void rotateHue(float degrees)
    {
        initHue();
        concat(preHue.matrix );
        rotateBlue(degrees );
        concat(postHue.matrix );
    }

    public void rotateRed(float degrees )
    {
        rotateColor(degrees, 2, 1 );
    }

    public void rotateGreen(float degrees )
    {
        rotateColor(degrees, 0, 2 );
    }

    public void rotateBlue(float degrees )
    {
        rotateColor(degrees, 1, 0 );
    }

    private void rotateColor(float degrees, int x, int y )
    {
        	degrees *= RAD;
	        float[] mat = new float[]{
                                    1f,0f,0f,0f,0f,
                                    0f,1f,0f,0f,0f,
                                    0f,0f,1f,0f,0f,
                                    0f,0f,0f,1f,0f};
            mat[x + x * 5] = mat[y + y * 5] = Mathf.Cos(degrees );
			mat[y + x * 5] = Mathf.Sin(degrees );
			mat[x + y * 5] = -Mathf.Sin(degrees );
			concat(mat );
    }

    public void shearRed(float green, float blue )
    {
        shearColor( 0, 1, green, 2, blue );
    }

    public void shearGreen(float red, float blue )
    {
        shearColor( 1, 0, red, 2, blue );
    }

    public void shearBlue(float red, float green )
    {
        shearColor( 2, 0, red, 1, green );
    }

    private void shearColor(int x, int y1, float d1, int y2, float d2 )
    {
        float[] mat = new float[]{
                                    1f,0f,0f,0f,0f,
                                    0f,1f,0f,0f,0f,
                                    0f,0f,1f,0f,0f,
                                    0f,0f,0f,1f,0f};
        mat[y1 + x * 5] = d1;
	    mat[y2 + x * 5] = d2;
	    concat(mat );
    }

    public void applyColorDeficiency(string type )
	{
		// the values of this method are copied from http://www.nofunc.com/Color_Matrix_Library/ 
			
		switch (type )
		{
       		case "Protanopia":
       			concat(new float[] { 0.567f, 0.433f, 0, 0, 0, 0.558f, 0.442f, 0, 0, 0, 0, 0.242f, 0.758f, 0, 0, 0, 0, 0, 1, 0 });
       			break;
            case "Protanomaly":
                concat(new float[] { 0.817f, 0.183f, 0, 0, 0, 0.333f, 0.667f, 0, 0, 0, 0, 0.125f, 0.875f, 0, 0, 0, 0, 0, 1, 0 });
                break;
            case "Deuteranopia":
                concat(new float[] { 0.625f, 0.375f, 0, 0, 0, 0.7f, 0.3f, 0, 0, 0, 0, 0.3f, 0.7f, 0, 0, 0, 0, 0, 1, 0 });
               	break;
            case "Deuteranomaly":
                concat(new float[] { 0.8f, 0.2f, 0, 0, 0, 0.258f, 0.742f, 0, 0, 0, 0, 0.142f, 0.858f, 0, 0, 0, 0, 0, 1, 0 });
                break;
            case "Tritanopia":
                concat(new float[] { 0.95f, 0.05f, 0, 0, 0, 0, 0.433f, 0.567f, 0, 0, 0, 0.475f, 0.525f, 0, 0, 0, 0, 0, 1, 0 });
                break;
            case "Tritanomaly":
                concat(new float[] { 0.967f, 0.033f, 0, 0, 0, 0, 0.733f, 0.267f, 0, 0, 0, 0.183f, 0.817f, 0, 0, 0, 0, 0, 1, 0 });
                break;
            case "Achromatopsia":
                concat(new float[] { 0.299f, 0.587f, 0.114f, 0, 0, 0.299f, 0.587f, 0.114f, 0, 0, 0.299f, 0.587f, 0.114f, 0, 0, 0, 0, 0, 1, 0 });
               	break;
            case "Achromatomaly":
                concat(new float[] { 0.618f, 0.320f, 0.062f, 0, 0, 0.163f, 0.775f, 0.062f, 0, 0, 0.163f, 0.320f, 0.516f, 0, 0, 0, 0, 0, 1, 0 });
                break;
                
		}
    
	}


    public void transformVector(float[] values )
    {
        if (values.Length != 4) return;

        float r = values[0] * matrix[0] + values[1] * matrix[1] + values[2] * matrix[2] + values[3] * matrix[3] + matrix[4];
       	float g = values[0] * matrix[5] + values[1] * matrix[6] + values[2] * matrix[7] + values[3] * matrix[8] + matrix[9];
       	float b = values[0] * matrix[10] + values[1] * matrix[11] + values[2] * matrix[12] + values[3] * matrix[13] + matrix[14];
       	float a = values[0] * matrix[15] + values[1] * matrix[16] + values[2] * matrix[17] + values[3] * matrix[18] + matrix[19];

       	values[0] = r;
       	values[1] = g;
       	values[2] = b;
       	values[3] = a;
    }

}
