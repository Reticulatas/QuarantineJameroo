Shader "Hidden/Shader/Pixelize"
{
    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        return output;
    }

    // List of properties to control your post process effect
    float _Intensity;
    TEXTURE2D_X(_InputTexture);

    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float depth = LoadCameraDepth(input.positionCS.xy);	
		depth = Linear01Depth(depth, _ZBufferParams);

		float3 outColor;

		if (depth > 0.03f)
		{
			float aspect = _ScreenSize.y / (float)_ScreenSize.x;
			float dx = _Intensity * (1.0 / _ScreenSize.x);
			float dy = _Intensity * (1.0 / _ScreenSize.y) * aspect;

			float2 newTexCoord = { dx * floor(input.texcoord.x / dx), dy * floor(input.texcoord.y / dy) };
			float2 positionSS = newTexCoord * _ScreenSize.xy;
			outColor = LOAD_TEXTURE2D_X(_InputTexture, positionSS).xyz;
		}
		else
		{
			outColor = LOAD_TEXTURE2D_X(_InputTexture, input.positionCS.xy).xyz;
		}

		//outColor = float4(depth, depth, depth, 1);
		return float4(outColor, 1);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "Pixelize"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment CustomPostProcess
                #pragma vertex Vert
            ENDHLSL
        }
    }
    Fallback Off
}
