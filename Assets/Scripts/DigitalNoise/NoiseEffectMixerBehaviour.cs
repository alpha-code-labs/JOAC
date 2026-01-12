// NoiseEffectMixerBehaviour.cs
using UnityEngine;
using UnityEngine.Playables;

public class NoiseEffectMixerBehaviour : PlayableBehaviour
{
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        var renderer = playerData as Renderer;
        if (renderer == null) return;

        float totalWeight = 0f;
        float blendedIntensity = 0f;

        // Blend multiple noise clips if overlapping
        int inputCount = playable.GetInputCount();
        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            if (inputWeight > 0f)
            {
                var inputPlayable = (ScriptPlayable<NoiseEffectPlayableBehaviour>)playable.GetInput(i);
                var behaviour = inputPlayable.GetBehaviour();

                blendedIntensity += behaviour.intensity * inputWeight;
                totalWeight += inputWeight;
            }
        }

        // Apply blended effect
        if (totalWeight > 0f)
        {
            // Update noise effect based on blended values
        }
    }
}