Shader "HiveVolumeRenderer/Scalar Volume Direct"
{
    Properties
    {
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _Intensity("Intensity", Float) = 1
        _WindowMin ("Window Minimum", Float) = -1024
        _WindowMax ("Window Maximum", Float) = 3071
        _CutMin("Cutoff Minimum", Float) = -1024
        _CutMax("Cutoff Maximum", Float) = 3071
        _CullDistance("Cull Distance", Float) = 0.15
    }
        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            LOD 100
            Cull Front
            ZTest LEqual
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
                CGPROGRAM

                #pragma multi_compile __ CUTOUT_ON
                #pragma multi_compile __ MASK_ON

                #pragma vertex vertex
                #pragma fragment fragment

                #include "UnityCG.cginc"

                struct VertexData
                {
                    float4 position : POSITION;
                    float4 normal : NORMAL;
                    float2 uv : TEXCOORD0;
                };

                struct FragmentInputData
                {
                    float4 screenSpacePosition : SV_POSITION;
                    float3 objectSpacePosition : TEXCOORD1;
                    float3 vertexToCamera : TEXCOORD2;
                    float3 normal : NORMAL;
                    float2 uv : TEXCOORD0;
                };

                struct FragmentOutputData
                {
                    float4 color : SV_TARGET;
                    float depth : SV_DEPTH;
                };

                sampler3D _DataTex;
                sampler2D _NoiseTex;
                sampler2D _TFTex;

                float _Dimensions[3];

                float _Intensity;
                float _WindowMin;
                float _WindowMax;
                float _CutMin;
                float _CutMax;
                float _CullDistance;

                #ifdef CUTOUT_ON

                    #define CUTOUT_TYPE_PLANE 1 
                    #define CUTOUT_TYPE_BOX_INCL 2 
                    #define CUTOUT_TYPE_BOX_EXCL 3
                    #define CUTOUT_TYPE_SPHERE_INCL 4
                    #define CUTOUT_TYPE_SPHERE_EXCL 5

                    float _CutoutTypes[8];
                    float4x4 _CutoutMatrices[8];
                    int _CutoutToolCount;

                    bool PositionCulledByCutoutTool(float3 position)
                    {
                        // Move the reference in the middle of the mesh, like the pivot
                        float4 pivotPos = float4(position - float3(0.5f, 0.5f, 0.5f), 1.0f);

                        bool clipped = false;
                        for (int i = 0; i < _CutoutToolCount && !clipped; ++i)
                        {
                            const int type = (int)_CutoutTypes[i];
                            const float4x4 mat = _CutoutMatrices[i];

                            // Convert from model space to plane's vector space
                            float3 planeSpacePos = mul(mat, pivotPos);
                            if (type == CUTOUT_TYPE_PLANE)
                                clipped = planeSpacePos.z > 0.0f;
                            else if (type == CUTOUT_TYPE_BOX_INCL)
                                clipped = !(planeSpacePos.x >= -0.5f && planeSpacePos.x <= 0.5f && planeSpacePos.y >= -0.5f && planeSpacePos.y <= 0.5f && planeSpacePos.z >= -0.5f && planeSpacePos.z <= 0.5f);
                            else if (type == CUTOUT_TYPE_BOX_EXCL)
                                clipped = planeSpacePos.x >= -0.5f && planeSpacePos.x <= 0.5f && planeSpacePos.y >= -0.5f && planeSpacePos.y <= 0.5f && planeSpacePos.z >= -0.5f && planeSpacePos.z <= 0.5f;
                            else if (type == CUTOUT_TYPE_SPHERE_INCL)
                                clipped = length(planeSpacePos) > 0.5;
                            else if (type == CUTOUT_TYPE_SPHERE_EXCL)
                                clipped = length(planeSpacePos) < 0.5;
                        }
                        return clipped;
                    }

                #endif

                #ifdef MASK_ON
                    
                    // Masking not yet implemented

                    bool PositionCulledByMask(float3 position)
                    {
                        return false;

                        /*
                        const int3 currentCoordinates = int3(
                            round(position.x * _Dimensions[0]), 
                            round(position.y * _Dimensions[1]), 
                            round(position.z * _Dimensions[2])
                        );
                        const int voxelIndex = 
                            currentCoordinates.x 
                            + (currentCoordinates.y * _Dimensions[0]) 
                            + (currentCoordinates.z * _Dimensions[0] * _Dimensions[1]);
                        */
                    }

                #endif

                #define STEP_COUNT 512

                #define STEP_SIZE 1.732f / STEP_COUNT // 1.732 is the longest straight line that can be drawn in a unit cube

                // -------------------------------------------------------
                // Locates the fragment corresponding to the input vertex
                // -------------------------------------------------------
                FragmentInputData VertexToInputFragment(VertexData vertIn)
                {
                    FragmentInputData fragOut;

                    fragOut.screenSpacePosition = UnityObjectToClipPos(vertIn.position);
                    fragOut.objectSpacePosition = vertIn.position;
                    fragOut.vertexToCamera = ObjSpaceViewDir(vertIn.position);
                    fragOut.normal = UnityObjectToWorldNormal(vertIn.normal);
                    fragOut.uv = vertIn.uv;

                    return fragOut;
                }

                // -------------------------------------------------------
                // Raymarches towards the camera using the input fragment
                // and returns the output fragment to be rendered
                // -------------------------------------------------------
                FragmentOutputData DirectVolumeRendering(FragmentInputData fragIn)
                {
                    // Get a ray pointing from the vertex to the fragment
                    float3 origin = fragIn.objectSpacePosition + float3(0.5f, 0.5f, 0.5f);
                    float3 direction = normalize(fragIn.vertexToCamera);

                    // Create a small random offset in order to remove artifacts
                    origin = origin + (2.0f * direction / STEP_COUNT) * tex2D(_NoiseTex, float2(fragIn.uv.x, fragIn.uv.y)).r;

                    // March along the ray
                    float4 color = float4(0.0f, 0.0f, 0.0f, 0.0f);
                    float distanceToCamera = length(fragIn.vertexToCamera) - _CullDistance;
                    uint depth = 0;
                    for (uint step = 0; step < STEP_COUNT; step++)
                    {
                        const float distance = step * STEP_SIZE;
                        const float3 currentPosition = origin + (direction * distance);

                        // Break if marching outside the render cube
                        if (currentPosition.x < 0.0f
                        || currentPosition.x > 1.0f
                        || currentPosition.y < 0.0f
                        || currentPosition.y > 1.0f
                        || currentPosition.z < 0.0f
                        || currentPosition.z > 1.0f)
                            break;

                        // Break if viewing from inside the volume and the raycast has gone behind the camera
                        if (distance > distanceToCamera)
                            break;

                        #ifdef MASK_ON
                            if (PositionCulledByMask(currentPosition))
                                continue;
                        #endif

                        #ifdef CUTOUT_ON
                            if (PositionCulledByCutoutTool(currentPosition))
                                continue;
                        #endif

                        // Calculate the normalised value at the current position
                        const float density = tex3Dlod(_DataTex, float4(currentPosition, 0.0f));
                        const float value = clamp((density - _WindowMin) / (_WindowMax - _WindowMin), 0, 1);

                        // Convert this value to a color using the transfer function
                        float4 baseColor = tex2Dlod(_TFTex, float4(value, 0.0f, 0.0f, 0.0f));

                        // Hide any values outside the cutoff range
                        if (density < _CutMin || density > _CutMax)
                            baseColor.a = 0.0f;

                        // Combine this step's RGB with the previous step's RGB
                        color.rgb = baseColor.a * baseColor.rgb + (1.0f - baseColor.a) * color.rgb;

                        // Combine this step's alpha with the previous step's alpha
                        color.a = (baseColor.a + (1.0f - baseColor.a) * color.a);

                        // If this step's alpha is above a certain threshold, add to the depth buffer
                        if (baseColor.a > 0.15f)
                            depth = step;

                        // If this step is completely opaque there's no need to continue stepping
                        if (color.a > 1.0f)
                            break;
                    }

                    // Adjust for intensity
                    color.a *= _Intensity;

                    // Write the fragment output
                    FragmentOutputData fragOut;
                    fragOut.color = color;

                    if (depth != 0)
                    {
                        float3 localPosition = origin + direction * (depth * STEP_SIZE) - float3(0.5f, 0.5f, 0.5f);
                        float4 clipPosition = UnityObjectToClipPos(float4(localPosition, 1.0f));
                        
                        #if defined(SHADER_API_GLCORE) || defined(SHADER_API_OPENGL) || defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
                            fragOut.depth = (clipPosition.z / clipPosition.w) * 0.5 + 0.5;
                        #else
                            fragOut.depth = clipPosition.z / clipPosition.w;
                        #endif
                    }
                    else
                        fragOut.depth = 0;

                    return fragOut;
                }

                // ------------------------------------------------------
                // Called procedurally by ShaderLab
                // ------------------------------------------------------
                FragmentInputData vertex(VertexData vertIn)
                {
                    return VertexToInputFragment(vertIn);
                }

                // ------------------------------------------------------
                // Called procedurally by ShaderLab
                // ------------------------------------------------------
                FragmentOutputData fragment(FragmentInputData fragIn)
                {
                    return DirectVolumeRendering(fragIn);
                }

                ENDCG
            }
        }
}
