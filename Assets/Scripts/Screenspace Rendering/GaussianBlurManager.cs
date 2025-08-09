using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;

using static SimulationParameters;
using UnityEngine.Rendering;

public class GaussianBlurManager
{

    // Make DepthWorldBlurRadius and DepthBlurIterationCount editable per instance but for testing don't have to since i mean it can be same
    static Material GaussianBlur1DMaterial = new Material(Shader.Find("Unlit/BilateralDepthBlur1D"));
    float kernelRadius = -1;

    public GaussianBlurManager()
    {
        CreateAndSetupGaussianKernel();
    }

    public void Blur(CommandBuffer cmd, RenderTexture src, RenderTexture firstPassRT, RenderTexture dest)
    {
        for (int i = 0; i < DepthBlurIterationCount; i++)
        {
            cmd.Blit(src, firstPassRT, GaussianBlur1DMaterial, 0);
            cmd.Blit(firstPassRT, dest, GaussianBlur1DMaterial, 1);

            src = dest;
        }        
    }

    public void CreateAndSetupGaussianKernel()
    {
        if (kernelRadius == DepthWorldBlurRadius)
        {
            Debug.Log("Aborting Gaussian Kernel Creation since SimulationParameters.WorldDepthBlurRadius is same as current radius");
            return;
        }

        kernelRadius = DepthWorldBlurRadius;
        CreateGaussianKernelTexture1D((int) ceil(DepthWorldBlurRadius));
        GaussianBlur1DMaterial.SetTexture("GaussianKernel", gaussianKernel1D);
        UniformAllParameters();
    }

    public void UniformAllParameters() {
        GaussianBlur1DMaterial.SetFloat("WorldKernelRadius", DepthWorldBlurRadius);
        GaussianBlur1DMaterial.SetFloat("DepthBlurBilateralFalloff", DepthBlurBilateralFalloff);
    }

    Texture2D gaussianKernel1D;
    void CreateGaussianKernelTexture1D(int radius)
    {
        // Init texture
        gaussianKernel1D = new(radius * 2 + 1, 1, TextureFormat.R16, false);
        float[] kernel = new float[radius * 2 + 1];

        // Compute kernel
        float sum = 0f;

        float sigma = max(radius * 0.5f, 1); // Radius is double sigma which is SD
        float sigma2 = sigma * sigma;
        for (int x = -radius; x <= radius; x++)
        {
            float val = exp(-x * x * 0.5f / sigma2) / (sigma * sqrt(2 * PI));
            int pixelLocation = x + radius;

            kernel[pixelLocation] = val;
            sum += val;
        }

        // Normalize
        for (int i = 0; i < kernel.Length; i++)
        {
            kernel[i] /= sum;
        }

        // Send to texture
        for (int i = 0; i < kernel.Length; i++)
        {
            gaussianKernel1D.SetPixel(i, 0, Color.white * kernel[i]);
        }

        // Send changes to GPU
        gaussianKernel1D.Apply();
    }

    Texture2D gaussianKernel2D;
    void CreateGaussianKernelTexture2D(int radius)
    {
        // Init texture
        gaussianKernel2D = new(radius * 2 + 1, radius * 2 + 1, TextureFormat.R16, false);
        float[,] kernel = new float[radius * 2 + 1, radius * 2 + 1];

        // Compute kernel
        float sum = 0f;

        float sigma = max(radius * 0.5f, 1); // Radius is double sigma which is SD
        float sigma2 = sigma * sigma;
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                float power = -0.5f * (x * x + y * y) * (1.0f / sigma2);
                float coeff = 1f / (2.0f * PI * sigma2);

                float gaussVal = coeff * exp(power);
                sum += gaussVal;

                kernel[x + radius, y + radius] = gaussVal;
            }
        }

        // Normalize
        for (int x = 0; x < kernel.Length; x++)
        {
            for (int y = 0; y < kernel.Length; y++)
            {
                kernel[x, y] /= sum;
            }
        }

        // Send to texture
        for (int x = 0; x < kernel.Length; x++)
        {
            for (int y = 0; y < kernel.Length; y++)
            {
                gaussianKernel2D.SetPixel(x, y, Color.white * kernel[x, y]);
            }
        }

        // Send changes to GPU
        gaussianKernel2D.Apply();
    }
}