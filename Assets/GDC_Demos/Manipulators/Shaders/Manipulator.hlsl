//Simple Mask
float SphereMask(float3 position, float3 center, float radius, float hardness)
{
    return 1 - saturate((distance(position, center) - radius) / (1 - hardness));
}

//Slightly more complicated falloff function
float ComputeFalloff(float3 position, float3 center, float innerRadius, float outerRadius)
{
    float dist = distance(position, center);
    float t = (dist - innerRadius) / (outerRadius - innerRadius);
    return 1 - saturate(t);
}

float _Hardness;
float _Radius;
float4 _AnchorPosition;
float4x4 _TransformationMatrix;

//Single Manipulator
float3 ApplyManipulator(float3 position)
{
    float3 manipulatedPosition = mul(_TransformationMatrix, float4(position, 1)).xyz;
    
    float falloff = SphereMask(position, _AnchorPosition.xyz, _Radius, _Hardness);
    manipulatedPosition = lerp(position, manipulatedPosition, falloff);
    return manipulatedPosition;
}


//https://docs.unity3d.com/Packages/com.unity.shadergraph@6.9/manual/Rotate-About-Axis-Node.html
float3 RotateAboutAxis(float3 position, float3 axis, float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    float one_minus_c = 1.0 - c;

    axis = normalize(axis);
    float3x3 rot_mat = 
    {   one_minus_c * axis.x * axis.x + c, one_minus_c * axis.x * axis.y - axis.z * s, one_minus_c * axis.z * axis.x + axis.y * s,
        one_minus_c * axis.x * axis.y + axis.z * s, one_minus_c * axis.y * axis.y + c, one_minus_c * axis.y * axis.z - axis.x * s,
        one_minus_c * axis.z * axis.x - axis.y * s, one_minus_c * axis.y * axis.z + axis.x * s, one_minus_c * axis.z * axis.z + c
    };
    return mul(rot_mat, position).xyz;
}

const static int MAX_MANIPULATORS = 10;
uniform int _NumManipulators = 0;
uniform float4 _Rotations[MAX_MANIPULATORS];
uniform float4 _Translations[MAX_MANIPULATORS];
uniform half _ScaleFactors[MAX_MANIPULATORS];
uniform float4 _AnchorPositions[MAX_MANIPULATORS];
uniform half _Hardnesses[MAX_MANIPULATORS];
uniform half _Radii[MAX_MANIPULATORS];

float3 ApplyManipulators(float3 position)
{
    float3 totalOffset = float3(0, 0, 0);
    for (int i = 0; i < _NumManipulators; i++)
    {
        half scale = _ScaleFactors[i];
        float4 translation = _Translations[i];
        float4 rotation = _Rotations[i];
        half hardness = _Hardnesses[i];
        half radius = _Radii[i];
        float4 anchorPosition = _AnchorPositions[i];
        
        float falloff = ComputeFalloff(position, anchorPosition.xyz, radius * hardness, radius);

        position -= anchorPosition.xyz;
        float3 scaledPosition = position * scale * falloff;
        float3 rotatedPosition = RotateAboutAxis(position, normalize(rotation).xyz, length(rotation) * falloff) - position;
        float3 translatedPosition = translation.xyz * falloff;
        position += anchorPosition.xyz;

        totalOffset += translatedPosition + scaledPosition + rotatedPosition;
    }

    return totalOffset + position;
}
