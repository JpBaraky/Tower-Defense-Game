Shader "Custom/InvisibleMask"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZWrite On
            ColorMask 0   // Do not write color, only depth
        }
    }
}