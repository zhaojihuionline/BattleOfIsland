#ifndef _CE_CHAR_EXTRA_INCLUDED
#define _CE_CHAR_EXTRA_INCLUDED

half3 ApplyExtraEmission(float2 uv, half3 normalWS, half3 viewDirWS) {
#if defined(_ENABLE_FRESNEL_ON) || defined(_ENABLE_FROZEN_ON) || defined(_ENABLE_BEATEN_RIM_ON)
    float ndotv = saturate(dot(normalWS, viewDirWS));
#endif // defined(_ENABLE_FRESNEL_ON) || defined(_ENABLE_FROZEN_ON) || (_ENABLE_BEATEN_RIM_ON)
                    
    half3 extra = half3(0, 0, 0);
    half frozen = 0;
#ifdef _ENABLE_FROZEN_ON
    half3 texFrozen = (SAMPLE_TEXTURE2D(_FrozenTex, sampler_FrozenTex, TRANSFORM_TEX(uv, _FrozenTex))).rgb * _FrozenColor;
    frozen = _FrozenIntensity * pow( 1.0 - ndotv, _FrozenRange );
    extra += lerp(half3(0, 0, 0), texFrozen, frozen);
#endif // _ENABLE_FROZEN_ON

#if defined(_ENABLE_BEATEN_RIM_ON)
    extra *= (1 - step(frozen, 0)) * 0.5;
    extra += ( _BeatenRimIntensity * pow( 1.0 - ndotv, _BeatenRimRange ) * _BeatenRimColor );
#elif defined(_ENABLE_FRESNEL_ON)
    extra += ( _FresnelIntensity * pow( 1.0 - ndotv, _FresnelRange ) * _FresnelColor );
#endif // _ENABLE_FRESNEL_ON || _ENABLE_BEATEN_RIM_ON
    return extra;
}

#endif // _CE_CHAR_EXTRA_INCLUDED