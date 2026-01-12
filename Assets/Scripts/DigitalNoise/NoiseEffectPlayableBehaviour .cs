// Updated NoiseEffectPlayableBehaviour.cs for UI Image
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

[System.Serializable]
public class NoiseEffectPlayableBehaviour : PlayableBehaviour
{
    public float intensity = 1f;
    public float scale = 0.1f;
    public float speed = 1f;
    public Vector2 scrollDirection = Vector2.right;
    public Color noiseColor = Color.white;
    public NoiseBlendMode blendMode = NoiseBlendMode.Overlay;

    private Material noiseMaterial;
    private static readonly int IntensityProperty = Shader.PropertyToID("_Intensity");
    private static readonly int ScaleProperty = Shader.PropertyToID("_Scale");
    private static readonly int OffsetProperty = Shader.PropertyToID("_Offset");
    private static readonly int ColorProperty = Shader.PropertyToID("_NoiseColor");

    public override void OnPlayableCreate(Playable playable)
    {
        if (noiseMaterial == null)
        {
            Shader noiseShader = Shader.Find("Custom/PerlinNoiseTransition");
            if (noiseShader != null)
                noiseMaterial = new Material(noiseShader);
        }
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        // Handle both Image and Renderer components
        if (playerData is Image imageComponent)
        {
            ProcessImageComponent(playable, info, imageComponent);
        }
        else if (playerData is Renderer rendererComponent)
        {
            ProcessRendererComponent(playable, info, rendererComponent);
        }
    }

    private void ProcessImageComponent(Playable playable, FrameData info, Image image)
    {
        if (noiseMaterial == null) return;

        float time = (float)playable.GetTime();
        Vector2 offset = scrollDirection * time * speed;

        // Update shader properties
        noiseMaterial.SetFloat(IntensityProperty, intensity * info.weight);
        noiseMaterial.SetFloat(ScaleProperty, scale);
        noiseMaterial.SetVector(OffsetProperty, offset);
        noiseMaterial.SetColor(ColorProperty, noiseColor);

        // Apply material to UI Image
        image.material = noiseMaterial;

        // Control alpha through Image component
        Color imageColor = image.color;
        imageColor.a = intensity * info.weight;
        image.color = imageColor;
    }

    private void ProcessRendererComponent(Playable playable, FrameData info, Renderer renderer)
    {
        if (noiseMaterial == null) return;

        float time = (float)playable.GetTime();
        Vector2 offset = scrollDirection * time * speed;

        noiseMaterial.SetFloat(IntensityProperty, intensity * info.weight);
        noiseMaterial.SetFloat(ScaleProperty, scale);
        noiseMaterial.SetVector(OffsetProperty, offset);
        noiseMaterial.SetColor(ColorProperty, noiseColor);

        renderer.material = noiseMaterial;
    }
}